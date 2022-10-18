using UnityEngine;
using UnityEditor;
using System;

namespace pwnedu.ScriptEditor
{
    public class SaveAsDialogue : EditorWindow
    {
        string windowName;
        string saveAs;
        string entryFieldResult;
        int resultOption;
        Tuple<int, string> result;

        public static Tuple<int, string> ShowDialogueWindow(string windowName, string saveAs) 
        {
            #region Create Dialogue Window

            SaveAsDialogue dialog = CreateInstance<SaveAsDialogue>();
            dialog.titleContent = new GUIContent(windowName);

            dialog.minSize = new Vector2(320, 120);
            dialog.maxSize = new Vector2(320, 120);

            dialog.windowName = windowName;
            dialog.saveAs = saveAs;
            dialog.entryFieldResult = dialog.saveAs;
            dialog.result = new Tuple<int, string>(dialog.resultOption, dialog.saveAs);

            dialog.ShowModal();

            return dialog.result;

            #endregion
        }

        private void OnGUI()
        {
            #region Draw Window Layout

            GUILayout.Space(20);
            GUILayout.BeginHorizontal();
            GUILayout.Space(20);
            EditorGUILayout.LabelField($"{windowName}:", GUILayout.Width(75));
            entryFieldResult = EditorGUILayout.TextField(entryFieldResult, GUILayout.Width(180));
            EditorGUILayout.LabelField(".cs", GUILayout.Width(20));
            GUILayout.EndHorizontal();

            GUILayout.FlexibleSpace();
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Cancel", GUILayout.Width(100)))
            {
                Cancelled();
            }

            if (GUILayout.Button($"{windowName}", GUILayout.Width(100)))
            {
                SaveAs();
            }

            GUIContent content = new GUIContent("Rename Class", $"{windowName} and Rename Class");
            if (GUILayout.Button(content, GUILayout.Width(100)))
            {
                SaveAsAndRenameClass();
            }

            GUILayout.Space(5);
            GUILayout.EndHorizontal();
            GUILayout.Space(8);

            #endregion
        }

        private void Cancelled()
        {
            entryFieldResult = saveAs;
            result = new Tuple<int, string>(0, entryFieldResult);
            Debug.Log($"Cancelled: S={saveAs} E={entryFieldResult}");
            Close();
        }

        private void SaveAs()
        {
            if (!string.IsNullOrEmpty(entryFieldResult))
            {
                result = new Tuple<int, string>(1, entryFieldResult);
                Debug.Log($"SaveAs: S={saveAs} E={entryFieldResult}");
                Close();
            }
        }

        private void SaveAsAndRenameClass()
        {
            if (!string.IsNullOrEmpty(entryFieldResult))
            {
                result = new Tuple<int, string>(2, entryFieldResult);
                Debug.Log($"RenameClass: S={saveAs} E={entryFieldResult}");
                Close();
            }
        }
    }
}