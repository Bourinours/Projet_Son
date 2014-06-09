using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour {

    public List<Interaction> mObjects;
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
        if (mObjects.Count <= 0)
            return;
        for (int i = 0; mObjects[i].victoryState(); i++)
        {
            if (i == (mObjects.Count - 1))
            {
                mEnd = true;
                disactiveObjects();
                break;
            }
        }
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
