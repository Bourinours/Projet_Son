/*
 * +-------------------------------------------------------------------------------------+
 *  Project: FMOD_Unity
 *  Description: The ReverbSettings are basically the global FmodReverb settings
 * 				 for a scene. They are held unto by the GlobalReverbZone, and saved
 * 				 in an asset next to the scene if modified during play, until they
 * 				 can be retrieved in a more durable form in the editor.
 *  Company: ISART Digital (www.isartdigital.com)
 *  Author: Galaad BROD (galaad.brod@gmail.com)
 * +-------------------------------------------------------------------------------------+
 */

#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using System.IO;
using System.Collections;

public class ReverbSettings : ScriptableObject {
	public static readonly FMOD.REVERB_PROPERTIES GENERIC_REVERB_PROPERTIES = new FMOD.REVERB_PROPERTIES(
		0,  0,     1.00f, -1000,  -100,   0,   1.49f,  0.83f, 1.0f,  -2602, 0.007f,    200, 0.011f,   0.25f, 0.000f, 5000.0f, 250.0f, 100.0f, 100.0f, 0x3f);
	public static readonly FMOD.REVERB_PROPERTIES OFF_REVERB_PROPERTIES = new FMOD.REVERB_PROPERTIES(
		0,  -1,     1.00f, -10000,  -10000,   0,   1f,  1f, 1.0f,  -2602, 0.007f,    200, 0.011f,   0.25f, 0.000f, 5000.0f, 250.0f, 0.0f, 0.0f, 0x33f);
	public static readonly FMOD.REVERB_PROPERTIES PSYCHOTIC_REVERB_PROPERTIES = new FMOD.REVERB_PROPERTIES(
		0,  25,    0.50f, -1000,  -151,   0,   7.56f,  0.91f, 1.0f,  -626,  0.020f,    774, 0.030f,   4.00f, 1.000f, 5000.0f, 250.0f, 100.0f, 100.0f, 0x1f);
	public static readonly FMOD.REVERB_PROPERTIES DEFAULT_REVERB_PROPERTIES = OFF_REVERB_PROPERTIES;
	
	[System.Serializable]
	public class FmodReverbPreset {
		[System.Serializable]
		public class PropertiesClass { // for serialization...
			public int Instance;
			public int Environment;
			public float EnvDiffusion;
			public int Room;
			public int RoomHF;
			public int RoomLF;
			public float DecayTime;
			public float DecayHFRatio;
			public float DecayLFRatio;
			public int Reflections;
			public float ReflectionsDelay;
			public int Reverb;
			public float ReverbDelay;
			public float ModulationTime;
			public float ModulationDepth;
			public float HFReference;
			public float LFReference;
			public float Diffusion;
			public float Density;
			public uint Flags;
			
			public PropertiesClass(FMOD.REVERB_PROPERTIES properties) {
				this.Instance = properties.Instance;
				this.Environment = properties.Environment;
				this.EnvDiffusion = properties.EnvDiffusion;
				this.Room = properties.Room;
				this.RoomHF = properties.RoomHF;
				this.RoomLF = properties.RoomLF;
				this.DecayTime = properties.DecayTime;
				this.DecayHFRatio = properties.DecayHFRatio;
				this.DecayLFRatio = properties.DecayLFRatio;
				this.Reflections = properties.Reflections;
				this.ReflectionsDelay = properties.ReflectionsDelay;
				this.Reverb = properties.Reverb;
				this.ReverbDelay = properties.ReverbDelay;
				this.ModulationTime = properties.ModulationTime;
				this.ModulationDepth = properties.ModulationDepth;
				this.HFReference = properties.HFReference;
				this.LFReference = properties.LFReference;
				this.Diffusion = properties.Diffusion;
				this.Density = properties.Density;
				this.Flags = properties.Flags;
			}
			public void CopyToProperties(ref FMOD.REVERB_PROPERTIES properties) {
				properties.Instance = this.Instance;
				properties.Environment = this.Environment;
				properties.EnvDiffusion = this.EnvDiffusion;
				properties.Room = this.Room;
				properties.RoomHF = this.RoomHF;
				properties.RoomLF = this.RoomLF;
				properties.DecayTime = this.DecayTime;
				properties.DecayHFRatio = this.DecayHFRatio;
				properties.DecayLFRatio = this.DecayLFRatio;
				properties.Reflections = this.Reflections;
				properties.ReflectionsDelay = this.ReflectionsDelay;
				properties.Reverb = this.Reverb;
				properties.ReverbDelay = this.ReverbDelay;
				properties.ModulationTime = this.ModulationTime;
				properties.ModulationDepth = this.ModulationDepth;
				properties.HFReference = this.HFReference;
				properties.LFReference = this.LFReference;
				properties.Diffusion = this.Diffusion;
				properties.Density = this.Density;
				properties.Flags = this.Flags;
			}
		}
		
