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

        static EditorWindow editorWindow;
        static Rect popupWindow;
        static ScriptStyle styleData;

        const string toolPath = "Packages/com.kiltec.scripteditor/";
        const string menuItem = "Tools/Script Editor/";

        readonly string[] allowedExtensions = new string[6] { ".cs", ".csv", ".json", ".xml", ".txt", ".md" };
        readonly string defaultScript = "NewScript";
        readonly string nl = Environment.NewLine;

        // Variables
        string referencePath = "Assets/Scripts/";
        string documentChanged;
        string fixedLineBreaks;
        string find, replace;
        string fileName, renameFile;
        string codeText, revertText;
        string extension = ".cs";

        int scriptId = 1;
        int numberOfLines = 1;
        int findNextPos = 0;

        bool popup = false;
        bool lineDisplay = true;
        bool countDisplay = true;
        bool focus = true;

        // Layouts
        Color headerColour = new Color32(60, 60, 180, 255);
        Color borderColour = new Color32(180, 120, 80, 255);
        Color lineColour = Color.grey;
        Texture2D headerTexture;
        Texture2D borderTexture;
        Rect headerSection, bodySection, footerSection;
        Rect toolTip, buttonBar, saveIndicator;
        GUIStyle horizontalLine;
        GUIStyle editorStyle;
        GUIStyle numberStyle;
        GUIStyle footerStyle;

        Vector2 scrollPos;

        #endregion

        //****************************************[ Create Window ]****************************************//

        // Shortcut [Ctrl + Alt + E]
        [MenuItem(menuItem + "Open Editor %&e", priority = 2)] 
        public static void ShowWindow()
        {
            editorWindow = GetWindow(typeof(ScriptEditor));
            var res = Screen.currentResolution;
            var content = new GUIContent("Script Editor");
            editorWindow.titleContent = content;
            editorWindow.maxSize = new Vector2(res.width, res.height);
            editorWindow.minSize = new Vector2(360, 240);
        }

        [MenuItem(menuItem + "Script Editor Settings", priority = 11)]
        private static void AttributeSettings()
        {
            var path = $"{toolPath}Editor/Default Script Style.asset";

            if (!File.Exists(path)) { return; }

            var asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
            EditorGUIUtility.PingObject(asset);
            Selection.activeObject = asset;
        }

        [MenuItem(menuItem + "Script Editor Help", priority = 12)]
        private static void ScriptEditorHelp()
        {
            var path = $"{toolPath}README.md";

            if (!File.Exists(path)) { Debug.Log(path);  return; }

            var asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
            EditorGUIUtility.PingObject(asset);
            Selection.activeObject = asset;
        }

        //****************************************[ Initialise ]****************************************//

        private void Awake()
        {
            OpenOrCreateScript();
        }

        public void OnEnable()
        {
            InitTextures();
            SetStyle();
            numberOfLines = codeText.Split(Environment.NewLine).Length;
            revertText = codeText;
            focus = true;
        }

        private void OnHierarchyChange()
        {
            InitTextures();
            Repaint();
        }

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

            horizontalLine = new GUIStyle();
            horizontalLine.normal.background = EditorGUIUtility.whiteTexture;
            horizontalLine.margin = new RectOffset(0, 0, 4, 4);

            if (styleData == null)
            {
                editorStyle = new GUIStyle()
                {
                    normal = new GUIStyleState() { textColor = new Color32(125, 150, 200, 255) },
                    border = new RectOffset(0, 0, 0, 0),
                    padding = new RectOffset(4, 0, 0, 0),
                    alignment = TextAnchor.MiddleLeft,
                    clipping = TextClipping.Clip,
                    fontStyle = FontStyle.Bold,
                    fixedHeight = 20,
                    richText = true,
                    fontSize = 14
                };

                numberStyle = new GUIStyle(styleData.style.TextStyle)
                {
                    normal = new GUIStyleState() { textColor = Color.grey },
                    border = new RectOffset(0, 0, 0, 0),
                    padding = new RectOffset(0, 0, 0, 0),
                    alignment = TextAnchor.MiddleRight,
                    clipping = TextClipping.Clip,
                    fontStyle = FontStyle.Normal,
                    fixedHeight = EditorStyles.textArea.fixedHeight,
                    richText = true,
                    fontSize = 8
                };

                footerStyle = new GUIStyle()
                {
                    normal = new GUIStyleState() { textColor = Color.grey },
                    border = new RectOffset(0, 0, 0, 0),
                    padding = new RectOffset(0, 0, 0, 2),
                    alignment = TextAnchor.MiddleLeft,
                    clipping = TextClipping.Clip,
                    fontStyle = FontStyle.Normal,
                    fixedHeight = footerSection.height,
                    richText = true,
                    fontSize = 10
                };

                horizontalLine.fixedHeight = 1;
            }
            else
            {
                editorStyle = new GUIStyle(styleData.style.TextStyle);

                numberStyle = new GUIStyle(styleData.style.NumberStyle)
                {
                    fixedHeight = EditorStyles.textArea.fixedHeight
                };

                footerStyle = new GUIStyle(styleData.style.FooterStyle)
                {
                    fixedHeight = footerSection.height
                };

                horizontalLine.fixedHeight = styleData.style.lineHeight;
                lineColour = styleData.style.lineColor;
                lineDisplay = styleData.lineDisplay;
                countDisplay = styleData.countDisplay;
            }

            #endregion
        }

        private void OnGUI()
        {
            if (lineDisplay || countDisplay)
            {
                numberOfLines = codeText.Split('\n').Length;
            }

            #region Keyboard Input

            // Check first if key has been pressed
            var e = Event.current;

            if (e.type == EventType.KeyDown)
            {
                //Debug.Log(e.keyCode);

                switch (e.keyCode)
                {
                    case KeyCode.Escape:
                        CloseWindow();
                        //Debug.Log("Close");
                        break;
                    case KeyCode.Tab:
                        TabSpace();
                        //Debug.Log("Tabbed");
                        break;
                    case KeyCode.F1:
                        ScriptEditorHelp();
                        //Debug.Log("Show Help");
                        break;
                    case KeyCode.F2:
                        SaveScript();
                        Debug.Log("Quick Save");
                        break;
                    case KeyCode.F11:
                        //window.maximized = true;
                        //Debug.Log("Maximised");
                        break;
                    case KeyCode.F12:
                        AttributeSettings();
                        //Debug.Log("Settings");
                        break;
                }
            }

            #endregion

            #region Draw Script Editor Window

            DrawLayout();
            DrawHeader();
            DrawContent();
            DrawFooter();
            DrawToolTip();
            DrawButtonBar();
            DrawSaveIndicator();

            #endregion

            #region Popup Window

            if (popup)
            {
                BeginWindows();

                // All Popup Windows must come inside here.
                popupWindow = GUILayout.Window(1, popupWindow, DrawPopupWindow, "Options Menu");

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

            toolTip = new Rect(headerSection.width - 197, headerSection.y, 10, headerSection.height);
            saveIndicator = new Rect(headerSection.width - 68, headerSection.y, 66, headerSection.height);
            buttonBar = new Rect(headerSection.width - 186, 1, 125, headerSection.height);
            popupWindow = new Rect(buttonBar.x, headerSection.height, 150, 225);

            bodySection.x = headerSection.x;
            bodySection.y = headerSection.height;
            bodySection.width = headerSection.width;
            bodySection.height = position.height - headerSection.height - footerSection.height;

            footerSection.x = headerSection.x;
            footerSection.y = bodySection.height + headerSection.height;
            footerSection.width = bodySection.width;
            
            if (countDisplay == false)
            {
                footerSection.height = 5;
            }
            else
            {
                footerSection.height = 15;
            }

            #endregion
        }

        //****************************************[ Draw GUI ]****************************************//

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
            var tip = $"Script Editor by pwnedu\n\n(Ctrl + Alt + E)\t Open Editor\t{nl}(F1)\t\t Help{nl}(F2)\t\t Save{nl}(F12)\t\t Settings{nl}(ESC)\t\t Quit\n\n© BlitzKorp Pty Ltd {DateTime.Now.Year}";
            EditorGUILayout.LabelField(new GUIContent("?", tip));
            GUILayout.EndArea();

            #endregion
        }


        private void DrawButtonBar()
        {
            #region Draw Header Buttons

            GUILayout.BeginArea(buttonBar);
            GUILayout.BeginHorizontal();

            var previousBackgroundColor = GUI.backgroundColor;
            var previousColor = GUI.contentColor;

            if (styleData)
            {
                GUI.backgroundColor = styleData.style.menuButtonColor;
                GUI.contentColor = styleData.style.menuButtonTextColor;
            }

            if (GUILayout.Button("▼", EditorStyles.miniButton, GUILayout.Width(24), GUILayout.Height(16)))
            {
                popup = !popup;
            }

            if (GUILayout.Button("Save", EditorStyles.miniButton, GUILayout.Width(44), GUILayout.Height(16)))
            {
                SaveScript();
            }

            if (GUILayout.Button("Close", EditorStyles.miniButton, GUILayout.Width(44), GUILayout.Height(16)))
            {
                CloseWindow();
            }

            GUI.backgroundColor = previousBackgroundColor;
            GUI.contentColor = previousColor;

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

            GUILayout.BeginHorizontal();
            if (lineDisplay)
            {
                GUILayout.BeginVertical();

                var width = 12;
                if (numberOfLines > 10) { width = 16; }
                if (numberOfLines > 100) { width = 20; }
                if (numberOfLines > 1000) { width = 24; }

                for (int i = 1; i < numberOfLines + 1; i++)
                {
                    GUILayout.Label($"{i}", numberStyle, GUILayout.Width(width), GUILayout.Height(styleData.style.TextStyle.lineHeight));
                }
                GUILayout.EndVertical();
            }
            GUILayout.BeginVertical();
            if (popup)
            {
                GUILayout.Label(codeText, EditorStyles.textArea, GUILayout.MaxWidth(bodySection.width), GUILayout.ExpandWidth(true), GUILayout.MaxHeight(bodySection.height), GUILayout.ExpandHeight(true));
            }
            else
            {
                GUI.SetNextControlName("ScriptArea");

                codeText = GUILayout.TextArea(codeText, GUILayout.MaxWidth(bodySection.width), GUILayout.ExpandWidth(true), GUILayout.MaxHeight(bodySection.height), GUILayout.ExpandHeight(true));

                if (focus)
                {
                    GUI.FocusControl("ScriptArea");
                    focus = false;
                }
            }
            GUILayout.Space(5);
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();

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
            
            if (countDisplay)
            {
                GUILayout.BeginHorizontal();

                GUILayout.FlexibleSpace();
                GUILayout.Label($"Characters: {codeText.Length}", footerStyle, GUILayout.MaxHeight(bodySection.height), GUILayout.Width(110), GUILayout.ExpandHeight(true));
                GUILayout.Label($"Lines: {numberOfLines}", footerStyle, GUILayout.MaxHeight(bodySection.height), GUILayout.Width(80), GUILayout.ExpandHeight(true));

                GUILayout.EndHorizontal();
            }

            GUILayout.EndArea();

            #endregion
        }

        private void DrawPopupWindow(int unusedWindowID)
        {
            #region Popup Window Layout

            HorizontalLine(lineColour);

            var previousBackgroundColor = GUI.backgroundColor;
            var previousColor = GUI.contentColor;

            if (styleData)
            {
                GUI.backgroundColor = styleData.style.dropdownButtonColor;
                GUI.contentColor = styleData.style.dropdownButtonTextColor;
            }

            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Find"))
            {
                FindTextButton();
            }

            if (GUILayout.Button("Find Next"))
            {
                FindNextTextButton();
            }

            if (GUILayout.Button("Find & Replace"))
            {
                FindAndReplaceButton();
            }

            if (GUILayout.Button("Clear"))
            {
                ClearTextButton();
            }

            if (GUILayout.Button("Revert"))
            {
                RevertTextButton();
            }

            GUILayout.FlexibleSpace();
            HorizontalLine(lineColour);
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

            GUI.backgroundColor = previousBackgroundColor;
            GUI.contentColor = previousColor;

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

            find = result.Item2;

            if (string.IsNullOrEmpty(find))
            {
                Debug.Log("find is null");
                return;
            }

            var indexes = ScriptEditorUtility.HighlightPositions(codeText, find);

            editor.selectIndex = indexes.Item1;
            editor.cursorIndex = indexes.Item2;

            Debug.Log($"Finding matches for {find}.");
            Debug.Log($"Select From: {editor.selectIndex} Select To: {editor.cursorIndex}");

            find = string.Empty;

            #endregion
        }

        private void FindNextTextButton()
        {
            #region Find Text Button Function

            popup = false;

            TextEditor editor = (TextEditor)GUIUtility.GetStateObject(typeof(TextEditor), GUIUtility.keyboardControl);
            findNextPos = editor.cursorIndex;

            if (!string.IsNullOrEmpty(editor.SelectedText))
            {
                find = editor.SelectedText;
            }

            var result = FindReplaceDialogue.ShowDialogueWindow("Find Next", find, true);

            find = result.Item2;

            if (string.IsNullOrEmpty(find)) 
            { 
                Debug.Log("find is null"); 
                return; 
            }

            var indexes = ScriptEditorUtility.HighlightPositions(codeText, find, findNextPos);

            if (findNextPos >= codeText.Length) { findNextPos = 0; }
            if (findNextPos == editor.cursorIndex) { findNextPos = 0; } // editor cursor position maxes out at around 16,422 characters

            editor.selectIndex = indexes.Item1;
            editor.cursorIndex = indexes.Item2;

            Debug.Log($"Finding matches for {find}.");
            Debug.Log($"Select From: {editor.selectIndex} Select To: {editor.cursorIndex}");

            //findNextPos = indexes.Item2;
            
            find = string.Empty;

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

            Debug.Log($"Replacing All Matches for {find} and replaced with {replace}.");

            codeText = codeText.Replace(find, replace);
            find = string.Empty;
            replace = string.Empty;
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

            var save = SaveAsDialogue.ShowDialogueWindow("Rename File", extension, renameFile);
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

            var save = SaveAsDialogue.ShowDialogueWindow("Save As", extension, renameFile);

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

        private void TabSpace()
        {
            TextEditor editor = (TextEditor)GUIUtility.GetStateObject(typeof(TextEditor), GUIUtility.keyboardControl);
            codeText = codeText.Insert(editor.selectIndex, "\t");
        }
        private void OpenOrCreateScript()
        {
            #region Check For Supported Filetype and Open or Create Script

            bool loadFile = false;

            referencePath = ScriptEditorUtility.GetSelectedPath();
            fileName = ScriptEditorUtility.GetSelectedFile();

            var fileExtension = ScriptEditorUtility.GetExtension(referencePath, fileName);
            foreach (var fileType in allowedExtensions)
            {
                if (fileExtension == fileType) 
                {
                    extension = fileExtension;
                    loadFile = true;
                    break;
                }
            }

            if (fileName != defaultScript && loadFile) // Open the file.
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
                    // Now checking our extension prior, I don't think we will reach here anymore.
                    Debug.LogWarning("Incorrect file type. Script Editor only supports C# files, " + fileName + " is not a valid script file.");
                }
            }
            else // Create new file.
            {
                if (!referencePath.EndsWith("/"))
                {
                    if (Directory.Exists(referencePath))
                    {
                        referencePath += "/";
                    }
                    else //we have found a file without extension
                    {
                        int shorten = referencePath.LastIndexOf('/') + 1;
                        referencePath = referencePath.Substring(0, shorten);
                    }
                }

                fileName = defaultScript + scriptId;
                //Debug.Log(fileName);
                while (File.Exists(referencePath + fileName + extension))
                {
                    //Debug.Log("This file already exists: " + referencePath + fileName + extension); 
                    scriptId++;
                    fileName = defaultScript + scriptId;
                }
                if (!File.Exists(fileName))
                {
                    //Debug.Log(fileName + extension + " is available."); 
                    codeText = "using UnityEngine;" + nl + nl + "public class " + fileName + " : MonoBehaviour" + nl + "{" + nl + "\t" + nl + "}" + nl;
                    documentChanged = "not saved";
                }
            }

            #endregion
        }

        private void SaveScript()
        {
            #region Save Script and Focus Asset

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

            documentChanged = "saved";

            #endregion
        }

        private void SaveAs()
        {
            renameFile = renameFile.Replace(".cs", "");
            fileName = renameFile;
            SaveScript();
            renameFile = "";
        }

        private void DeleteScript(string file)
        {
            AssetDatabase.DeleteAsset(referencePath + file + extension);
        }

        private void CloseWindow()
        {
            #region Close Window Dialogue

            if (documentChanged == "not saved")
            {
                int result = EditorUtility.DisplayDialogComplex("Close Script", "This will close the script without saving." + nl + "Are you sure?", "Confirm", "Cancel", "Save And Close");
                
                if (result == 0) 
                { 
                    Debug.Log("Closed."); 
                    this.Close(); 
                }
                
                if (result == 1) 
                { 
                    Debug.Log("Cancelled."); 
                }
                
                if (result == 2) 
                {
                    Debug.Log("Saved before closing.");
                    SaveScript(); 
                    this.Close(); 
                }
            }
            else { this.Close(); }

            #endregion
        }

        //****************************************[ Style ]****************************************//

        private void HorizontalLine(Color color)
        {
            var c = GUI.backgroundColor;
            GUI.backgroundColor = color;
            GUILayout.Box(GUIContent.none, horizontalLine);
            GUI.backgroundColor = c;
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