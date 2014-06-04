/*
 * +-------------------------------------------------------------------------------------+
 *  Project: FMOD_Unity
 *  Description: Manages the reverb zone stack, choosing the good reverb to activate
 * 				 based on zone priorities and handles interpolation between reverbs.
 *  Company: ISART Digital (www.isartdigital.com)
 *  Author: Galaad BROD (galaad.brod@gmail.com)
 * +-------------------------------------------------------------------------------------+
 */

using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections;
using System.Collections.Generic;

public class FmodReverbManager : MonoBehaviour {
	private FMOD.REVERB_PROPERTIES m_startProperties = ReverbSettings.DEFAULT_REVERB_PROPERTIES;
	private FMOD.REVERB_PROPERTIES m_curProperties = ReverbSettings.DEFAULT_REVERB_PROPERTIES;
	private FMOD.REVERB_PROPERTIES m_endProperties = ReverbSettings.DEFAULT_REVERB_PROPERTIES;
	private bool m_isInTransition = false;
	private float m_curTransitionDuration = 0;
	private float m_curTransitionTime = 0;
	
	private List<FmodReverbZone> m_reverbStack = new List<FmodReverbZone>();
	private FmodReverbZone m_currentZone;
	private FmodReverbZone m_globalReverbZone;
	private FmodReverb m_globalReverb;
	
	// Use this for initialization
	void Start () {
		FmodEventSystemHandle handle = new FmodEventSystemHandle();
		ReverbSettings.FmodReverbPreset globalReverbSetting = FmodReverb.GLOBAL_REVERB;
		m_globalReverb = FmodReverb.CreateInstance(typeof(FmodReverb)) as FmodReverb;
		GameObject obj = GameObject.Instantiate(Resources.Load("FmodReverbZone")) as GameObject;
		m_globalReverbZone = obj.GetComponent<FmodReverbZone>();
		
		if (m_globalReverbZone == null) {
			Debug.LogError("Prefab for FmodReverbZone should have component FmodReverbZone !");
		}
		m_globalReverbZone.SetGlobal(true);
		m_globalReverbZone.name = "Global Reverb";
		m_globalReverb.Initialize(globalReverbSetting.Name, globalReverbSetting.Properties);
		m_globalReverbZone.SetReverb(m_globalReverb);
		m_globalReverbZone.Priority = 0;
		m_globalReverbZone.fadeTime = 0;
		m_currentZone = m_globalReverbZone;
		StackReverb (m_currentZone);
		handle.getEventSystem().setReverbImmediate(globalReverbSetting.Properties);
		handle.Dispose();
		m_startProperties = globalReverbSetting.Properties;
		m_curProperties = globalReverbSetting.Properties;
		m_endProperties = globalReverbSetting.Properties;
	}
	
	// Update is called once per frame
	void Update () {
		if (m_isInTransition) {
			FmodEventSystemHandle handle = new FmodEventSystemHandle();

			m_curTransitionTime += Time.deltaTime;
			if (m_curTransitionTime >= m_curTransitionDuration) {
				m_curTransitionTime = m_curTransitionDuration;
				m_isInTransition = false;
			}
			m_curProperties = FmodReverb.getLerpedProperties(m_startProperties, m_endProperties, m_curTransitionTime / m_curTransitionDuration);
			handle.getEventSystem().setReverbImmediate(m_curProperties);
			handle.Dispose();
		}
	}
	
	private void _updateReverbStack() {
		if (m_reverbStack.Count == 0) {
			return ;
		}
		m_reverbStack.Sort((x, y) => (int)(y.Priority).CompareTo((int)x.Priority));
		FmodReverbZone newCurrentZone = m_reverbStack[0];
		
		
		if (m_currentZone != newCurrentZone) { // we should transition
			float fadeInTime = newCurrentZone.fadeTime;
			
			// if we are transitionning from a zone back to the global zone, we use the previous fade time
			// to fade out
			if (newCurrentZone == m_globalReverbZone && m_currentZone != m_globalReverbZone) {
				m_globalReverb.Initialize(m_globalReverb.getName(), FmodReverb.GLOBAL_REVERB.Properties);
				fadeInTime = m_currentZone.fadeTime;
			}
			FMOD.REVERB_PROPERTIES targetProps = newCurrentZone.GetReverb().getProperties();
			if (fadeInTime <= 0) {
				FmodEventSystemHandle handle = new FmodEventSystemHandle();
				
				m_isInTransition = false;
				m_startProperties = m_curProperties;
				m_endProperties = targetProps;
				handle.getEventSystem().setReverbImmediate(targetProps);
				handle.Dispose();
			} else {
				m_isInTransition = true;
				m_startProperties = m_curProperties;
				m_endProperties = targetProps;
				m_curTransitionDuration = fadeInTime;
				m_curTransitionTime = 0;			
			}
			m_currentZone = newCurrentZone;
		}
	}
	
	public void StackReverb (FmodReverbZone zone)
	{
		m_reverbStack.Add(zone);
		_updateReverbStack();
	}
	
	public void UnstackReverb(FmodReverbZone zone) {
		m_reverbStack.Remove(zone);
		_updateReverbStack();
	}
	
	public List<FmodReverbZone> GetReverbZoneStack() {
		return (m_reverbStack);
	}
	
	public void UpdateGlobalReverb() {
		if (m_currentZone == m_globalReverbZone && m_globalReverbZone != null && m_globalReverb != null) {
			FmodEventSystemHandle handle = new FmodEventSystemHandle();
			ReverbSettings.FmodReverbPreset globalReverbSetting = ReverbSettings.Get().CurPreset;
			
			m_globalReverb = m_globalReverbZone.GetReverb();
			m_globalReverb.Initialize(globalReverbSetting.Name, globalReverbSetting.Properties);
			m_curProperties = m_globalReverb.getProperties();
			m_endProperties = m_curProperties;
			handle.getEventSystem().setReverbImmediate(m_curProperties);
			handle.Dispose();
		}
	}
}
