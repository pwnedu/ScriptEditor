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

            //Debug.Log(path);
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
        
        public static string GetExtension(string referencePath, string fileName)
        {
            var path = Path.GetFileName(AssetDatabase.GetAssetPath(Selection.activeObject));
            var file = path.Replace(referencePath + fileName, "");
            var extension = file.Replace(fileName, "");
            //Debug.Log(extension);

            return extension;
        }

        public static void SelectFile(string filePath)
        {
            EditorUtility.FocusProjectWindow();
            UnityEngine.Object obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(filePath);
            Selection.activeObject = obj;
        }
    }
}