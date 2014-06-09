using UnityEngine;
using System.Collections;

public class Button : Interaction
{
    private bool mState = false; // false = off / true = On

	public void VRAction()
	{
        if (mActionMutex)
            return;
		Debug.Log("VRAction actived");
		if (source != null && source.CurrentStatus == FmodEventAudioSource.Status.Stopped)
			source.Play();
        this.activated = true;
        mState = true;
	}

    public override bool victoryState()
    {
        if (mState)
            return true;
        return false;
    }
}
