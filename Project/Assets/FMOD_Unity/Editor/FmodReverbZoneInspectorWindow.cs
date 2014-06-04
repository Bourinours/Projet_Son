/*
 * +-------------------------------------------------------------------------------------+
 *  Project: FMOD_Unity
 *  Description: This window displays all FmodReverbZones that the AudioListener is into
 * 				 and displays them, sorted by priority, for debugging. The top one is the
 * 				 current one.
 *  Company: ISART Digital (www.isartdigital.com)
 *  Author: Galaad BROD (galaad.brod@gmail.com)
 * +-------------------------------------------------------------------------------------+
 */

#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

public class FmodReverbZoneInspectorWindow : EditorWindow {
	
	[MenuItem("FMOD/Reverb/Active Reverb Zones")]
	public static void showGlobalSettingsWindow() {
		EditorWindow.GetWindow(typeof(FmodReverbZoneInspectorWindow));
	}
	
	public void OnGUI() {
		this.title = "Active Reverb Zones";
		if (Application.isPlaying == false) {
			EditorGUILayout.HelpBox("Application not playing", MessageType.Info, true);
			Repaint();
			return ;
		}
		List<FmodReverbZone> zoneStack = FmodEventSystem.GetReverbManager().GetReverbZoneStack();
		
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField("Zone name", GUILayout.Width(3 * this.position.width / 10));
		EditorGUILayout.LabelField("Reverb preset", GUILayout.Width(3 * this.position.width / 10));
		EditorGUILayout.LabelField("Priority", GUILayout.Width(2 * this.position.width / 10));
		EditorGUILayout.LabelField("Fade time", GUILayout.Width(2 * this.position.width / 10));
		EditorGUILayout.EndHorizontal();
		
		foreach (FmodReverbZone zone in zoneStack) {
			if (zone != null) {
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.LabelField(zone.name, GUILayout.Width(3 * this.position.width / 10));
				EditorGUILayout.LabelField(zone.GetReverb().getName(), GUILayout.Width(3 * this.position.width / 10));
				EditorGUILayout.LabelField(zone.Priority.ToString(), GUILayout.Width(2 * this.position.width / 10));
				EditorGUILayout.LabelField(zone.fadeTime.ToString(), GUILayout.Width(2 * this.position.width / 10));
				EditorGUILayout.EndHorizontal();				
			}
		}
		Repaint();
	}
}
#endif
