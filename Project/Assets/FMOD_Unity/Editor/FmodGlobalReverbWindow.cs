/*
 * +-------------------------------------------------------------------------------------+
 *  Project: FMOD_Unity
 *  Description: Handles the code for the window displaying the global reverberation
 * 				 settings. This window is only available if a GlobalReverbZone object has
 * 				 been created using the menu "FMOD/Reverb/Create Global Reverb"
 *  Company: ISART Digital (www.isartdigital.com)
 *  Author: Galaad BROD (galaad.brod@gmail.com)
 * +-------------------------------------------------------------------------------------+
 */

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System;
using System.Collections;

public class FmodGlobalReverbWindow : EditorWindow {
	
	[MenuItem("FMOD/Reverb/Create Global Reverb (only outside of play)")]
	public static void CreateGlobalReverb() {
		if (!EditorApplication.isPlaying && !EditorApplication.isPaused) {
			GameObject.Instantiate(Resources.Load("GlobalReverbZone"));
		}
	}
	
	[MenuItem("FMOD/Reverb/Create Global Reverb (only outside of play)", true)]
	public static bool enableCreateGlobalReverb() {
		if (EditorApplication.isPlayingOrWillChangePlaymode) {
			return (false);
		}
		GlobalReverbZone obj = GameObject.FindObjectOfType(typeof(GlobalReverbZone)) as GlobalReverbZone;
		
		return (obj == null);
	}

	[MenuItem("FMOD/Reverb/Global Settings")]
	public static void showGlobalSettingsWindow() {
		EditorWindow.GetWindow(typeof(FmodGlobalReverbWindow));
	}

	[MenuItem("FMOD/Reverb/Global Settings", true)]
	public static bool enableGlobalSettingsWindow() {
		GlobalReverbZone obj = GameObject.FindObjectOfType(typeof(GlobalReverbZone)) as GlobalReverbZone;
		
		return (obj != null);
	}
		
	private ReverbSettings m_settings = null;
		
	public FmodGlobalReverbWindow() {
	}
	
	public void OnGUI() {
		if (m_settings == null) {
			m_settings = ReverbSettings.Get();
			if (m_settings == null)
				return ;
		}
		string[] presetsName = new string[m_settings.PRESETS.Length];
		
		for (int i = 0; i < m_settings.PRESETS.Length; i++) {
			presetsName[i] = m_settings.PRESETS[i].Name;
		}
		this.title = "Global Reverb settings";
		m_settings.SelectedPreset = EditorGUILayout.Popup("Reverb Preset", m_settings.SelectedPreset, presetsName);

		
		GUI.enabled = (m_settings.CurPreset.Name == "User");
		m_settings.CurPreset.Properties.Room = 100 * EditorGUILayout.IntSlider("Master level", (int)((float)m_settings.CurPreset.Properties.Room / 100), -100, 0);
		m_settings.CurPreset.Properties.DecayTime = ((float)EditorGUILayout.IntSlider("Decay Time", (int)(m_settings.CurPreset.Properties.DecayTime * 1000), 100, 20000)) / 1000.0F;
		m_settings.CurPreset.Properties.DecayHFRatio = EditorGUILayout.Slider("HF Decay Ratio", m_settings.CurPreset.Properties.DecayHFRatio, 0.1f, 2);
		m_settings.CurPreset.Properties.ReflectionsDelay = (float)EditorGUILayout.IntSlider("Pre Delay", (int)(m_settings.CurPreset.Properties.ReflectionsDelay * 1000), 0, 300) / 1000;
		m_settings.CurPreset.Properties.ReverbDelay = (float)EditorGUILayout.IntSlider("Late Delay", (int)(m_settings.CurPreset.Properties.ReverbDelay * 1000), 0, 100) / 1000;
		m_settings.CurPreset.Properties.Reflections = (int)(100 * EditorGUILayout.Slider("Early Reflections", ((float)m_settings.CurPreset.Properties.Reflections / 100), -100, 10));
		m_settings.CurPreset.Properties.Reverb = (int)(100 * EditorGUILayout.Slider("Late Reflections", ((float)m_settings.CurPreset.Properties.Reverb / 100), -100, 20));
		m_settings.CurPreset.Properties.Diffusion = EditorGUILayout.IntSlider("Diffusion", (int)m_settings.CurPreset.Properties.Diffusion, 0, 100);
		m_settings.CurPreset.Properties.Density = EditorGUILayout.IntSlider("Density", (int)m_settings.CurPreset.Properties.Density, 0, 100);
		m_settings.CurPreset.Properties.RoomHF = (int)(100 * EditorGUILayout.Slider("HF Gain", ((float)m_settings.CurPreset.Properties.RoomHF / 100), -100, 0));
		m_settings.CurPreset.Properties.RoomLF = (int)(100 * EditorGUILayout.Slider("LF Gain", ((float)m_settings.CurPreset.Properties.RoomLF / 100), -100, 0));
		m_settings.CurPreset.Properties.HFReference = EditorGUILayout.IntSlider("HF Crossover", (int)m_settings.CurPreset.Properties.HFReference, 20, 20000);
		m_settings.CurPreset.Properties.LFReference = EditorGUILayout.IntSlider("LF Crossover", (int)m_settings.CurPreset.Properties.LFReference, 20, 1000);

		if (GUI.enabled && GUI.changed) {
		}
		GUI.enabled = true;
		if (GUILayout.Button("Save Settings")) {
			m_settings = ReverbSettings.SaveSettings(m_settings);
		}
	}
}
#endif
