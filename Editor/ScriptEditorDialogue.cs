//-----------------------------------------------------------------
//	Project	: #PROJECT_NAME#
//	Author	: #DEVELOPER_NAME#                    
//	Date	: #CREATION_DATE#
//-----------------------------------------------------------------

using UnityEngine;
using UnityEditor;

public class ScriptEditorDialogue : EditorWindow
{
	string entry;
	string entryFieldResult;

	// Add menu item to test this script.
	[MenuItem("Window/My Editor Window/ScriptEditorDialogue")]
	static void TestFromMenu()
	{
		ShowDialogueWindow("ScriptEditorDialogue", "Some Input");
    }

	public static string ShowDialogueWindow(string windowName, string entry)
	{
		#region Create Dialogue Window

		ScriptEditorDialogue dialog = CreateInstance<ScriptEditorDialogue>();
		dialog.titleContent = new GUIContent(windowName);

		dialog.minSize = new Vector2(320, 120);
		dialog.maxSize = new Vector2(320, 120);

		dialog.entry = entry;
		dialog.entryFieldResult = entry;

		dialog.ShowModal();

		return dialog.entryFieldResult;

		#endregion
	}

	private void OnGUI()
	{
		#region Draw Window Layout

		GUILayout.Space(20);
		GUILayout.BeginHorizontal();
		GUILayout.Space(20);
		entryFieldResult = EditorGUILayout.TextField(entryFieldResult, GUILayout.Width(280));
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

		GUILayout.EndHorizontal();

		#endregion
    }

	private void Accepted()
	{
		if (!string.IsNullOrEmpty(entryFieldResult))
		{
			Close();
		}
	}

	private void Cancelled()
	{
		entryFieldResult = entry;
		Close();
	}
}