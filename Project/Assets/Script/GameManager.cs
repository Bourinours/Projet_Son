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
	private bool dialogue = false;
    
    private bool mEnd = false;
    private enum eState
    {
        _ETAP1_ = 0,
        _ETAP2_,
        _ETAP3_,
        _ETAP4_,
        _ETAP5_,
        _ETAP6_,
		_FINISH_
    };

	// Use this for initialization
	void Start () 
	{
        mState = eState._ETAP1_;
		this.SetDialog(mEtap1[0]);
	}
	
	// Update is called once per frame
	void Update () 
	{
		if (mState == eState._FINISH_)
			return;
        if (mBegin)
            begin();
        if (!mEnd)
            updateState();
        else
            end();
		if (source.CurrentStatus == FmodEventAudioSource.Status.Stopped)
			dialogue = false;
	}

    private void begin()
    {
		if (source.CurrentStatus == FmodEventAudioSource.Status.Stopped)
		{
			dialogue = false;
			mBegin = false;
			mObjects[0].desactiveActionMutex();
			mObjects[1].desactiveActionMutex();
		}
	}

    private void updateState()
    {
		if (mState == eState._ETAP1_)
		{
			etap1();
		}
		else if (mState == eState._ETAP2_)
		{
			if (dialogue == false)
			{
				mObjects[2].desactiveActionMutex();
				mObjects[3].desactiveActionMutex();
			}
			etap2();
		}
		else if (mState == eState._ETAP3_)
		{
			if (dialogue == false)
			{
				mObjects[4].desactiveActionMutex();
				mObjects[5].desactiveActionMutex();
			}
			etap3();
		}
		else if (mState == eState._ETAP4_)
		{
			if (dialogue == false)
			{
				mObjects[6].desactiveActionMutex();
				mObjects[7].desactiveActionMutex();
			}
			etap4();
		}
		else if (mState == eState._ETAP5_)
		{
			if (dialogue == false)
				mObjects[8].desactiveActionMutex();
			etap5();
		}
		else if (mState == eState._ETAP6_)
		{
			if (dialogue == false)
			{
				mObjects[9].desactiveActionMutex();
				mObjects[10].desactiveActionMutex();
			}
			etap6();
		}
    }

    private void etap1()
    {
        if (mObjects[0].victoryState() && mObjects[1].victoryState())
        {
            mObjects[0].activeActionMutex();
            mObjects[1].activeActionMutex();
			if (dialogue == false)
			{
				this.SetDialog(mEtap2[0]);
				mState = eState._ETAP2_;
			}
        }
    }

    private void etap2()
    {
        if (mObjects[2].victoryState() && mObjects[3].victoryState())
        {
            mObjects[2].activeActionMutex();
            mObjects[3].activeActionMutex();
			if (dialogue == false)
			{
				this.SetDialog(mEtap3[0]);
				mState = eState._ETAP3_;
			}
        }
    }

    private void etap3()
    {
        if (mObjects[4].victoryState() && mObjects[5].victoryState())
        {
            mObjects[4].activeActionMutex();
            mObjects[5].activeActionMutex();
			if (dialogue == false)
			{
				if (source.CurrentStatus == FmodEventAudioSource.Status.Stopped)
				{
					this.SetDialog(mEtap4[0]);
					mState = eState._ETAP4_;
				}
			}
        }
    }

    private void etap4()
    {
        if (mObjects[6].victoryState() && mObjects[7].victoryState())
        {
            mObjects[6].activeActionMutex();
            mObjects[7].activeActionMutex();
			if (dialogue == false)
			{
				this.SetDialog(mEtap5[0]);
				mState = eState._ETAP5_;
			}
        }
    }

    private void etap5()
    {
        if (mObjects[8].victoryState())
        {
            mObjects[8].activeActionMutex();
			if (dialogue == false)
			{
				this.SetDialog(mEtap6[0]);
				mState = eState._ETAP6_;
			}
        }
    }

    private void etap6()
    {
        if (mObjects[9].victoryState() && mObjects[10].victoryState())
        {
            mObjects[9].activeActionMutex();
            mObjects[10].activeActionMutex();
            mEnd = true;
        }
    }

    public void end(bool win = true)
    {
        Debug.Log("End");
		if (win)
		{
			source.Stop();
			source.SetSourceEvent(mEtap6[7]);
			source.Play();
		}
		else
		{
			mEnd = true;
			mObjects[9].activeActionMutex();
			mObjects[10].activeActionMutex();
			source.Stop();
			source.SetSourceEvent(mEtap6[3]);
			source.Play();
		}
		mState = eState._FINISH_;
    }

	public bool SetDialog(FmodEvent eventDialog, bool stop = false)
	{
		if (dialogue == false || stop)
		{
			source.Stop();
			source.SetSourceEvent(eventDialog);
			source.Play();
			dialogue = true;
			return true;
		}
		return false;
	}
}
