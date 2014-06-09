using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour {

    public FmodEventAudioSource source;
    public List<Interaction> mObjects;
    public List<FmodEvent> mEtap1;
    public List<FmodEvent> mEtap2;
    public List<FmodEvent> mEtap3;
    public List<FmodEvent> mEtap4;
    public List<FmodEvent> mEtap5;
    public List<FmodEvent> mEtap6;
    private eState mState;
    
    private bool mEnd = false;
    private enum eState
    {
        _ETAP1_ = 0,
        _ETAP2_,
        _ETAP3_,
        _ETAP4_,
        _ETAP5_,
        _ETAP6_
    };

	// Use this for initialization
	void Start () {
        mState = eState._ETAP1_;
	}
	
	// Update is called once per frame
	void Update () {
        if (!mEnd)
            updateState();
        else
            end();
	}

    private void updateState()
    {
        if (mState == eState._ETAP1_)
            etap1();
        else if (mState == eState._ETAP2_)
            etap2();
        else if (mState == eState._ETAP3_)
            etap3();
        else if (mState == eState._ETAP4_)
            etap4();
        else if (mState == eState._ETAP5_)
            etap5();
        else if (mState == eState._ETAP6_)
            etap6();
//        source.SetSourceEvent(mEvent[0]);
    }

    private void etap1()
    {

    }

    private void etap2()
    {

    }

    private void etap3()
    {

    }

    private void etap4()
    {

    }

    private void etap5()
    {

    }

    private void etap6()
    {

    }

    private void end()
    {
        Debug.Log("End");
    }
}
