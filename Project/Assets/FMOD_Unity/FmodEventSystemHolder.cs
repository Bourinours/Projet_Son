/*
 * +-------------------------------------------------------------------------------------+
 *  Project: FMOD_Unity
 *  Description: This component will be added on the fly to an empty GameObject
 * 				if there isn't one already.
 * 				Its purpose is to receive the OnDestroy event indicating the
 * 				closing of the scene, and making sure the FmodEventSystem persists
 * 				throughout the scene.
 * 				If you need to keep the system alive between scenes, do so by
 * 				indicating in another script that the Holder object is not
 * 				to be destroyed using :
 * 					Object.DontDestroyOnLoad(holderGameObject);
 *  Company: ISART Digital (www.isartdigital.com)
 *  Author: Galaad BROD (galaad.brod@gmail.com)
 * +-------------------------------------------------------------------------------------+
 */

#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using System.Collections;

public class FmodEventSystemHolder : MonoBehaviour {
	
	protected FmodEventSystemHandle m_handle = null;
	
	public void OnEnable() {
		FmodEventSystemHolder holder = FmodEventSystem.getEventSystemHolder();
		
		if (holder != null && holder != this) {
			Component[] comps = gameObject.GetComponents<Component>();
			
			if (comps.Length == 2) { // if only transform and holder comps, we can delete the gameObject
				Destroy(gameObject);
			} else { // if there are any other components, we can't delete the gameObject, but we can delete the comp
				Destroy(this);
			}
		} else {
#if UNITY_EDITOR
        	EditorApplication.playmodeStateChanged += clean;
#endif
		}
	}
		
	public void SetFmodEventSystemHandle(FmodEventSystemHandle handle) {
		if (m_handle == null) {
			m_handle = handle;
			GameObject.DontDestroyOnLoad(gameObject);
		}
	}
	
	public void Update() {
		if (m_handle != null) {
			FmodEventSystem system = m_handle.getEventSystem();
			if (system != null) {
				system.updateSystem();
			}
		}
	}
		
	public void OnDestroy() {
		if (m_handle != null) {
			m_handle.Dispose();
			m_handle = null;
		}
	}
	
	private void clean() {
#if UNITY_EDITOR
		if (EditorApplication.isPlaying && !EditorApplication.isPlayingOrWillChangePlaymode) {
        	EditorApplication.playmodeStateChanged -= clean;	
			OnDestroy ();
		}
#endif
	}
}
