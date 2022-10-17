using System.IO;
using UnityEditor;
using UnityEngine;

namespace pwnedu.ScriptEditor
{
    public static class ScriptEditorUtility
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

            GameObject gameObject = Selection.activeGameObject;
            UnityEngine.Object obj = Selection.activeObject;

            if (obj == null || gameObject)
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

        public static void SelectFile(string filePath)
        {
            EditorUtility.FocusProjectWindow();
            UnityEngine.Object obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(filePath);
            Selection.activeObject = obj;
        }
    }
}
