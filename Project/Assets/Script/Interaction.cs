using UnityEngine;
using System.Collections;

public class Interaction : MonoBehaviour
{

	#region Public members

	public bool activated = false;
	public FmodEventAudioSource source;
	public FmodEventAudioSource impact;
	public GameObject wandGrab = null;
	public GameObject charactere;
	public FmodEvent eventDialog;

	#endregion

	#region Protected members

	public bool mActionMutex = true;

	#endregion

	protected virtual void Awake()
	{
		if (source != null)
		{
			source.playOnAwake = false;
		}
		if (impact != null)
		{
			impact.playOnAwake = false;
		}
	}

	public virtual void VRAction()
	{
		Debug.Log("VRAction actived");
		if (source != null && source.CurrentStatus == FmodEventAudioSource.Status.Stopped)
			source.Play();
	}

	public virtual void OnCollisionEnter(Collision col)
	{
		Debug.Log("HitCollision " + col.collider.name);
		if (impact != null && impact.CurrentStatus == FmodEventAudioSource.Status.Stopped)
			impact.Play();
	}

	public virtual void OnTriggerEnter(Collider col)
	{
		if (mActionMutex)
			return;
		Debug.Log("TriggerEnter " + col.name);
		if (impact != null)// && impact.CurrentStatus == FmodEventAudioSource.Status.Stopped)
		{
			impact.Stop ();
			impact.Play();
		}
	}

    public virtual bool victoryState()
    {
        return true;
    }

    public void activeActionMutex()
    {
        mActionMutex = true;
    }

	public void desactiveActionMutex()
	{
		mActionMutex = false;
	}

	protected virtual void ActiveDialog()
	{
		if (this.charactere == null)
			return;
		GameManager tmp = this.charactere.GetComponent<GameManager>();
		if (tmp == null)
			return;
		if (impact != null)
			impact.Stop();
		tmp.SetDialog(eventDialog, true);
	}
}