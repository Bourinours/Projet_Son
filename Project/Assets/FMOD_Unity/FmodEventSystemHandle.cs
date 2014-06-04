/*
 * +-------------------------------------------------------------------------------------+
 *  Project: FMOD_Unity
 *  Description: The FmodEventSystemHandle class was built as a smart pointer of
 * 				 sorts to handle the referencing to the FmodEventSystem.
 * 				 Because the FMOD.EventSystem class lives on the C++ unmanaged
 * 				 memory and does not have a singleton, because the reference is
 * 				 easily lost by Mono (in cases of a rebuild for example) and
 * 				 because we can't let the GarbageCollector do its job when it 
 * 				 so pleases, we need to handle the Disposition of the handles
 * 				 ourselves.
 * 				 So be VERY CAREFUL to call Dispose() manually as soon as you don't
 * 				 need the handle. It's recommended to lose the reference to the
 * 				 handle itself right after.
 *  Company: ISART Digital (www.isartdigital.com)
 *  Author: Galaad BROD (galaad.brod@gmail.com)
 * +-------------------------------------------------------------------------------------+
 */

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;


/**
 */
public class FmodEventSystemHandle : IDisposable {
	private static int m_nbHandles = 0; 
	private FmodEventSystem m_eventSystem = null;
	
	public static int NbHandles {
		get { return (m_nbHandles); }
	}
	
	public FmodEventSystemHandle() {
		FmodEventSystem.getEventSystem(this);
		if (m_eventSystem != null) {
			m_nbHandles++;
		}
	}
	public FmodEventSystem getEventSystem() {
		return (m_eventSystem);
	}
	public bool setEventSystem(FmodEventSystem system) {
		if (system == null || m_eventSystem != null) {
			return (false);
		}
		m_eventSystem = system;
		return (false);
	}
	
	public FmodMusicSystem getMusicSystem() {
		return (m_eventSystem.getMusicSystem());
	}
	
	#region IDisposable implementation
	public void Dispose ()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}
	protected virtual void Dispose(bool disposing) {
		if (m_eventSystem != null) {
			if (m_nbHandles == 0) {
				Debug.LogWarning("FMOD_Unity: FmodEventSystemHandle: An event system is still referenced but there is no handle on it ! This should not happen !");
			} else {
				if (--m_nbHandles == 0 && Application.isEditor) {
					m_eventSystem.clean();
					m_eventSystem = null;
				}
			}
		}		
	}
	#endregion
}
