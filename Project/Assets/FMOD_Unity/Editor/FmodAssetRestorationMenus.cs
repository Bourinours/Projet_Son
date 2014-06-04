#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections;

public class FmodAssetRestorationMenus : EditorWindow {
	[MenuItem("FMOD/Restoration/Update Asset Restoration Data")]
	public static void UpdateRestorationData() {
		foreach (UnityEngine.Object obj in Resources.FindObjectsOfTypeAll(typeof(FmodEventAudioSource))) {
			FmodEventAudioSource src = obj as FmodEventAudioSource;
			
			if (src != null) {
				src.UpdateRestorationData();
			}
		}
	}

	[MenuItem("FMOD/Restoration/Restore Lost References In Audio Sources")]
	public static void RestoreAssets() {
		foreach (UnityEngine.Object obj in Resources.FindObjectsOfTypeAll(typeof(FmodEventAudioSource))) {
			FmodEventAudioSource src = obj as FmodEventAudioSource;
			
			if (src != null) {
				src.RestoreAsset();
			}
		}		
	}
}
#endif