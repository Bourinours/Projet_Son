using UnityEngine;
using System.Collections;

public class Potentio : Interaction
{
    public FmodEventAudioSource obj;
    public string paramSound = "param01";
    private float mPrimRot = 0.0f;

    private void Start()
    {
        float value = 4.0f;
        while (value >= 4.0f && value <= 7.0f)
            value = Random.Range(3.0f, 8.0f);
        obj.SetParameterValue(paramSound, Mathf.Clamp(value, obj.GetParameterMinRange(paramSound), obj.GetParameterMaxRange(paramSound)));
    }

    private void Update()
    {
        if (mActionMutex)
            return;
        if (wandGrab == null)
        {
            mPrimRot = 0.0f;
            return;
        }
		float lRot = wandGrab.transform.eulerAngles.z;
        if (lRot > 350.0f && mPrimRot < 10.0f)
            mPrimRot = 360.0f + mPrimRot;
        if (lRot < 10.0f && mPrimRot > 350.0f)
            mPrimRot = mPrimRot - 360.0f;
        this.UpdateValueObj(lRot - mPrimRot);
        obj.Play();
        mPrimRot = lRot;
    }

    public void VRAction()
    {
        if (mActionMutex)
            return;
		mPrimRot = wandGrab.transform.eulerAngles.z;
        Debug.Log("VRAction actived");
        if (source != null)
            source.Play();
    }

    private void UpdateValueObj(float val)
    {
        float value;

        if (obj == null)
            return;
        value = obj.GetParameterValue(paramSound);
        value += val;
        obj.SetParameterValue(paramSound, Mathf.Clamp(value, obj.GetParameterMinRange(paramSound), obj.GetParameterMaxRange(paramSound)));
    }

    public override bool victoryState()
    {
        float value = obj.GetParameterValue(paramSound);
        if (value >= 4.0f && value <= 7.0f)
            return true;
        return false;
    }
}
