﻿using UnityEngine;
using System.Collections;

public class ButtonValue : Interaction
{
	public int incr = 1;
	public FmodEventAudioSource obj;
	public string paramSound = "param01";

	public void VRAction()
	{
		Debug.Log("VRAction actived");
		if (source != null)
		{
			source.Play();
			this.UpdateValueObj();
		}
	}

	private void UpdateValueObj()
	{
		float value;

		if (obj == null)
			return;
		value = obj.GetParameterValue(paramSound);
		value += incr;
		obj.SetParameterValue(paramSound, Mathf.Clamp(value, obj.GetParameterMinRange(paramSound), obj.GetParameterMaxRange(paramSound)));
	}
}

