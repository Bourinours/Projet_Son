using UnityEngine;
using System.Collections;

public class Potentio : Interaction
{
    public FmodEventAudioSource obj;
    public string paramSound = "param01";
    private int mWantedValue;

    private void Start()
    {
        mWantedValue = Random.Range(0, 4);
    }

    public void VRAction()
    {
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
        value += 1;
        obj.SetParameterValue(paramSound, Mathf.Clamp(value, obj.GetParameterMinRange(paramSound), obj.GetParameterMaxRange(paramSound)));
    }

    public override bool victoryState()
    {
        //if (value == mWantedValue)
        //  return true;
        return false;
    }
}
