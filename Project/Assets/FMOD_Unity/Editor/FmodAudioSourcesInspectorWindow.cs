/*
 * +-------------------------------------------------------------------------------------+
 *  Project: FMOD_Unity
 *  Description: Handles the code for the window displaying all active audio sources.
 *  Company: ISART Digital (www.isartdigital.com)
 *  Author: Galaad BROD (galaad.brod@gmail.com)
 * +-------------------------------------------------------------------------------------+
 */

#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;  
using System.Collections;
using System.Collections.Generic;

public class FmodAudioSourcesInspectorWindow : EditorWindow {
	
	[MenuItem("FMOD/Active Audio Sources")]
	public static void showAudioSourcesWindow() {
		EditorWindow.GetWindow(typeof(FmodAudioSourcesInspectorWindow));
	}
	
	public void OnGUI() {
		this.title = "Active Audio Sources";
		if (Application.isPlaying == false) {
			EditorGUILayout.HelpBox("Application not playing", MessageType.Info, true);
			Repaint();
			return ;
		}
		List<FmodEventAudioSource> sources = FmodEventAudioSource.getAllAudioSources();
		
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField("Source name", GUILayout.Width(4 * this.position.width / 10));
		EditorGUILayout.LabelField("Event name", GUILayout.Width(3 * this.position.width / 10));
		EditorGUILayout.LabelField("Status", GUILayout.Width(2 * this.position.width / 10));
		EditorGUILayout.EndHorizontal();
		
		foreach (FmodEventAudioSource src in sources) {
			if (src != null) {
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.LabelField(src.name, GUILayout.Width(4 * this.position.width / 10));
				EditorGUILayout.LabelField((src.getSource() == null ? "(No event selected)" : src.getSource().getName()), GUILayout.Width(3 * this.position.width / 10));
				EditorGUILayout.LabelField(src.getStatus(), GUILayout.Width(2 * this.position.width / 10));
				EditorGUILayout.EndHorizontal();				
			}
		}
		Repaint();
	}
}
#endif
