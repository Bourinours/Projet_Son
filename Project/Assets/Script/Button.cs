using UnityEngine;
using System.Collections;

public class Button : Interaction
{
	public void VRAction()
	{
		Debug.Log("VRAction actived");
		if (source != null && source.CurrentStatus == FmodEventAudioSource.Status.Stopped)
		{
			source.Play();
			this.activated = true;
		}
	}

    public override bool victoryState()
    {
        return base.victoryState();
    }
}
