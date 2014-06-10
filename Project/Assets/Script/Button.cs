using UnityEngine;
using System.Collections;

public class Button : Interaction
{
    private bool mState = false; // false = off / true = On

	public override void VRAction()
	{
		if (activated && !mActionMutex)
		{
			if (eventDialog.Count >= 2)
				this.ActiveDialog(eventDialog[2]);
			return;
		}
        if (mActionMutex || activated)
            return;
		Debug.Log("VRAction actived");
		if (source != null && source.CurrentStatus == FmodEventAudioSource.Status.Stopped)
			source.Play();
        this.activated = true;
        mState = true;
		if (eventDialog.Count >= 1)
			this.ActiveDialog(eventDialog[0]);
	}

    public override bool victoryState()
    {
        if (mState)
            return true;
        return false;
    }

	public override void OnTriggerEnter(Collider col)
	{
		if (mActionMutex || activated)
			return;
		Debug.Log("TriggerEnter " + col.name);
		if (eventDialog.Count >= 1)
			this.ActiveDialog(eventDialog[1]);
	}
}
