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
		this.ActiveDialog(eventDialog[0]);
	}

	public void Update()
	{
		if (mActionMutex)
			return;
		if (source.CurrentStatus == FmodEventAudioSource.Status.Stopped)
			this.endGame();
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
		if (activated)
			return value;
		if (value <= minDistance)
			this.ActiveDialog(eventDialog[1]);
		else if (value > 7)
			this.ActiveDialog(eventDialog[2]);
		source.SetParameterValue(paramSound, value);
		return value;
	}

    public override bool victoryState()
    {
		if (this.checkValue() <= minDistance)
			return true;
		return false;
    }

	public override void desactiveActionMutex()
	{
		mActionMutex = false;
		if (source != null)
			source.Play();
	}

	public override void OnTriggerEnter(Collider col)
	{
		if (mActionMutex)
			return;
		Debug.Log("TriggerEnter " + col.name);
	}

	private void endGame()
	{
		if (this.charactere == null)
			return;
		GameManager tmp = this.charactere.GetComponent<GameManager>();
		if (tmp == null)
			return;
		tmp.end(false);
	}
}