		[SerializeField]
		private PropertiesClass m_serializedProperties = new PropertiesClass(OFF_REVERB_PROPERTIES);
		public FMOD.REVERB_PROPERTIES Properties = OFF_REVERB_PROPERTIES;
		public string Name = "Off";
		
		public void BeforeSerialization() {
			m_serializedProperties = new PropertiesClass(Properties);
		}
		public void AfterDeserialization() {
			m_serializedProperties.CopyToProperties(ref Properties);
		}
	}
	private static ReverbSettings LOADED_REVERB_SETTINGS = null;
	
	// -------------------------------------------------------------------
	
	
	public FmodReverbPreset[] PRESETS;
	public int SelectedPreset = 0;
	
	public FmodReverbPreset CurPreset {
		get {
			if (PRESETS == null || PRESETS.Length < SelectedPreset) {
				return (null);
			}
			return (PRESETS[SelectedPreset]);
		}
	}
	
	public static ReverbSettings Get() {
		LoadSettings();
		if (LOADED_REVERB_SETTINGS == null) {
			LOADED_REVERB_SETTINGS = Create();
			ReverbSettings.SaveSettings(LOADED_REVERB_SETTINGS);
		}
		return (LOADED_REVERB_SETTINGS);
	}
	
#if UNITY_EDITOR
	public static string GetSettingsAssetPath() {
		return (EditorApplication.currentScene + "_FmodGlobalReverbSettings.asset");
	}
#endif
	
	public static ReverbSettings Create () {
		ReverbSettings reverbSettings = ScriptableObject.CreateInstance(typeof(ReverbSettings)) as ReverbSettings;
		GlobalReverbZone globalReverb = GameObject.FindObjectOfType(typeof(GlobalReverbZone)) as GlobalReverbZone;
		GameObject obj = null;
			
		reverbSettings.PRESETS = new ReverbSettings.FmodReverbPreset[] {
			new FmodReverbPreset() { Properties = OFF_REVERB_PROPERTIES, Name = "Off" },
			new FmodReverbPreset() { Properties = GENERIC_REVERB_PROPERTIES, Name = "Generic" },
     		new FmodReverbPreset() { Properties = PSYCHOTIC_REVERB_PROPERTIES, Name = "Psychotic" },
			new FmodReverbPreset() { Properties = GENERIC_REVERB_PROPERTIES, Name = "User" },
		};
		reverbSettings.hideFlags = HideFlags.HideInHierarchy;
		if (globalReverb == null) {
			obj = GameObject.Instantiate(Resources.Load("GlobalReverbZone")) as GameObject;

			globalReverb = obj.GetComponent<GlobalReverbZone>();
		}
		if (globalReverb != null) {
			globalReverb.settings = reverbSettings;			
		}
		return (reverbSettings);
	}
		
