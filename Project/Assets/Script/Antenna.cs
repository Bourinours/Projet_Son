using UnityEngine;
using System.Collections;

public class Antenna : Interaction 
{
	public string paramSound = "param01";
	public Transform dest = null;
	public float minDistance = 0;
	private Transform mDefaultParent;

	public void Start()
	{
		mDefaultParent = this.transform.parent;
	}

	public override void VRAction()
	{
		if (mActionMutex)
			return;
		Debug.Log("VRAction actived");
		if (source != null && source.CurrentStatus == FmodEventAudioSource.Status.Stopped)
			source.Play();
		this.transform.parent = wandGrab.transform.parent;
	}

	public void Update()
	{
		if (mActionMutex)
			return;
		if (wandGrab == null)
		{
			this.transform.parent = mDefaultParent;
			return;
		}
		else
			this.transform.parent = wandGrab.transform.parent;
		this.checkValue();
	}

	private float checkValue()
	{
		float value;

		value = Vector3.Distance(dest.position, this.transform.position);
		return value;
	}

    public override bool victoryState()
    {
		if (this.checkValue() <= minDistance)
			return true;
		return false;
    }
}
