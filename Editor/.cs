using System.IO;
using UnityEditor;
using UnityEngine;

public class ScriptEditor : EditorWindow
{
    //variables
    string referencePath = "Assets/Scripts/";
    readonly string defaultScript = "NewScript";
    readonly string extension = ".cs";
    string documentChanged = null;
    string renameFile = null;
    string fileName = null;
    string text = null;
    static int scriptId = 0;

    //layout
    Color headerColour = new Color(55f / 255f, 0f / 255f, 175f / 255f, 1f);
    Texture2D headerTexture;
    Rect headerSection;
    Rect saveIndicator;
    Vector2 scrollPos;

    [MenuItem("Tools/Script Editor %e")]
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
    }

    public void InitTextures()
    {
        headerTexture = new Texture2D(1, 1);
        headerTexture.SetPixel(0, 0, headerColour);
        headerTexture.Apply();
    }

    private void OnGUI()
    {
        DrawLayout();
        DrawHeader();
        DrawContent();
    }

    private void DrawLayout()
    {
        headerSection.x = 0;
        headerSection.y = 0;
        headerSection.width = Screen.width;
        headerSection.height = 20;

        saveIndicator.x = headerSection.width - 60;
        saveIndicator.y = headerSection.y + 0;
        saveIndicator.width = 58;
        saveIndicator.height = 20;
    }

    private void DrawHeader()
    {
        GUILayout.BeginArea(headerSection);
        if (headerTexture != null) { GUI.DrawTexture(headerSection, headerTexture); }
        GUILayout.Space(5);
        GUILayout.Label(referencePath + fileName + extension, EditorStyles.boldLabel);
        GUILayout.EndArea();

        GUILayout.BeginArea(saveIndicator);
        GUILayout.Space(1);
        GUILayout.Label(documentChanged, EditorStyles.helpBox);
        GUILayout.EndArea();
    }

    private void DrawContent()
    {
        GUILayout.FlexibleSpace();
        GUILayout.Space(20);
        scrollPos = GUILayout.BeginScrollView(scrollPos);
        EditorGUI.BeginChangeCheck();
        text = GUILayout.TextArea(text, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true)); //, GUILayout.MinWidth(300), GUILayout.MinHeight(400));
        if (EditorGUI.EndChangeCheck())
        {
            documentChanged = "not saved";
        }
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Save", GUILayout.Width(95), GUILayout.Height(20))) 
        {
            SaveScript();
            documentChanged = "saved";
        }
        if (GUILayout.Button("SaveAs", GUILayout.Width(95), GUILayout.Height(20)))
        {
            if (renameFile != "")
            {
                fileName = renameFile;
                SaveScript();
                documentChanged = "saved";
            }
            else
            {
                if (EditorUtility.DisplayDialog("Save Script As", "You need to set a file name first!", "Continue")) { }
                Debug.LogWarning("You need to set a file name first!");
            }
        }
        if (GUILayout.Button("Close", GUILayout.Width(95), GUILayout.Height(20)))
        {
            if (documentChanged == "not saved")
            { 
                if (EditorUtility.DisplayDialog("Close Script", "This will close the script without saving.\nAre you sure?", "Confirm")) 
                { 
                    this.Close(); 
                } 
            }
            else { this.Close(); }
        }
        if (GUILayout.Button("Delete", GUILayout.Width(95), GUILayout.Height(20)))
        {
            if (EditorUtility.DisplayDialog("Delete Script", "This will permanently delete the script.\nAre you sure?", "Confirm"))
            {
                DeleteScript();
                this.Close();
            }
        }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        renameFile = EditorGUILayout.TextField("Set New File Name", renameFile, GUILayout.ExpandWidth(true));
        GUILayout.EndHorizontal();

        EditorGUILayout.EndScrollView();
        GUILayout.FlexibleSpace();
    }

    private void OpenScript()
    {
        referencePath = UnityUtil.GetSelectedPath();
        fileName = UnityUtil.GetSelectedFile();

        //Read text in the file
        if (fileName != defaultScript)
        {
            StreamReader reader = new StreamReader(referencePath + fileName + extension);
            text = reader.ReadToEnd();
            reader.Close();
        } 
        else
        {
            scriptId++;
            fileName = defaultScript + scriptId;
            text = "using UnityEngine;\r\n\r\npublic class " + fileName + " : MonoBehaviour\r\n{\r\n\r\n}";
        }
    }

    private void SaveScript()
    {
        //Write some text to the file
        StreamWriter writer = new StreamWriter(referencePath + fileName + extension, false); //, encoding: Encoding.Default
        writer.Flush();
        writer.NewLine = "\r\n";
        writer.WriteLine(text);
        writer.Close();

        //Re-import the file to update the reference in the editor
        //AssetDatabase.ImportAsset(referencePath + fileName + extension);
        AssetDatabase.Refresh();
    }

    private void DeleteScript()
    {
        AssetDatabase.DeleteAsset(referencePath + fileName + extension);
    }

    public static class UnityUtil
    {
        public static string GetSelectedPath()
        {
            string path;

            path = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (path == "")
            {
                path = "Assets/Scripts/";
            }
            else if (Path.GetExtension(path) != "")
            {
                path = path.Replace(Path.GetFileName(AssetDatabase.GetAssetPath(Selection.activeObject)), "");
            }

            return path;
        }

        public static string GetSelectedFile()
        {
            string file;

            Object obj = Selection.activeObject;
            if (obj == null)
            {
                file = "NewScript";
                return file;
            }
            else
            {
                file = Path.GetFileName(obj.name);
                return file;
            }
        }
    }
}
