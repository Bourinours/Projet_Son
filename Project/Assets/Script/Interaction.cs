using UnityEngine;
using System.Collections;

public class Interaction : MonoBehaviour
{

	#region Public members

	public bool activated = false;
	public FmodEventAudioSource source;
	public FmodEventAudioSource impact;

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

	protected void Update()
	{

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
		Debug.Log("TriggerEnter " + col.name);
		if (impact != null && impact.CurrentStatus == FmodEventAudioSource.Status.Stopped)
			impact.Play();
	}
}