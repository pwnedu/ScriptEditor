using UnityEngine;
using UnityEditor;
using System;

namespace pwnedu.ScriptEditor
{
    public class FindReplaceDialogue : EditorWindow
    {
        #region Private Fields

        readonly string replace;
        string find;
        string windowName;
        string entryFieldResult;
        bool findOnly;

        #endregion

        public static Tuple<string, string> ShowDialogueWindow(string windowName, string find, bool findOnly)
        {
            #region Create Dialogue Window

            FindReplaceDialogue dialog = CreateInstance<FindReplaceDialogue>();
            dialog.titleContent = new GUIContent(windowName);

            dialog.minSize = new Vector2(320, 120);
            dialog.maxSize = new Vector2(320, 120);

            dialog.windowName = windowName;
            dialog.findOnly = findOnly;

            if (!string.IsNullOrEmpty(find))
            {
                dialog.find = find;
            }
            
            if (findOnly)
            {
                dialog.entryFieldResult = dialog.find;
            }
            else
            {
                dialog.entryFieldResult = dialog.replace;
            }

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

            if (!findOnly)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(20);
                EditorGUILayout.LabelField("Replace:", GUILayout.Width(75));
                entryFieldResult = EditorGUILayout.TextField(entryFieldResult, GUILayout.Width(200));
                GUILayout.EndHorizontal();
            }

            GUILayout.FlexibleSpace();
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Cancel", GUILayout.Width(100)))
            {
                Cancelled();
            }

            if (GUILayout.Button(windowName, GUILayout.Width(100)))
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
            #region Accept

            if (findOnly && !string.IsNullOrEmpty(find))
            {
                entryFieldResult = find;
            }
            else if (!string.IsNullOrEmpty(replace))
            {
                entryFieldResult = replace;
            }

            if (!string.IsNullOrEmpty(entryFieldResult))
            {
                Debug.Log($"Accepted: F={find} R={replace} E={entryFieldResult}");
                Close();
            }

            #endregion
        }

        private void Cancelled()
        {
            #region Cancel

            entryFieldResult = string.Empty;
            Debug.Log($"Cancelled: F={find} R={replace} E={entryFieldResult}");
            Close();

            #endregion
        }
    }
}