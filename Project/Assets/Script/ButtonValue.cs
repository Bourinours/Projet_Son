using UnityEngine;
using System.Collections;

public class ButtonValue : Interaction
{
	public float incr = 1;
	public FmodEventAudioSource obj;
	public string paramSound = "param01";
    private int mWantedValue;

    private void Start()
    {
        mWantedValue = Random.Range(0, 10);
    }

	public void VRAction()
	{
        if (mActionMutex)
            return;
		Debug.Log("VRAction actived");
		if (source != null)
		{
			source.Play();
			this.UpdateValueObj();
		}
	}

	private void UpdateValueObj()
	{
		float value;

		if (obj == null)
			return;
		value = obj.GetParameterValue(paramSound);
		value += incr;
		obj.SetParameterValue(paramSound, Mathf.Clamp(value, obj.GetParameterMinRange(paramSound), obj.GetParameterMaxRange(paramSound)));
	}

    public override bool victoryState()
    {
        if (obj.GetParameterValue(paramSound) == mWantedValue)
            return true;
        return false;
    }
}

