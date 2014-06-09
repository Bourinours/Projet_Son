using UnityEngine;
using System.Collections;

public class Lever : Interaction
{
    private bool mState = false; // false = off / true = On

    public void VRAction()
    {
        if (mActionMutex)
            return;
        Debug.Log("VRAction actived");
        this.activated = true;
        mState = true;
		if (source != null)
			source.Play ();
    }

    public override bool victoryState()
    {
        if (mState)
            return true;
        return false;
    }
}
