/*
 * +-------------------------------------------------------------------------------------+
 *  Project: FMOD_Unity
 *  Description: Keeps track of the current ReverbSettings, the global reverberation,
 * 				 in a way that Unity can tie it to a scene, serialize it and properly
 * 				 export it.
 *  Company: ISART Digital (www.isartdigital.com)
 *  Author: Galaad BROD (galaad.brod@gmail.com)
 * +-------------------------------------------------------------------------------------+
 */

using UnityEngine;
using System.Collections;

public class GlobalReverbZone : MonoBehaviour {

	[HideInInspector()]
	public ReverbSettings settings;
		
	// Use this for initialization
	void Start () {
		if (settings == null) {
			settings = ReverbSettings.Get();
		}
	}
}
