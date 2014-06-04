/*
 * +-------------------------------------------------------------------------------------+
 *  Project: FMOD_Unity
 *  Description: The logic behind the FmodReverbZone Custom Inspector.
 * 				 Handles only this small inspector. No spatial information is
 * 				 handled here, as this is let to the discretion of the collider
 * 				 used on the gameObject.
 *  Company: ISART Digital (www.isartdigital.com)
 *  Author: Galaad BROD (galaad.brod@gmail.com)
 * +-------------------------------------------------------------------------------------+
 */

#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(FmodReverbZone))]
public class FmodReverbZoneEditor : Editor
{
	public override void OnInspectorGUI() {
		serializedObject.Update();
		if (serializedObject != null && serializedObject.targetObject != null) {
			FmodReverbZone zone = serializedObject.targetObject as FmodReverbZone;
			FmodReverb reverb = zone.GetReverb();
			SerializedProperty fadeTimeProp = serializedObject.FindProperty("fadeTime");
			SerializedProperty priorityProp = serializedObject.FindProperty("m_priority");
			bool isGlobal = zone.IsGlobal();
			
			if (reverb != null) {
				EditorGUILayout.LabelField("Name", reverb.getName() + (isGlobal ? " (global reverb)" : ""));
				if (isGlobal) {
					GUI.enabled = false;
				}
				EditorGUILayout.Slider(fadeTimeProp, 0, 100, "Fade Time");
				EditorGUILayout.Slider(priorityProp, 0, 10000, "Priority");
				if (isGlobal) {
					GUI.enabled = true;
				}
				EditorGUILayout.Separator();
				EditorGUILayout.PrefixLabel("Parameters");
				EditorGUI.indentLevel += 1;
				GUI.enabled = false;
				EditorGUILayout.IntSlider("Master level", (int)((float)reverb.Room / 100), -100, 0);
				EditorGUILayout.IntSlider("Decay Time", (int)(reverb.DecayTime * 1000), 100, 20000);
				EditorGUILayout.Slider("HF Decay Ratio", reverb.DecayHFRatio, 0.1f, 2);
				EditorGUILayout.IntSlider("Pre Delay", (int)(reverb.ReflectionsDelay * 1000), 0, 300);
				EditorGUILayout.IntSlider("Late Delay", (int)(reverb.ReverbDelay * 1000), 0, 100);
				EditorGUILayout.Slider("Early Reflections", ((float)reverb.Reflections / 100), -100, 10);
				EditorGUILayout.Slider("Late Reflections", ((float)reverb.Reverb / 100), -100, 20);
				EditorGUILayout.IntSlider("Diffusion", (int)reverb.Diffusion, 0, 100);
				EditorGUILayout.IntSlider("Density", (int)reverb.Density, 0, 100);
				EditorGUILayout.Slider("HF Gain", ((float)reverb.RoomHF / 100), -100, 0);
				EditorGUILayout.Slider("LF Gain", ((float)reverb.RoomLF / 100), -100, 0);
				EditorGUILayout.IntSlider("HF Crossover", (int)reverb.HFReference, 20, 20000);
				EditorGUILayout.IntSlider("LF Crossover", (int)reverb.LFReference, 20, 1000);
				GUI.enabled = true;
				EditorGUI.indentLevel -= 1;
				
				serializedObject.ApplyModifiedProperties();				
			} else {
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.PrefixLabel("Name");
				EditorGUILayout.HelpBox("No Reverb is set", MessageType.Error);
				EditorGUILayout.EndHorizontal();
				GUI.enabled = false;
				EditorGUILayout.FloatField("Fade Time", 3);
				GUI.enabled = true;
				EditorGUILayout.Separator();
				EditorGUILayout.PrefixLabel("Parameters");
				GUI.enabled = false;
				EditorGUI.indentLevel += 1;
				EditorGUILayout.IntSlider("Master level", 0, -100, 0);
				EditorGUILayout.IntSlider("Decay Time", 100, 100, 20000);
				EditorGUILayout.Slider("HF Decay Ratio", 0.1f, 0.1f, 2);
				EditorGUILayout.IntSlider("Pre Delay", 0, 0, 300);
				EditorGUILayout.IntSlider("Late Delay", 0, 0, 100);
				EditorGUILayout.Slider("Early Reflections", -100, -100, 10);
				EditorGUILayout.Slider("Late Reflections", -100, -100, 20);
				EditorGUILayout.IntSlider("Diffusion", 0, 0, 100);
				EditorGUILayout.IntSlider("Density", 0, 0, 100);
				EditorGUILayout.Slider("HF Gain", -100, -100, 0);
				EditorGUILayout.Slider("LF Gain", -100, -100, 0);
				EditorGUILayout.IntSlider("HF Crossover", 20, 20, 20000);
				EditorGUILayout.IntSlider("LF Crossover", 20, 20, 1000);
				EditorGUI.indentLevel -= 1;
				GUI.enabled = true;
			}
		}
	}
}
#endif
