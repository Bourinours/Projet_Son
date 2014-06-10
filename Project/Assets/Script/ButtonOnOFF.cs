using UnityEngine;
using System.Collections;

public class ButtonOnOFF : Interaction
{
	public override void VRAction()
	{
		Debug.Log("VRAction actived");
		if (source != null)
		{
			if (source.CurrentStatus == FmodEventAudioSource.Status.Stopped)
				source.Play();
			else
				source.Stop();
		}
		this.activated = !this.activated;
	}

    public override bool victoryState()
    {
        return base.victoryState();
    }
}
