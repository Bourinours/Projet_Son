/*
 * +-------------------------------------------------------------------------------------+
 *  Project: FMOD_Unity
 *  Description: This script handles the import of .fev files and the creation of
 * 				 a FmodEventAsset next to that file. It also exits playmode as soon
 * 				 as a source file is imported, before Mono can rebuild and make us
 * 				 lose the handle to FMOD.
 *  Company: ISART Digital (www.isartdigital.com)
 *  Author: Galaad BROD (galaad.brod@gmail.com)
 * +-------------------------------------------------------------------------------------+
 */

#if UNITY_EDITOR
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System;
using System.IO;

public class FmodEventImportPostProcessor : AssetPostprocessor {
	
	static void UpdateEventAsset(FmodEventAsset oldAsset, FmodEventAsset newAsset) {
		FmodEventAudioSource[] allSources = Resources.FindObjectsOfTypeAll(typeof(FmodEventAudioSource)) as FmodEventAudioSource[];
		FmodReverbZone[] allReverbZones = Resources.FindObjectsOfTypeAll(typeof(FmodReverbZone)) as FmodReverbZone[];
		
		foreach (FmodEventAudioSource source in allSources) {
			source.CheckForOldFormat();
			if (source.getSource() != null) {
				FmodEvent oldEvent = source.getSource();
				FmodEvent newEvent = newAsset.getMatchingEvent(oldEvent);
				
				if (newEvent == null) {
					source.SetSourceEvent(null);
				} else {
					source.UpdateExistingEvent(newEvent);
				}
			}
		}
		foreach (FmodReverbZone zone in allReverbZones) {
			if (zone.GetReverb() != null) {
				FmodReverb oldReverb = zone.GetReverb();
				FmodReverb newReverb = newAsset.getMatchingReverb(oldReverb);
				
				if (newReverb == null) {
					zone.ResetReverb();
				} else {
					zone.ResetReverb();
					zone.SetReverb(newReverb);
				}
			}
		}
	}
	
	static void CheckForRemainsOfDeletedParameters() {
		FmodEventAudioSource[] allSources = Resources.FindObjectsOfTypeAll(typeof(FmodEventAudioSource)) as FmodEventAudioSource[];

		foreach (FmodEventAudioSource source in allSources) {
			source.CheckForRemainOfDeletedParameters();
		}		
	}
	
	static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths) {
		
		foreach (string str in importedAssets) {
			if (str.EndsWith(@".fev")) {
				if (File.Exists(str + @".asset")) {
					FmodEventAsset newAsset = FmodEventAsset.CreateInstance("FmodEventAsset") as FmodEventAsset;
					FmodEventAsset oldAsset = AssetDatabase.LoadAssetAtPath(str + @".asset", typeof(FmodEventAsset)) as FmodEventAsset;

					newAsset.Initialize(str);
					UpdateEventAsset(oldAsset, newAsset);
					newAsset.CreateAsset(str);
					AssetDatabase.SaveAssets();
					CheckForRemainsOfDeletedParameters();
					FmodEventSystem.ClearHolder();
				} else {
					
					FmodEventAsset asset = FmodEventAsset.CreateInstance("FmodEventAsset") as FmodEventAsset;
					asset.Initialize(str);
					asset.CreateAsset(str);
					FmodEventSystem.ClearHolder();
				}
			} else if (Application.isPlaying && Application.isEditor &&
				(str.EndsWith(@".cs") || str.EndsWith(@".js"))) {
				 #if UNITY_EDITOR
					EditorApplication.isPlaying = false; // we are forced to stop the application in the editor at this point, or else Mono will rebuild the app and we'll lose the managed memory. Unfortunately, the C++ application will still be running and we won't be able to come back.
					Debug.LogWarning("FMOD_Unity : Forced to leave the application because source code was about to be built");
				#endif
			}
		}
	}

	static void ERRCHECK(FMOD.RESULT result)
    {
        if (result != FMOD.RESULT.OK)
        {
            Debug.Log("FMOD_Unity: FmodEventImportPostProcessor: " + result + " - " + FMOD.Error.String(result));
         }
    }
}
#endif
