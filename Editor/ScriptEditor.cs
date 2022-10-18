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
        #region Private Fields

        // Variables
        string referencePath = "Assets/Scripts/";
        readonly string defaultScript = "NewScript";
        readonly string extension = ".cs";
        readonly string nl = Environment.NewLine;
        string documentChanged;
        string fixedLineBreaks;
        string find, replace;
        string fileName, renameFile;
        string codeText, revertText;

        int scriptId = 1;
        bool popup = false;

        // Layouts
        static Rect windowRect;
        static ScriptStyle styleData;
        Color headerColour = new Color32(60, 60, 180, 255);
        Color borderColour = new Color32(180, 120, 80, 255);
        Texture2D headerTexture;
        Texture2D borderTexture;
        Rect headerSection, bodySection, footerSection;
        Rect toolTip, buttonBar, saveIndicator;
        GUIStyle horizontalLine;
        GUIStyle editorStyle;

        Vector2 scrollPos;

        #endregion

        // Shortcut [Ctrl + Alt + E]
        [MenuItem("Tools/Script Editor/Open Editor %&e", priority = 2)] 
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
            revertText = codeText;
        }

        private void OnHierarchyChange()
        {
            InitTextures();
            Repaint();
        }

        //****************************************[ Initialise ]****************************************//

        public void InitTextures()
        {
            #region Initiate Textures and Find Style Data

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

            #endregion
        }

        public void SetStyle()
        {
            #region Initiate Styles

            if (styleData == null)
            {
                editorStyle = new GUIStyle()
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
                editorStyle = new GUIStyle(styleData.style.TextStyle);
            }

            horizontalLine = new GUIStyle();
            horizontalLine.normal.background = EditorGUIUtility.whiteTexture;
            horizontalLine.margin = new RectOffset(0, 0, 4, 4);
            horizontalLine.fixedHeight = 1;

            #endregion
        }

        private void OnGUI()
        {
            #region Draw Script Editor Window

            DrawLayout();
            DrawHeader();
            DrawContent();
            DrawFooter();
            DrawToolTip();
            DrawSaveAndCloseButtons();
            DrawSaveIndicator();

            #endregion

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
            #region Draw Layouts

            headerSection.x = 0;
            headerSection.y = 0;
            headerSection.width = position.width;
            headerSection.height = 20;

            toolTip = new Rect(headerSection.width - 197f, 1f, 10f, headerSection.height);
            saveIndicator = new Rect(headerSection.width - 68, headerSection.y + 0, 66f, headerSection.height);
            buttonBar = new Rect(headerSection.width - 186, 2f, 125f, headerSection.height);
            windowRect = new Rect(buttonBar.x, headerSection.height, 150, 200);

            bodySection.x = headerSection.x;
            bodySection.y = headerSection.height;
            bodySection.width = headerSection.width;
            bodySection.height = position.height - headerSection.height - footerSection.height;

            footerSection.x = headerSection.x;
            footerSection.y = bodySection.height + headerSection.height;
            footerSection.width = bodySection.width;
            footerSection.height = 5;

            #endregion
        }

        //****************************************[ DrawGUI ]****************************************//

        private void DrawHeader()
        {
            #region Draw Header

            GUILayout.BeginArea(headerSection);
            if (headerTexture != null) { GUI.DrawTexture(headerSection, headerTexture); }
            GUILayout.Space(4);
            GUILayout.Label(" " + referencePath + fileName + extension, editorStyle); //, EditorStyles.boldLabel);
            GUILayout.EndArea();
           
            #endregion
        }

        private void DrawToolTip()
        {
            #region Draw Tooltip

            GUILayout.BeginArea(toolTip);
            GUILayout.Space(1);
            EditorGUILayout.LabelField(new GUIContent("?", "Basic Script Editor v1.0" + nl + nl +
                "Select the file you would like to edit" + nl + "and press Ctrl + Alt + E or open the editor" + nl + "in Tools > Script Editor." + nl + nl +
                "Create a new script by opening the" + nl + "Script Editor without a script selected." + nl + nl +
                "© BlitzKorp Pty Ltd " + DateTime.Now.Year));
            GUILayout.EndArea();

            #endregion
        }


        private void DrawSaveAndCloseButtons()
        {
            #region Draw Header Buttons

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

            #endregion
        }

        private void DrawSaveIndicator()
        {
            #region Draw Save Indication Box

            GUILayout.BeginArea(saveIndicator);
            GUILayout.Space(1);
            GUILayout.Label(documentChanged, EditorStyles.helpBox);
            GUILayout.EndArea();

            #endregion
        }

        private void DrawContent()
        {
            #region Draw Text Body

            if (borderTexture != null) { GUI.DrawTexture(bodySection, borderTexture); }
            GUILayout.BeginArea(bodySection);
            var space = 4;
            if (Vector2.Distance(Vector2.zero, scrollPos) > 0.1f) { space += 2; } //scrollPos != Vector2.zero
            GUILayout.Space(space);

            scrollPos = GUILayout.BeginScrollView(scrollPos);

            EditorGUI.BeginChangeCheck();
            if (popup)
            {
                GUILayout.Label(codeText, EditorStyles.textArea, GUILayout.MaxHeight(bodySection.height), GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            }
            else
            {
                codeText = GUILayout.TextArea(codeText, GUILayout.MaxHeight(bodySection.height), GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            }
            
            if (EditorGUI.EndChangeCheck())
            {
                documentChanged = "not saved";
            }
            EditorGUILayout.EndScrollView();
            
            GUILayout.EndArea();

            #endregion
        }

        private void DrawFooter()
        {
            #region Draw Footer

            if (borderTexture != null) { GUI.DrawTexture(footerSection, borderTexture); }
            GUILayout.BeginArea(footerSection);

            GUILayout.EndArea();

            #endregion
        }

        private void DrawPopupWindow(int unusedWindowID)
        {
            #region Popup Window Layout

            HorizontalLine(Color.grey);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Find Text"))
            {
                FindTextButton();
            }

            if (GUILayout.Button("Find & Replace"))
            {
                FindAndReplaceButton();
            }

            if (GUILayout.Button("Clear Text"))
            {
                ClearTextButton();
            }

            if (GUILayout.Button("Revert Text"))
            {
                RevertTextButton();
            }

            GUILayout.FlexibleSpace();
            HorizontalLine(Color.grey);
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Rename"))
            {
                RenameFileButton();
            }

            if (GUILayout.Button("Save As"))
            {
                SaveAsFileButton();
            }

            if (GUILayout.Button("Delete"))
            {
                DeleteFileButton();
            }
            GUILayout.FlexibleSpace();
            GUI.DragWindow();

            #endregion
        }

        //****************************************[ Popup Menu Buttons ]****************************************//

        private void FindTextButton()
        {
            #region Find Text Button Function

            popup = false;

            TextEditor editor = (TextEditor)GUIUtility.GetStateObject(typeof(TextEditor), GUIUtility.keyboardControl);

            if (!string.IsNullOrEmpty(editor.SelectedText))
            {
                find = editor.SelectedText;
            }

            var result = FindReplaceDialogue.ShowDialogueWindow("Find Text", find, true);

            find = result.Item1;

            if (string.IsNullOrEmpty(find)) { Debug.Log("find is null"); return; }

            Debug.Log($"Finding All Matches for {find} with {replace}.");

            var indexes = FindPosition(find);

            editor.selectIndex = indexes.Item1;
            editor.cursorIndex = indexes.Item2;

            #endregion
        }

        private void FindAndReplaceButton()
        {
            #region Find And Replace Text Button Function

            popup = false;

            TextEditor editor = (TextEditor)GUIUtility.GetStateObject(typeof(TextEditor), GUIUtility.keyboardControl);
            
            if (!string.IsNullOrEmpty(editor.SelectedText))
            {
                find = editor.SelectedText;
            }

            var result = FindReplaceDialogue.ShowDialogueWindow("Replace Text", find, false);

            find = result.Item1;
            replace = result.Item2;

            if (string.IsNullOrEmpty(find)) { Debug.Log("find is null"); return; }
            if (string.IsNullOrEmpty(replace)) { Debug.Log("replace is null"); return; }

            Debug.Log($"Replacing All Matches for {find} with {replace}.");

            codeText = codeText.Replace(find, replace);
            find = "";
            replace = "";
            documentChanged = "not saved";

            #endregion
        }

        private void ClearTextButton()
        {
            #region Clear Text Button Function

            popup = false;

            if (EditorUtility.DisplayDialog("Clear Text", "This will clear all text in the editor." + nl + "Are you sure?", "Confirm", "Cancel"))
            {
                codeText = "";
                documentChanged = "not saved";
                Debug.Log("Text cleared!");
            }

            #endregion
        }

        private void RevertTextButton()
        {
            #region Revert Text Button Function

            popup = false;

            if (EditorUtility.DisplayDialog("Revert Text", "This will revert any changed text back to the original since the start of the session." + nl + "Are you sure?", "Confirm", "Cancel"))
            {
                codeText = revertText;
                Debug.Log("Text reverted back to original.");
            }

            #endregion
        }

        private void RenameFileButton()
        {
            #region Rename File Button Function

            popup = false;

            var save = SaveAsDialogue.ShowDialogueWindow("Rename File", renameFile);
            var oldFile = fileName;

            if (save.Item2 != string.Empty)
            {
                renameFile = save.Item2;

                switch (save.Item1)
                {
                    case 0:
                        Debug.Log($"Cancelled Rename");
                        break;
                    case 1:
                        // Rename File
                        SaveAs();
                        DeleteScript(oldFile);
                        Debug.Log($"Rename File {renameFile}");
                        break;
                    // Rename File and Class
                    case 2:
                        codeText = new Regex(fileName).Replace(codeText, renameFile, 1); //Replace first occurrence only
                        SaveAs();
                        DeleteScript(oldFile);
                        Debug.Log($"Rename File and Class {renameFile}");
                        break;
                    default:
                        break;
                }
            }
            else if (save.Item1 != 0)
            {
                Debug.LogWarning("You need to set a file name before renaming your file!");
            }
            else
            {
                Debug.Log($"Cancelled Rename");
            }

            #endregion
        }

        private void SaveAsFileButton()
        {
            #region Save As File Button Function

            popup = false;

            var save = SaveAsDialogue.ShowDialogueWindow("Save As", renameFile);

            if (save.Item2 != string.Empty)
            {
                renameFile = save.Item2;

                switch (save.Item1)
                {
                    case 0:
                        Debug.Log($"Cancelled SaveAs");
                        break;
                    case 1:
                        // Save As New File
                        SaveAs();
                        Debug.Log($"Save As New File {renameFile}");
                        break;
                    // Save As New File and Rename Class
                    case 2:
                        codeText = new Regex(fileName).Replace(codeText, renameFile, 1); //Replace first occurrence only
                        SaveAs();
                        Debug.Log($"Save As New File and Rename Class {renameFile}");
                        break;
                    default:
                        break;
                }
            }
            else if (save.Item1 != 0)
            {
                Debug.LogWarning("You need to set a file name before renaming your file!");
            }
            else
            {
                Debug.Log($"Cancelled SaveAs");
            }

            #endregion
        }

        private void DeleteFileButton()
        {
            #region Delete File Button Function

            popup = false;

            if (EditorUtility.DisplayDialog("Delete File", "This will permanently delete your script and clear text in the editor." + nl + "Are you sure?", "Confirm", "Cancel"))
            {
                DeleteScript(fileName);
                codeText = string.Empty;
                documentChanged = "not saved";
                popup = false;
                Debug.Log("File deleted.");
            }

            #endregion
        }

        //****************************************[ Functions ]****************************************//

        private void OpenScript()
        {
            #region Open Script Function

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

            #endregion
        }

        private void SaveScript()
        {
            #region Save Script Function

            // Make sure line endings are fit for the environment. Finds all occurrence of \r\n or \n and replaces with nl
            fixedLineBreaks = Regex.Replace(codeText, @"\r\n?|\n", nl);

            // Write text to the file.
            StreamWriter writer = new StreamWriter(referencePath + fileName + extension, false, encoding: Encoding.UTF8);
            writer.Flush();
            writer.Write(fixedLineBreaks);
            writer.Close();

            //AssetDatabase.Refresh(); // Update the reference in the editor.
            AssetDatabase.ImportAsset(referencePath + fileName + extension); // Or re-import the file might be faster.
            ScriptEditorUtility.SelectFile(referencePath + fileName + extension);

            #endregion
        }

        private Tuple<int, int> FindPosition(string findPosition)
        {
            int start = codeText.IndexOf(findPosition);
            int end = start + findPosition.Length;

            return new Tuple<int,int>(start, end);
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

        //****************************************[ Style ]****************************************//

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