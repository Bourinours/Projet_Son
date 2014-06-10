using UnityEngine;
using System.Collections;

public class Plug : Interaction
{
	public int index = 1;
	public string paramSound = "param01";
	private bool mState = false;

	public override void VRAction()
	{
		if (activated || wandGrab.GetComponent<Cable>().index == 0 || mActionMutex)
			return;
		Debug.Log("VRAction actived");
		this.checkValue();
		if (source != null && source.CurrentStatus == FmodEventAudioSource.Status.Stopped)
			source.Play();
	}

	private void checkValue()
	{
		int indexWand = wandGrab.GetComponent<Cable>().index;

		if (this.index == indexWand)
		{
			this.source.SetParameterValue(paramSound, 1);
			this.activated = true;
			wandGrab.GetComponent<Cable>().index = 0;
			this.impact.Stop();
			this.mState = true;
			this.ActiveDialog(eventDialog[0]);
		}
		else
			this.source.SetParameterValue(paramSound, 5);
	}

	public override void OnTriggerEnter(Collider col)
	{
		Cable tmp = col.transform.parent.gameObject.GetComponent<Cable>();

		if (activated || (tmp != null && tmp.index == 0) || mActionMutex)
			return;
		Debug.Log("TriggerEnter " + col.name);
		if (impact != null)
		{
			impact.Stop ();
			impact.Play();
		}
	}
	public void OnTriggerExit(Collider col)
	{
        if (mActionMutex)
            return;
		Debug.Log("TriggerExit " + col.name);
		if (impact != null && impact.CurrentStatus == FmodEventAudioSource.Status.Playing)
		{
			impact.KeyOffOnParameter("Magnet");
		}
		if (source != null && !activated)
			source.Stop();
	}

    public override bool victoryState()
    {
		if (mState)
			return true;
		return false;
    }
}
