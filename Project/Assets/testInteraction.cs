using UnityEngine;
using System.Collections;

public class testInteraction : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		if(MiddleVR.VRDeviceMgr.IsWandButtonToggled(1)){
			Debug.Log("YEAHI");
		}
	}
	
	void VRAction(){
		Debug.Log("coucou");	
	}
}
