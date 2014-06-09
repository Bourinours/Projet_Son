using UnityEngine;
using System.Collections;

public class Plug : Interaction
{
	public int index = 1;

	public virtual void VRAction()
	{
		Debug.Log("VRAction actived");
		if (source != null && source.CurrentStatus == FmodEventAudioSource.Status.Stopped)
			source.Play();
		Debug.Log(wandGrab.name);
	}

	public virtual void OnTriggerExit(Collider col)
	{
		Debug.Log("TriggerExit " + col.name);
		if (impact != null && impact.CurrentStatus == FmodEventAudioSource.Status.Playing)
			impact.Stop();
	}

    public override bool victoryState()
    {
        return base.victoryState();
    }
}
