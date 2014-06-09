using UnityEngine;
using System.Collections;

public class Boudoir : MonoBehaviour 
{
	public int wantedValue;
	public FmodEventAudioSource source;
	public GameObject charactere;
	public FmodEvent eventDialog;

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
		{
			if (eventDialog != null)
			{
				if (this.charactere == null)
					return true;
				GameManager tmp = this.charactere.GetComponent<GameManager>();
				if (tmp == null)
					return true;
				tmp.SetDialog(eventDialog, true);
				eventDialog = null;
			}
			return true;
		}
		return false;
	}
}
