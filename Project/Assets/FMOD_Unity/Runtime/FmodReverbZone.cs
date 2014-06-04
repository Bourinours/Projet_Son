/*
 * +-------------------------------------------------------------------------------------+
 *  Project: FMOD_Unity
 *  Description: This component assigns a Reverb Definition (FmodReverb from a
 * 				 FmodEventAsset) to a zone in-game. For this, it needs a collider on
 * 				 trigger mode.
 * 
 * 				 The current zone is chosen based on the position of the
 * 				 AudioListener component and the priorities of the FmodReverbZones.
 * 				 Interpolation between two zones is based on the "fade time" of the
 * 			     entered zone (the global reverb uses the fade time of the previous zone).
 * 
 * 				 To see the current stack of FmodReverbZone, you can open the
 * 				 "FMOD/Reverb/Active Reverb Zones" menu.
 *  Company: ISART Digital (www.isartdigital.com)
 *  Author: Galaad BROD (galaad.brod@gmail.com)
 * +-------------------------------------------------------------------------------------+
 */

using UnityEngine;
using System.Collections;

public class FmodReverbZone : MonoBehaviour {
	[SerializeField]
	protected FmodReverb m_reverb;

	protected Collider m_collider = null;
	protected FmodEventSystemHandle m_handle = null;
	protected bool m_isGlobal = false;
	
	public float fadeTime = 3.0f; // how long the transition will be
	[SerializeField]
	protected float m_priority = 100;
	public float Priority {
		get { return (m_priority); }
		set { m_priority = (int)value; }
	}
	
	public void SetReverb(FmodReverb reverb) {
		if (m_reverb == null) {
			m_reverb = reverb;
		}
	}
	
	public FmodReverb GetReverb() {
		return (m_reverb);
	}
	
	public void ResetReverb() {
		m_reverb = null;
	}

	public void SetGlobal (bool isGlobal)
	{
		m_isGlobal = isGlobal;
		if (m_isGlobal == true) {
			collider.enabled = false;
		}
	}
	
	public bool IsGlobal() {
		return (m_isGlobal);
	}
	
	// Use this for initialization
	void Start () {
		if (m_isGlobal) {
			return ;
		}
		if (gameObject.collider == null) {
			Debug.LogError("FMOD_Unity: Missing collider on object '" + gameObject.name + "': FmodReverbZone needs a trigger collider");
		} else if (gameObject.collider.isTrigger == false) {
			Debug.LogError("FMOD_Unity: Collider on object '" + gameObject.name + "' is not a trigger collider. FmodReverbZone needs a trigger collider.");
		} else {
			m_collider = gameObject.collider;
			if (Application.isEditor) {
				AudioListener[] listeners = UnityEngine.Object.FindObjectsOfType(typeof(AudioListener)) as AudioListener[];
				
				if (listeners.Length > 0) {
					Collider col = listeners[0].collider;
					Rigidbody rigid = listeners[0].rigidbody;
					
					if (col == null) {
						Debug.LogWarning("FMOD_Unity: Warning: you use Reverb Zones but the object with the AudioListener doesn't have a collider and so won't trigger any event when entering or exiting a Reverb Zone.");
					}
					if (rigid == null) {
						Debug.LogWarning("FMOD_Unity: Warning: you use Reverb Zones but the object with the AudioListerner doesn't have a rigidbody; it probably should have one in order to trigger events when entering or exiting a Reverb Zone.");
					}
				}
			}
		}
	}
	
	void OnTriggerEnter(Collider other) {
		if (m_isGlobal == false && other.gameObject.GetComponent<AudioListener>() != null) {
			EnableReverb();
		}
	}
	
	void OnTriggerExit(Collider other) {
		if (m_isGlobal == false && other.gameObject.GetComponent<AudioListener>() != null) {
			DisableReverb();
		}
	}
	
	public void EnableReverb() {
		if (m_reverb != null && m_collider != null) {
			FmodEventSystemHandle handle = new FmodEventSystemHandle();
			
			handle.getEventSystem().setReverb(this);
			handle.Dispose();
		}
	}
	public void DisableReverb() {
		if (m_reverb != null && m_collider != null) {
			FmodEventSystemHandle handle = new FmodEventSystemHandle();
			
			handle.getEventSystem().unsetReverb(this);
			handle.Dispose();
		}
	}
}
