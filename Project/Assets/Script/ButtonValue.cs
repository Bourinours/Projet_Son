﻿using UnityEngine;
using System.Collections;

public class ButtonValue : Interaction
{
	public float incr = 1;
	public FmodEventAudioSource objSound;
	public GameObject obj;
	public string paramSound = "param01";

	public override void VRAction()
	{
        if (mActionMutex)
            return;
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
		value = objSound.GetParameterValue(paramSound);
		value += incr;
		if (value > objSound.GetParameterMaxRange(paramSound) || value < objSound.GetParameterMinRange(paramSound))
			this.ActiveDialog(eventDialog[1], true);
		else
			objSound.SetParameterValue(paramSound, Mathf.Clamp(value, objSound.GetParameterMinRange(paramSound), objSound.GetParameterMaxRange(paramSound)));
	}

    public override bool victoryState()
    {
		if (obj.GetComponent<Boudoir>() != null && obj.GetComponent<Boudoir>().CheckValue())
            return true;
        return false;
    }

	public override void OnTriggerEnter(Collider col)
	{
		if (mActionMutex)
			return;
		Debug.Log("TriggerEnter " + col.name);
		this.ActiveDialog(eventDialog[0], false);
	}
}

