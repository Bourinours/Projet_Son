/*
 * +-------------------------------------------------------------------------------------+
 *  Project: FMOD_Unity
 *  Description: OBSOLETE. Here for retro compatibility.
 * 				 The FmodEventAudioClip class exists mainly to respect coherence with the way Unity sounds work.
 * 				 It is now obsolete. Don't use it anymore. Seriously. This is only here for retro compatibility. FmodEventAudioSource handle the runtime now.
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
using System.Collections.Generic;
using System.Runtime.InteropServices;

public class FmodEventAudioClip : ScriptableObject
{
	[SerializeField]
	private FmodEvent m_source;

	public void Initialize(FmodEvent sourceEvent) {
		m_source = sourceEvent;
		name = m_source.name;
	}
	
	public FmodEvent.SourceType getSourceType() {
		return (m_source.getSourceType());	
	}
	
	public List<FmodEventParameter> getParameters() {
		return (m_source.getParameters());
	}
	
	public FmodEvent getSource() {
		return (m_source);
	}
}

