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
    private bool mBegin = true;
    
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
	void Start () 
	{
        mState = eState._ETAP1_;
	}
	
	// Update is called once per frame
	void Update () 
	{
        if (mBegin)
            begin();
        if (!mEnd)
            updateState();
        else
            end();
	}

    private void begin()
    {
        source.SetSourceEvent(mEtap1[0]);
		source.Play();
        mBegin = false;
		mObjects[0].desactiveActionMutex();
		mObjects[1].desactiveActionMutex();
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
    }

    private void etap1()
    {
        if (mObjects[0].victoryState() && mObjects[1].victoryState())
        {
            mObjects[0].activeActionMutex();
            mObjects[1].activeActionMutex();
            source.Stop();
            source.SetSourceEvent(mEtap2[0]);
            source.Play();
            mState = eState._ETAP2_;
			mObjects[2].desactiveActionMutex();
			mObjects[3].desactiveActionMutex();
        }
    }

    private void etap2()
    {
        if (mObjects[2].victoryState() && mObjects[3].victoryState())
        {
            mObjects[2].activeActionMutex();
            mObjects[3].activeActionMutex();
            source.Stop();
            source.SetSourceEvent(mEtap3[0]);
            source.Play();
            mState = eState._ETAP3_;
			mObjects[4].desactiveActionMutex();
			mObjects[5].desactiveActionMutex();
        }
    }

    private void etap3()
    {
        if (mObjects[4].victoryState() && mObjects[5].victoryState())
        {
            mObjects[4].activeActionMutex();
            mObjects[5].activeActionMutex();
            source.Stop();
            source.SetSourceEvent(mEtap4[0]);
            source.Play();
            mState = eState._ETAP4_;
			mObjects[6].desactiveActionMutex();
			mObjects[7].desactiveActionMutex();
        }
    }

    private void etap4()
    {
        if (mObjects[6].victoryState() && mObjects[7].victoryState())
        {
            mObjects[6].activeActionMutex();
            mObjects[7].activeActionMutex();
            source.Stop();
            source.SetSourceEvent(mEtap5[0]);
            source.Play();
            mState = eState._ETAP5_;
			mObjects[8].desactiveActionMutex();
        }
    }

    private void etap5()
    {
        if (mObjects[8].victoryState())
        {
            mObjects[8].activeActionMutex();
            source.Stop();
            source.SetSourceEvent(mEtap6[0]);
            source.Play();
            mState = eState._ETAP6_;
			mObjects[9].desactiveActionMutex();
			mObjects[10].desactiveActionMutex();
        }
    }

    private void etap6()
    {
        if (mObjects[9].victoryState() && mObjects[10].victoryState())
        {
            mObjects[9].activeActionMutex();
            mObjects[10].activeActionMutex();
            mState = eState._ETAP1_;
            source.Stop();
            source.SetSourceEvent(mEtap6[7]);
            source.Play();
            mEnd = true;
        }
    }

    private void end()
    {
        Debug.Log("End");
    }
}
