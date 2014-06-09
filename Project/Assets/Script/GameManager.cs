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
    private bool mEnd = false;

	// Use this for initialization
	void Start () {
	
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
//        source.SetSourceEvent(mEvent[0]);
    }

    private void end()
    {
        Debug.Log("End");
    }

    private void disactiveObjects()
    {
        for (int i = 0; i < mObjects.Count; i++)
        {
            mObjects[i].enabled = false;
        }
    }
}
