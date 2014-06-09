using UnityEngine;
using System.Collections;

public class Boudoir : MonoBehaviour 
{
	public int wantedValue;
	public FmodEventAudioSource source;

	// Use this for initialization
	void Start () 
	{
		int tmp = Random.Range(0, 10);

		wantedValue = Random.Range(0, 10);
		while (tmp == wantedValue)
			tmp = Random.Range(0, 10);
		source.SetParameterValue("param01", tmp);
	}

	public bool CheckValue()
	{
		if (source != null && source.GetParameterValue("param01") == wantedValue)
			return true;
		return false;
	}
}
