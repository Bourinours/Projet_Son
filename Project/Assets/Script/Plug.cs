using UnityEngine;
using System.Collections;

public class Plug : Interaction
{
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
