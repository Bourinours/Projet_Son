/*
 * +-------------------------------------------------------------------------------------+
 *  Project: FMOD_Unity
 *  Description: A wrapper around FMOD.EventParameter, containing information about
 * 				 a single parameter from a FMOD.Event / FmodEvent.
 * 				 The instantiated, runtime parameter is handled in
 * 				 FmodRuntimeEventParameter.
 *  Company: ISART Digital (www.isartdigital.com)
 *  Author: Galaad BROD (galaad.brod@gmail.com)
 * +-------------------------------------------------------------------------------------+
 */

using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System;
using System.Collections;

public class FmodEventParameter : ScriptableObject
{
	[SerializeField]
	protected string m_name;
	[SerializeField]
	protected float m_minRange;
	[SerializeField]
	protected float m_maxRange;
	[SerializeField]
	protected float m_value;
	[SerializeField]
	protected float m_velocity;
	[SerializeField]
	protected float m_seekSpeed;
	[SerializeField]
	protected FmodEvent m_event;
	
	public float MinRange { get { return (m_minRange); } }
	public float MaxRange { get { return (m_maxRange); } }
	public float Velocity { get { return (m_velocity); } }
	public float SeekSpeed { get { return (m_seekSpeed); } }
		
	public FmodEventParameter() {
	}
		
	public void Initialize(FMOD.EventParameter param, FmodEvent evt) {
		IntPtr paramName = new IntPtr(0);
		int index = 0;
		FMOD.RESULT result = FMOD.RESULT.OK;
		
		hideFlags = HideFlags.HideInHierarchy;
		result = param.getInfo(ref index, ref paramName);
		ERRCHECK(result);
		m_name = System.Runtime.InteropServices.Marshal.PtrToStringAnsi(paramName);
		this.name = m_name;
		result = param.getRange(ref m_minRange, ref m_maxRange);
		ERRCHECK(result);
		result = param.getValue(ref m_value);
		ERRCHECK(result);
		result = param.getVelocity(ref m_velocity);
		ERRCHECK(result);
		result = param.getSeekSpeed(ref m_seekSpeed);
		ERRCHECK(result);
		m_event = evt;
	}
	
	public void Initialize(FmodEventParameter src, FmodEvent evt) {
#if UNITY_EDITOR
		hideFlags = HideFlags.HideInHierarchy;
		EditorUtility.CopySerialized(src, this);
		m_event = evt;
#endif
	}

	
	public void CreateAsset(string assetFile, FmodEvent asset) {
#if UNITY_EDITOR
		AssetDatabase.AddObjectToAsset(this, asset);
//		AssetDatabase.ImportAsset(assetFile, ImportAssetOptions.ImportRecursive); // force a save
#endif
	}
		
	public void SetValue(float val) {
		m_value = Mathf.Clamp(val, MinRange, MaxRange);
	}
			
	public float getValue ()
	{
		return (m_value);
	}

	public string getName ()
	{
		return (m_name);
	}
	
	
	//FMOD Error checking from return codes
	void ERRCHECK(FMOD.RESULT result)
    {
        if (result != FMOD.RESULT.OK)
        {
            Debug.LogWarning("FMOD_Unity: FmodEventParameter '" + m_name + "': Error: " + FMOD.Error.String(result));
        }
    }
}

