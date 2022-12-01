using System.IO;
using UnityEditor;
using UnityEngine;

namespace pwnedu.ScriptEditor
{
    public class ScriptEditorMenu : MonoBehaviour
    {
        const string toolPath = "Packages/com.kiltec.scripteditor/";
        const string menuItem = "Tools/Script Editor/";

        // Shortcut [Ctrl + Alt + E]
        [MenuItem(menuItem + "Open Editor %&e", priority = 2)]
        public static void ShowWindow()
        {
            ScriptEditor.GetWindow(typeof(ScriptEditor));
        }

        [MenuItem(menuItem + "Script Editor Settings", priority = 11)]
        public static void ScriptEditorSettings()
        {
            EditorGUIUtility.PingObject(ScriptEditor.StyleData);
            Selection.activeObject = ScriptEditor.StyleData;
        }

        [MenuItem(menuItem + "Script Editor Help", priority = 12)]
        public static void ScriptEditorHelp()
        {
            var path = $"{toolPath}README.md";

            if (!File.Exists(path)) { Debug.Log(path); return; }

            var asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
            EditorGUIUtility.PingObject(asset);
            Selection.activeObject = asset;
        }
    }
}