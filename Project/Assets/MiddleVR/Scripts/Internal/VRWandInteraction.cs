/* VRWandInteraction
 * MiddleVR
 * (c) i'm in VR
 */

using UnityEngine;
using System.Collections;
using MiddleVR_Unity3D;

public class VRWandInteraction : MonoBehaviour {

	public GameObject WandRay;
	public string JoystickName = "RazerHydra.Joystick0";
    public float RayLength = 2;

    public bool Highlight = true;
    public Color HighlightColor = new Color();
    public Color GrabColor = new Color();

    public bool RepeatAction = false;

    public GameObject ObjectInHand = null;
    public GameObject CurrentObject = null;

    private vrJoystick m_Buttons = null;
	private uint m_MainButton = 0;
    private bool      m_SearchedButtons = false;

    private GameObject m_Ray = null;

	// Use this for initialization
	void Start () {
        m_Ray = WandRay;

        if (m_Ray != null)
        {
            m_Ray.transform.localScale = new Vector3( 0.01f, RayLength / 2.0f, 0.01f );
            m_Ray.transform.localPosition = new Vector3( 0,0, RayLength / 2.0f );
        }
	}

    private Collider GetClosestHit()
    {
        // Detect objects
        RaycastHit[] hits;
        Vector3 dir = transform.localToWorldMatrix * Vector3.forward;

        hits = Physics.RaycastAll(transform.position, dir, RayLength);

        int i = 0;
        Collider closest = null;
        float distance = Mathf.Infinity;

        while (i < hits.Length)
        {
            RaycastHit hit = hits[i];

            //print("HIT : " + i + " : " + hit.collider.name);

            if( hit.distance < distance && hit.collider.name != "VRWand" && hit.collider.GetComponent<VRActor>() != null )
            {
                distance = hit.distance;
                closest = hit.collider;
            }

            i++;
        }

        return closest;
    }
	
    private void HighlightObject( GameObject obj, bool state )
    {
        HighlightObject(obj, state, HighlightColor);
    }

    private void HighlightObject( GameObject obj, bool state, Color hCol )
    {
        GameObject hobj = m_Ray;

        if (hobj != null && hobj.renderer != null && Highlight)
        {
            if( state )
            {
				CurrentObject.renderer.material.color = hCol;
                hobj.renderer.material.color = hCol;
            }
            else
            {
				if (CurrentObject != null)
					CurrentObject.renderer.material.color = Color.white;
                hobj.renderer.material.color = Color.white;
            }
        }
    }

    private void Grab( GameObject iObject )
    {
		Interaction inte;

        //MiddleVRTools.Log("Take :" + CurrentObject.name);

        ObjectInHand = iObject;
        //ObjectInHand.transform.parent = transform.parent;
		inte = iObject.GetComponent<Interaction>();
		if (inte != null)
			inte.wandGrab = this.gameObject;
        HighlightObject(ObjectInHand, true, GrabColor);
    }

    private void Ungrab()
    {
		Interaction inte;
		//MiddleVRTools.Log("Release : " + ObjectInHand);

		//ObjectInHand.transform.parent = transform.parent;
		inte = ObjectInHand.GetComponent<Interaction>();
		if (inte != null)
			inte.wandGrab = null;
		ObjectInHand = null;
		HighlightObject(CurrentObject, false, HighlightColor);
        CurrentObject = null;
    }

	// Update is called once per frame
	void Update () 
	{
        Collider hit = GetClosestHit();

        if( hit != null )
        {
            //print("Closest : " + hit.name);

            if( CurrentObject != hit.gameObject &&  ObjectInHand == null )
            {
                //MiddleVRTools.Log("Enter other : " + hit.name);
                HighlightObject( CurrentObject, false );
                CurrentObject = hit.gameObject;
                HighlightObject(CurrentObject, true );
                //MiddleVRTools.Log("Current : " + CurrentObject.name);
            }
        }
        // Else
        else
        {
            //MiddleVRTools.Log("No touch ! ");

            if (CurrentObject != null && CurrentObject != ObjectInHand)
            {
                HighlightObject(CurrentObject, false, HighlightColor );
                CurrentObject = null;
            }
        }

        //MiddleVRTools.Log("Current : " + CurrentObject);

		if (m_Buttons == null)
		{
			m_Buttons = MiddleVR.VRDeviceMgr.GetJoystick(JoystickName);
		}

		if (m_Buttons == null)
		{
			if (m_SearchedButtons == false)
			{
				//MiddleVRTools.Log("[~] VRWandInteraction: Wand buttons undefined. Please specify Wand Buttons in the configuration tool.");
				m_SearchedButtons = true;
			}
		}

        if (m_Buttons != null && CurrentObject != null )
        {
            VRActor script = CurrentObject.GetComponent<VRActor>();

            //MiddleVRTools.Log("Trying to take :" + CurrentObject.name);
            if (script != null)
            {
                // Grab
                if (m_Buttons.IsButtonToggled(m_MainButton))
                {
                    if (script.Grabable)
                    {
                        Grab(CurrentObject);
                    }
                }

                // Release
                if (m_Buttons.IsButtonToggled(m_MainButton, false) && ObjectInHand != null)
                {
                    Ungrab();
                }

                // Action
                if (((!RepeatAction && m_Buttons.IsButtonToggled(m_MainButton)) || (RepeatAction&& m_Buttons.IsButtonPressed(m_MainButton))))
                {
                    CurrentObject.SendMessage("VRAction", SendMessageOptions.DontRequireReceiver);
                }
            }
        }
	}
}
