using UnityEngine;
using UnityEditor;
using System;
using GluonGui.Dialog;

namespace pwnedu.ScriptEditor
{
    public class FindReplaceDialogue : EditorWindow
    {
        string find;
        string replace;
        string entryFieldResult;
        Tuple<string, string> result;

        public static Tuple<string, string> ShowDialogueWindow(string windowName, string find)
        {
            #region Create Dialogue Window

            FindReplaceDialogue dialog = CreateInstance<FindReplaceDialogue>();
            dialog.titleContent = new GUIContent(windowName);

            dialog.minSize = new Vector2(320, 120);
            dialog.maxSize = new Vector2(320, 120);

            dialog.find = find;
            dialog.entryFieldResult = dialog.replace;

            dialog.ShowModal();

            return new Tuple<string, string> (dialog.find, dialog.entryFieldResult);

            #endregion
        }

        private void OnGUI()
        {
            #region Draw Window Layout

            GUILayout.Space(20);
            GUILayout.BeginHorizontal();
            GUILayout.Space(20);
            EditorGUILayout.LabelField("Find:", GUILayout.Width(75));
            find = EditorGUILayout.TextField(find, GUILayout.Width(200));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Space(20);
            EditorGUILayout.LabelField("Replace:", GUILayout.Width(75));
            entryFieldResult = EditorGUILayout.TextField(entryFieldResult, GUILayout.Width(200));
            GUILayout.EndHorizontal();

            GUILayout.FlexibleSpace();
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Cancel", GUILayout.Width(100)))
            {
                Cancelled();
            }

            if (GUILayout.Button("Okay", GUILayout.Width(100)))
            {
                Accepted();
            }
            GUILayout.Space(5);
            GUILayout.EndHorizontal();
            GUILayout.Space(8);

            #endregion
        }

        private void Accepted()
        {
            if (!string.IsNullOrEmpty(entryFieldResult))
            {
                Debug.Log($"Accepted: F={find} R={replace} E={entryFieldResult}");
                Close();
            }
        }

        private void Cancelled()
        {
            entryFieldResult = replace;
            Debug.Log($"Cancelled: F={find} R={replace} E={entryFieldResult}");
            Close();
        }
    }
}