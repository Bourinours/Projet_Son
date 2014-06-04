/*
 * +-------------------------------------------------------------------------------------+
 *  Project: FMOD_Unity
 *  Description: Stores the settings that FMOD needs for defining a Reverb Def.
 * 				The FmodReverb are created from the Reverb Defs in a .fev file.
 * 
 * 				You can create FmodReverbs yourself, but be careful : as it's a
 * 				ScriptableObject, it won't be serialized in the scene on its own.
 * 				To be exported (or even saved) it needs to be saved to an asset.
 * 				You can use CreateAsset() for that purpose.
 *  Company: ISART Digital (www.isartdigital.com)
 *  Author: Galaad BROD (galaad.brod@gmail.com)
 * +-------------------------------------------------------------------------------------+
 */

using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections;

public class FmodReverb : ScriptableObject {
	protected static ReverbSettings LOADED_REVERB_SETTINGS = null;
	public static ReverbSettings.FmodReverbPreset GLOBAL_REVERB {
		get {
			LOADED_REVERB_SETTINGS = ReverbSettings.Get();
			return (LOADED_REVERB_SETTINGS.CurPreset);
		}
	}
		
	[SerializeField]
	protected string m_name;
	
	[RangeAttribute(-10000, 0)]
	public int Room;
	[RangeAttribute(-10000, 0)]
	public int RoomHF;
	[RangeAttribute(-10000, 0)]
	public int RoomLF;	
	[RangeAttribute(0.1f, 20)]
	public float DecayTime;
	[RangeAttribute(0.1f, 2)]
	public float DecayHFRatio;
	[RangeAttribute(0.1f, 2)]
	public float DecayLFRatio;
	[RangeAttribute(-10000, 1000)]
	public int Reflections;
	[RangeAttribute(0, 0.3f)]
	public float ReflectionsDelay;
	[RangeAttribute(-10000, 2000)]
	public int Reverb;
	[RangeAttribute(0.0f, 0.1f)]
	public float ReverbDelay;
	[RangeAttribute(0.04f, 4)]
	public float ModulationTime;
	[RangeAttribute(0, 1)]
	public float ModulationDepth;
	[RangeAttribute(1000, 20000)]
	public float HFReference;
	[RangeAttribute(20, 1000)]
	public float LFReference;
	[RangeAttribute(0, 100)]
	public float Diffusion;
	[RangeAttribute(0, 100)]
	public float Density;

	public void Initialize (string name, FMOD.REVERB_PROPERTIES reverb)
	{
		this.name = name;
		m_name = name;
		hideFlags = HideFlags.HideInHierarchy;
		Room = reverb.Room;
		RoomHF = reverb.RoomHF;
		RoomLF = reverb.RoomLF;
		DecayTime = reverb.DecayTime;
		DecayHFRatio = reverb.DecayHFRatio;
		DecayLFRatio = reverb.DecayLFRatio;
		Reflections = reverb.Reflections;
		ReflectionsDelay = reverb.ReflectionsDelay;
		Reverb = reverb.Reverb;
		ReverbDelay = reverb.ReverbDelay;
		ModulationTime = reverb.ModulationTime;
		ModulationDepth = reverb.ModulationDepth;
		HFReference = reverb.HFReference;
		LFReference = reverb.LFReference;
		Diffusion = reverb.Diffusion;
		Density = reverb.Density;
	}
	
	public void CreateAsset(string assetFile, FmodEventAsset asset) {
#if UNITY_EDITOR
		AssetDatabase.AddObjectToAsset(this, asset);
//		AssetDatabase.ImportAsset(assetFile); // force a save
#endif
	}

	
	public FMOD.REVERB_PROPERTIES getProperties() {
		return (new FMOD.REVERB_PROPERTIES(
			0, 0, 1,
			Room, RoomHF, RoomLF,
			DecayTime, DecayHFRatio, DecayLFRatio,
			Reflections, ReflectionsDelay,
			Reverb, ReverbDelay,
			ModulationTime, ModulationDepth,
			HFReference, LFReference,
			Diffusion,
			Density,
			FMOD.REVERB_FLAGS.DEFAULT
			));
	}
	
	public string getName() {
		return (m_name);
	}
	
	public static FMOD.REVERB_PROPERTIES getLerpedProperties(FMOD.REVERB_PROPERTIES a, FMOD.REVERB_PROPERTIES b, float coef) {
		FMOD.REVERB_PROPERTIES c = ReverbSettings.DEFAULT_REVERB_PROPERTIES;
		
		coef = Mathf.Clamp(coef, 0f, 1f);
		c.DecayHFRatio = Mathf.Lerp(a.DecayHFRatio, b.DecayHFRatio, coef);
		c.DecayLFRatio = Mathf.Lerp(a.DecayLFRatio, b.DecayLFRatio, coef);
		c.DecayTime = Mathf.Lerp(a.DecayTime, b.DecayTime, coef);
		c.Density = Mathf.Lerp(a.Density, b.Density, coef);
		c.Diffusion = Mathf.Lerp(a.Diffusion, b.Diffusion, coef);
		c.EnvDiffusion = Mathf.Lerp(a.EnvDiffusion, b.EnvDiffusion, coef);
		c.Environment = 0; // for USER, user-set value
		c.Flags = b.Flags; // can't interpolate on binary mask
		c.HFReference = Mathf.Lerp(a.HFReference, b.HFReference, coef);
		c.Instance = b.Instance;
		c.LFReference = Mathf.Lerp(a.LFReference, b.LFReference, coef);
		c.ModulationDepth = Mathf.Lerp(a.ModulationDepth, b.ModulationDepth, coef);
		c.ModulationTime = Mathf.Lerp(a.ModulationTime, b.ModulationTime, coef);
		c.Reflections = (int)Mathf.Lerp(a.Reflections, b.Reflections, coef);
		c.ReflectionsDelay = Mathf.Lerp(a.ReflectionsDelay, b.ReflectionsDelay, coef);
		c.Reverb = (int)Mathf.Lerp(a.Reverb, b.Reverb, coef);
		c.ReverbDelay = Mathf.Lerp(a.ReverbDelay, b.ReverbDelay, coef);
		c.Room = (int)Mathf.Lerp(a.Room, b.Room, coef);
		c.RoomHF = (int)Mathf.Lerp(a.RoomHF, b.RoomHF, coef);
		c.RoomLF = (int)Mathf.Lerp(a.RoomLF, b.RoomLF, coef);
		return (c);
	}
}
