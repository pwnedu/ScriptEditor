using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace pwnedu.ScriptEditor
{
    public class ScriptEditor : EditorWindow
    {
        // Variables
        string referencePath = "Assets/Scripts/";
        readonly string defaultScript = "NewScript";
        readonly string extension = ".cs";
        readonly string nl = Environment.NewLine; // This automatically selects "\r\n" for win "\n" for mac
        string documentChanged = "";
        string find = "";
        string replace = "";
        string renameFile = "";
        string fileName = "";
        string codeText = "";
        string fixedLineBreaks = "";
        static int scriptId = 1;

        // Layouts
        private static ScriptStyle styleData;
        //Color headerColour = new Color(55f / 255f, 0f / 255f, 175f / 255f, 1f);
        Color headerColour = new Color32(60, 60, 180, 255);
        Color borderColour = new Color32(180, 120, 80, 255);
        Texture2D headerTexture;
        Texture2D borderTexture;
        Rect headerSection;
        Rect toolTip;
        Rect buttonBar;
        Rect saveIndicator;
        Rect windowRect;
        Rect bodySection;
        Rect footerSection;

        Vector2 scrollPos;
        GUIStyle horizontalLine;
        GUIStyle style;

        bool popup = false;
        bool showFind = false;
        bool showRename = false;
        bool showDelete = false;

        [MenuItem("Tools/Script Editor/Load Editor %&e", priority = 2)] // Shortcut [Ctrl + Alt + E]
        public static void ShowWindow()
        {
            GetWindow(typeof(ScriptEditor));
        }

        private void Awake()
        {
            OpenScript();
        }

        public void OnEnable()
        {
            InitTextures();
            SetStyle();
        }

        private void OnHierarchyChange() // Fixes missing texture after leaving play mode.
        {
            InitTextures();
            Repaint();
        }

        public void InitTextures()
        {
            FindStyleData();

            if (styleData != null)
            {
                headerColour = styleData.style.HeaderColor;
                borderColour = styleData.style.BackgroundColor;
            }

            headerTexture = new Texture2D(1, 1);
            headerTexture.SetPixel(0, 0, headerColour);
            headerTexture.Apply();

            borderTexture = new Texture2D(1, 1);
            borderTexture.SetPixel(0, 0, borderColour);
            borderTexture.Apply();
        }

        public void SetStyle()
        {
            if (styleData == null)
            {
                style = new GUIStyle()
                {
                    normal = new GUIStyleState() { textColor = new Color32(125, 150, 200, 255) },
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleLeft,
                    fixedHeight = 20,
                    richText = true,
                    fontSize = 14,
                };
            }
            else
            {
                style = new GUIStyle(styleData.style.TextStyle);
            }

            horizontalLine = new GUIStyle();
            horizontalLine.normal.background = EditorGUIUtility.whiteTexture;
            horizontalLine.margin = new RectOffset(0, 0, 4, 4);
            horizontalLine.fixedHeight = 1;
        }

        private void OnGUI()
        {
            DrawLayout();
            DrawHeader();
            DrawContent();
            DrawFooter();

            #region Popup Window

            if (popup)
            {
                BeginWindows();

                // All Popup Windows must come inside here.
                windowRect = GUILayout.Window(1, windowRect, DrawPopupWindow, "Options Menu");

                EndWindows();
            }

            #endregion
        }

        private void DrawLayout()
        {
            headerSection.x = 0;
            headerSection.y = 0;
            headerSection.width = position.width;
            headerSection.height = 20;

            toolTip = new Rect(headerSection.width - 197f, 1f, 10f, headerSection.height);
            saveIndicator = new Rect(headerSection.width - 68, headerSection.y + 0, 66f, headerSection.height);
            buttonBar = new Rect(headerSection.width - 186, 2f, 125f, headerSection.height);
            windowRect = new Rect(buttonBar.x, headerSection.height, 150, 100);

            bodySection.x = headerSection.x;
            bodySection.y = headerSection.height;
            bodySection.width = headerSection.width;
            bodySection.height = position.height - headerSection.height - footerSection.height;

            footerSection.x = headerSection.x;
            footerSection.y = bodySection.height + headerSection.height;
            footerSection.width = bodySection.width;
            footerSection.height = 5;
        }

        private void DrawHeader()
        {
            GUILayout.BeginArea(headerSection);
            if (headerTexture != null) { GUI.DrawTexture(headerSection, headerTexture); }
            GUILayout.Space(4);
            GUILayout.Label(" " + referencePath + fileName + extension, style); //, EditorStyles.boldLabel);
            GUILayout.EndArea();
           
            ToolTip();
            SaveAndCloseButtons();
            SaveIndicator();
        }

        private void DrawContent()
        {
            if (borderTexture != null) { GUI.DrawTexture(bodySection, borderTexture); }
            GUILayout.BeginArea(bodySection);
            var space = 4;
            if (Vector2.Distance(Vector2.zero, scrollPos) > 0.1f) { space += 2; } //scrollPos != Vector2.zero
            GUILayout.Space(space);

            scrollPos = GUILayout.BeginScrollView(scrollPos);

            EditorGUI.BeginChangeCheck();

            codeText = GUILayout.TextArea(codeText, GUILayout.MaxHeight(bodySection.height), GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            
            if (EditorGUI.EndChangeCheck())
            {
                documentChanged = "not saved";
            }
            EditorGUILayout.EndScrollView();
            
            GUILayout.EndArea();
        }

        private void DrawFooter()
        {
            if (borderTexture != null) { GUI.DrawTexture(footerSection, borderTexture); }
            GUILayout.BeginArea(footerSection);

            GUILayout.EndArea();
        }

        private void SubMenu()
        {
            // Display Sub Menu



            //HorizontalLine(Color.grey);
            GUILayout.Space(5);
            GUILayout.BeginHorizontal();
            
            // Contextual Menus
            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();
            GUILayout.Space(10);
            showRename = EditorGUILayout.Foldout(showRename, "Rename", true);
            showFind = EditorGUILayout.Foldout(showFind, "Replace", true);
            showDelete = EditorGUILayout.Foldout(showDelete, "Delete", true);
            
            GUILayout.EndHorizontal();
            if (showFind || showRename || showDelete) { EditorGUILayout.Space(10); }
            GUILayout.BeginHorizontal();
            
            // Rename Menu
            if (showRename)
            {
                RenameAndSaveContent();
            }
            
            // Find Menu
            if (showFind)
            {
                FindAndRepalceContent();
            }
            
            // Delete Menu
            if (showDelete)
            {
                ClearAndDeleteButtons();
            }
            
            GUILayout.EndHorizontal();
            if (showFind || showRename || showDelete) 
            {
                EditorGUILayout.Space(10); 
            }
            GUILayout.EndVertical();
            
            
            // Always On Display
            //SaveAndCloseButtons();
            
            GUILayout.EndHorizontal();
            EditorGUILayout.Space(2);
        }



        private void DrawPopupWindow(int unusedWindowID)
        {
            #region Popup Window Layout

            HorizontalLine(Color.grey);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Find"))
            {

            }
            if (GUILayout.Button("Rename"))
            {

            }
            if (GUILayout.Button("Delete"))
            {

            }
            GUILayout.FlexibleSpace();
            GUI.DragWindow();

            #endregion
        }

        private void OpenScript()
        {
            referencePath = ScriptEditorUtility.GetSelectedPath();
            fileName = ScriptEditorUtility.GetSelectedFile();

            if (fileName != defaultScript) // Open file.
            {
                try
                {
                    // Read text in the file.
                    StreamReader reader = new StreamReader(referencePath + fileName + extension);
                    codeText = reader.ReadToEnd();
                    reader.Close();
                }
                catch (FileNotFoundException)
                {
                    Debug.LogWarning("Incorrect file type. Script Editor only supports C# files, " + fileName + " is not a valid script file.");
                }
            }
            else // Create new file.
            {
                fileName = defaultScript + scriptId;
                while (File.Exists(referencePath + fileName + extension))
                {
                    //Debug.Log("This file already exists: " + referencePath + fileName + extension); 
                    scriptId++;
                    fileName = defaultScript + scriptId;
                }
                if (!File.Exists(fileName))
                {
                    //Debug.Log(fileName + extension + " is available."); 
                    codeText = "using UnityEngine;" + nl + nl + "public class " + fileName + " : MonoBehaviour" + nl + "{" + nl + nl + "}" + nl;
                    documentChanged = "not saved";
                }
            }
        }

        private void SaveScript()
        {
            // Make sure line endings are fit for the environment. Finds all occurrence of \r\n or \n and replaces with nl
            fixedLineBreaks = Regex.Replace(codeText, @"\r\n?|\n", nl);

            // Write text to the file.
            StreamWriter writer = new StreamWriter(referencePath + fileName + extension, false, encoding: Encoding.UTF8);
            writer.Flush();
            writer.Write(fixedLineBreaks);
            writer.Close();

            AssetDatabase.Refresh(); // Update the reference in the editor.
                                     //AssetDatabase.ImportAsset(referencePath + fileName + extension); // Or re-import the file might be faster.
            ScriptEditorUtility.SelectFile(referencePath + fileName + extension);
        }

        private void SaveAs()
        {
            renameFile = renameFile.Replace(".cs", "");
            fileName = renameFile;
            SaveScript();
            documentChanged = "saved";
            renameFile = "";
        }

        private void DeleteScript(string file)
        {
            AssetDatabase.DeleteAsset(referencePath + file + extension);
        }

        private void SaveAsOptions()
        {
            if (renameFile != "")
            {
                int options = EditorUtility.DisplayDialogComplex("Save As File Options", "How would you like to save the new file?", "Save As New File", "Cancel Save", "Save As New and Rename Class");

                switch (options)
                {
                    // Save As New File
                    case 0:
                        SaveAs();
                        break;
                    // Cancel Save As New File
                    case 1:
                        break;
                    // Save As New File and Rename Class
                    case 2:
                        codeText = new Regex(fileName).Replace(codeText, renameFile, 1); //Replace first occurrence only
                        SaveAs();
                        break;
                    default:
                        break;
                }
            }
            else
            {
                Debug.LogWarning("You need to set a file name before creating a new file!");
            }
        }

        private void RenameOptions()
        {
            if (renameFile != "")
            {
                int options = EditorUtility.DisplayDialogComplex("Rename File Options", "How would you like to rename the file?", "Rename File Only", "Cancel Rename", "Rename File and Class");
                var oldFile = fileName;
                switch (options)
                {
                    // Rename File Only
                    case 0:
                        SaveAs();
                        DeleteScript(oldFile);
                        break;
                    // Cancel Rename File
                    case 1:
                        break;
                    // Rename File and Class
                    case 2:
                        codeText = new Regex(fileName).Replace(codeText, renameFile, 1); //Replace first occurrence only
                        SaveAs();
                        DeleteScript(oldFile);
                        break;
                    default:
                        break;
                }
            }
            else
            {
                Debug.LogWarning("You need to set a file name before renaming your file!");
            }
        }

        private void ToolTip()
        {
            GUILayout.BeginArea(toolTip);
            GUILayout.Space(1);
            EditorGUILayout.LabelField(new GUIContent("?", "Basic Script Editor v1.0" + nl + nl +
                "Select the file you would like to edit" + nl + "and press Ctrl + Alt + E or open the editor" + nl + "in Tools > Script Editor." + nl + nl +
                "Create a new script by opening the" + nl + "Script Editor without a script selected." + nl + nl +
                "© BlitzKorp Pty Ltd " + DateTime.Now.Year));
            GUILayout.EndArea();
        }

        private void SaveAndCloseButtons()
        {
            GUILayout.BeginArea(buttonBar);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("▼", GUILayout.Width(22), GUILayout.Height(16)))
            {
                popup = !popup;
            }

            if (GUILayout.Button("Save", GUILayout.Width(44), GUILayout.Height(16)))
            {
                SaveScript();
                documentChanged = "saved";
            }

            if (GUILayout.Button("Close", GUILayout.Width(44), GUILayout.Height(16)))
            {
                if (documentChanged == "not saved")
                {
                    if (EditorUtility.DisplayDialog("Close Script", "This will close the script without saving." + nl + "Are you sure?", "Confirm"))
                    {
                        this.Close();
                    }
                }
                else { this.Close(); }
            }
            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }

        private void SaveIndicator()
        {
            GUILayout.BeginArea(saveIndicator);
            GUILayout.Space(1);
            GUILayout.Label(documentChanged, EditorStyles.helpBox);
            GUILayout.EndArea();
        }

        private void ClearAndDeleteButtons()
        {
            GUILayout.EndHorizontal();
            if (showFind || showRename) 
            { 
                //EditorGUILayout.Space(5); 
                //HorizontalLine(Color.grey); 
                //EditorGUILayout.Space(5);
            }

            GUILayout.BeginHorizontal();
            GUILayout.Label("Clear Text or Delete the File", EditorStyles.miniLabel);
            GUILayout.EndHorizontal();

            GUILayout.Space(5f);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Clear", GUILayout.Width(80), GUILayout.Height(20)))
            {
                if (EditorUtility.DisplayDialog("Clear Text", "This will clear all text." + nl + "Are you sure?", "Confirm"))
                {
                    codeText = "";
                    documentChanged = "not saved";
                }
            }
            if (GUILayout.Button("Delete", GUILayout.Width(80), GUILayout.Height(20)))
            {
                if (EditorUtility.DisplayDialog("Delete Script", "This will permanently delete the script." + nl + "Are you sure?", "Confirm"))
                {
                    DeleteScript(fileName);
                    this.Close();
                }
            }
        }

        private void FindAndRepalceContent()
        {
            GUILayout.EndHorizontal();
            if (showRename) 
            { 
                //EditorGUILayout.Space(5); 
                //HorizontalLine(Color.grey); 
                //EditorGUILayout.Space(5); 
            }

            GUILayout.BeginHorizontal();
            GUILayout.Space(22);
            GUILayout.Label("Find: ", EditorStyles.miniLabel);
            find = EditorGUILayout.TextField(find);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Space(4f);
            GUILayout.Label("Replace: ", EditorStyles.miniLabel);
            replace = EditorGUILayout.TextField(replace);
            GUILayout.EndHorizontal();

            GUILayout.Space(5f);
            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Replace All", GUILayout.Width(80), GUILayout.Height(20)))
            {
                if (find != "")
                {
                    if (EditorUtility.DisplayDialog("Find and Replace", "This will replace all instances of \'" + find + "\' with \'" + replace + "\'." + nl + "Are you sure?", "Confirm"))
                    {
                        string findAndReplace;
                        findAndReplace = codeText.Replace(find, replace);
                        codeText = findAndReplace;
                        find = "";
                        replace = "";
                        documentChanged = "not saved";
                    }
                }
                else
                {
                    Debug.LogWarning("You need to set the \"Find\" field before renaming!");
                }
            }
        }

        private void RenameAndSaveContent()
        {
            GUILayout.Label("New File Name: ", EditorStyles.miniLabel);
            renameFile = EditorGUILayout.TextField(renameFile);
            GUILayout.EndHorizontal();
            GUILayout.Space(5f);
            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Rename", GUILayout.Width(80), GUILayout.Height(20)))
            {
                RenameOptions();
            }
            if (GUILayout.Button("Save As", GUILayout.Width(80), GUILayout.Height(20)))
            {
                SaveAsOptions();
            }
        }

        private void HorizontalLine(Color color)
        {
            var c = GUI.color;
            GUI.color = color;
            GUILayout.Box(GUIContent.none, horizontalLine);
            GUI.color = c;
        }

        private static void FindStyleData()
        {
            var guids = AssetDatabase.FindAssets($"t:{typeof(ScriptStyle)}");
            if (guids.Length > 0)
            {
                var path = AssetDatabase.GUIDToAssetPath(guids[0]);
                styleData = AssetDatabase.LoadAssetAtPath<ScriptStyle>(path);
            }
        }
    }
}