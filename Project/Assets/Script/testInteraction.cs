using UnityEngine;
using System.Collections;

public class testInteraction : MonoBehaviour {

	public Color ColorActivated = new Color();
	public Color ColorDesactivated = new Color();

	private bool m_activated = false;
	public bool activated
	{
		get { return m_activated;}
		set 
		{
			m_activated = !m_activated;
			if (m_activated)
				this.renderer.material.color = ColorActivated;
			else
				this.renderer.material.color = ColorDesactivated;
		}
	}

	// Use this for initialization
	void Start () 
	{
	
	}
	
	// Update is called once per frame
	void Update () 
	{
	}
	
	void VRAction()
	{
		Debug.Log("VRAction actived");
		activated = !activated;
	}
}