	public static ReverbSettings Create(ReverbSettings src) {
		if (src != null && src.PRESETS != null) {
			ReverbSettings reverbSettings = ScriptableObject.CreateInstance(typeof(ReverbSettings)) as ReverbSettings;
			int nbPresets = src.PRESETS.Length;
			GlobalReverbZone globalReverb = GameObject.FindObjectOfType(typeof(GlobalReverbZone)) as GlobalReverbZone;
			GameObject obj = null;
			
			reverbSettings.SelectedPreset = src.SelectedPreset;
			reverbSettings.PRESETS = new ReverbSettings.FmodReverbPreset[nbPresets];
			for (int i = 0; i < nbPresets; i++) {
				reverbSettings.PRESETS[i] = new FmodReverbPreset();
				reverbSettings.PRESETS[i].Name = src.PRESETS[i].Name;
				reverbSettings.PRESETS[i].Properties = src.PRESETS[i].Properties;			
			}
			if (globalReverb == null) {
				obj = GameObject.Instantiate(Resources.Load("GlobalReverbZone")) as GameObject;
				
				globalReverb = obj.GetComponent<GlobalReverbZone>();
			}
			if (globalReverb != null) {
				globalReverb.settings = reverbSettings;
			}
			return (reverbSettings);
		} else {
			return (Create ());
		}
	}
	
	private static void LoadSettings() {
		GlobalReverbZone globalReverb = GameObject.FindObjectOfType(typeof(GlobalReverbZone)) as GlobalReverbZone;

		if (LOADED_REVERB_SETTINGS == null) {
#if UNITY_EDITOR
			UnityEngine.Object[] tmp = AssetDatabase.LoadAllAssetsAtPath(ReverbSettings.GetSettingsAssetPath());

			if (tmp.Length > 0 && tmp[0] != null) {
				LOADED_REVERB_SETTINGS = tmp[0] as ReverbSettings;
				if (LOADED_REVERB_SETTINGS != null) {
					LOADED_REVERB_SETTINGS.afterDeserializePresets();
					
					if (globalReverb == null) {
						GameObject obj = GameObject.Instantiate(Resources.Load("GlobalReverbZone")) as GameObject;
						globalReverb = obj.GetComponent<GlobalReverbZone>();
					}				
					if (globalReverb != null) {
						globalReverb.settings = LOADED_REVERB_SETTINGS;
					}
				}
			}
#endif
			if (globalReverb != null && LOADED_REVERB_SETTINGS == null) {
				LOADED_REVERB_SETTINGS = globalReverb.settings;		
				if (LOADED_REVERB_SETTINGS != null) {
					LOADED_REVERB_SETTINGS.afterDeserializePresets();
				}
			}
		}
	}
	
	public static ReverbSettings SaveSettings(ReverbSettings toSave) {
		if (toSave != null) {
			ReverbSettings newSettings = ReverbSettings.Create(toSave);
			GlobalReverbZone globalReverb = GameObject.FindObjectOfType(typeof(GlobalReverbZone)) as GlobalReverbZone;
	
			newSettings.beforeSerializePresets();
#if UNITY_EDITOR
			string settingsAssetPath = ReverbSettings.GetSettingsAssetPath();
			if (AssetDatabase.Contains(toSave) == true) {
				AssetDatabase.DeleteAsset(settingsAssetPath);
			}
			AssetDatabase.CreateAsset(newSettings, settingsAssetPath);
			AssetDatabase.SaveAssets();
			
			if (globalReverb != null) {
				newSettings.afterDeserializePresets();
				globalReverb.settings = newSettings;
				LOADED_REVERB_SETTINGS = globalReverb.settings;				
				if (EditorApplication.isPlaying || EditorApplication.isPaused) {
					FmodEventSystem.GetReverbManager().UpdateGlobalReverb();
				}
			}
#endif
		} else {
			Debug.LogError("FMOD_Unity: ReverbSettings: Null reverb settings");
		}
		return (LOADED_REVERB_SETTINGS);
	}

	private void beforeSerializePresets() {
		if (PRESETS != null && PRESETS.Length > 0) {
			foreach (FmodReverbPreset preset in PRESETS) {
				if (preset != null) {
					preset.BeforeSerialization();
				}
			}			
		}
	}
	private void afterDeserializePresets() {
		if (PRESETS != null && PRESETS.Length > 0) {
			foreach (FmodReverbPreset preset in PRESETS) {
				if (preset != null) {
					preset.AfterDeserialization();					
				}
			}			
		}
	}
}
