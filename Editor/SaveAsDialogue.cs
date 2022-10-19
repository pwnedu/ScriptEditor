using UnityEngine;
using UnityEditor;
using System;

namespace pwnedu.ScriptEditor
{
    public class SaveAsDialogue : EditorWindow
    {
        #region Private Fields

        string windowName;
        string extension;
        string saveAs;
        string entryFieldResult;
        int resultOption;
        Tuple<int, string> result;

        #endregion

        public static Tuple<int, string> ShowDialogueWindow(string windowName, string extension, string saveAs) 
        {
            #region Create Dialogue Window

            SaveAsDialogue dialog = CreateInstance<SaveAsDialogue>();
            dialog.titleContent = new GUIContent(windowName);

            dialog.minSize = new Vector2(320, 120);
            dialog.maxSize = new Vector2(320, 120);

            dialog.windowName = windowName;
            dialog.extension = extension;
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
            entryFieldResult = EditorGUILayout.TextField(entryFieldResult, GUILayout.Width(175));
            EditorGUILayout.LabelField(extension, GUILayout.Width(40));
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
            #region Cancel

            entryFieldResult = saveAs;
            result = new Tuple<int, string>(0, entryFieldResult);
            Debug.Log($"Cancelled: S={saveAs} E={entryFieldResult}");
            Close();

            #endregion
        }

        private void SaveAs()
        {
            #region Save File Ónly

            if (!string.IsNullOrEmpty(entryFieldResult))
            {
                result = new Tuple<int, string>(1, entryFieldResult);
                Debug.Log($"SaveAs: S={saveAs} E={entryFieldResult}");
                Close();
            }

            #endregion
        }

        private void SaveAsAndRenameClass()
        {
            #region Save File And Rename Class

            if (!string.IsNullOrEmpty(entryFieldResult))
            {
                result = new Tuple<int, string>(2, entryFieldResult);
                Debug.Log($"RenameClass: S={saveAs} E={entryFieldResult}");
                Close();
            }

            #endregion
        }
    }
}