/*
 * +-------------------------------------------------------------------------------------+
 *  Project: FMOD_Unity
 *  Description: A wrapper around FMOD.EventGroup. Contains FmodEvents and defines
 * 				 the hierarchy inside a FmodEventAsset, and enables us to retrieve
 * 				 FMOD.Events based on said hierarchy.
 *  Company: ISART Digital (www.isartdigital.com)
 *  Author: Galaad BROD (galaad.brod@gmail.com)
 * +-------------------------------------------------------------------------------------+
 */

using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

public class FmodEventGroup : ScriptableObject {
	//FMOD Error checking from return codes
	static void ERRCHECK(FMOD.RESULT result)
    {
        if (result != FMOD.RESULT.OK)
        {
            Debug.Log("FMOD error! " + result + " - " + FMOD.Error.String(result));
         }
    }
	[SerializeField]
	protected string m_name;
	[SerializeField]
	protected string m_projectName;
	[SerializeField]
	protected FmodEventGroup m_parent;
	[SerializeField]
	protected List<FmodEventGroup> m_children = new List<FmodEventGroup>();
	[SerializeField]
	protected List<FmodEvent> m_events = new List<FmodEvent>();
	[SerializeField]
	public bool showEventsInEditor = true;
	
	protected FMOD.EventGroup m_runtimeEventGroup = null;
	
	protected void 		_getName(FMOD.EventGroup eventgroup) {
		FMOD.RESULT 	result	= FMOD.RESULT.OK;
		int 			index	= 0;
		IntPtr			tmp		= new IntPtr(0);
		
		result = eventgroup.getInfo(ref index, ref tmp);
		ERRCHECK(result);
		m_name = Marshal.PtrToStringAnsi(tmp);
	}
	
	public void Initialize(FMOD.EventGroup eventgroup, FmodEventGroup parentGroup, FmodEventAsset asset) {
        FMOD.EventGroup childEventgroup     	= null;
		FMOD.RESULT 	result 			= FMOD.RESULT.OK;
		FmodEventGroup  child = null;
		int 			numChildrenGroups 		= 0;
		int 			numEvents 		= 0;
		FMOD.Event 		e 				= null;
		FmodEvent 		toAdd 			= null;

		hideFlags = HideFlags.HideInHierarchy;
		m_projectName = asset.getProjectName();
		m_parent = parentGroup;
		_getName(eventgroup);
		result = eventgroup.loadEventData();
		ERRCHECK(result);
		result = eventgroup.getNumEvents(ref numEvents);
		ERRCHECK(result);
		for (int j = 0; j < numEvents; j++) {
			e = null;
			result = eventgroup.getEventByIndex(j, FMOD.EVENT_MODE.DEFAULT | FMOD.EVENT_MODE.ERROR_ON_DISKACCESS, ref e);
			ERRCHECK(result);
			if (result != FMOD.RESULT.OK) {
				result = FMOD.RESULT.OK;
			}
			toAdd = FmodEvent.CreateInstance("FmodEvent") as FmodEvent;
			if (e != null) {
				toAdd.Initialize(e, this, j, asset);
				e.release();					
			} else {
				toAdd.Initialize(this, j, asset);
			}
			m_events.Add (toAdd);
		}
		result = eventgroup.freeEventData(false);
		ERRCHECK(result);
		result = eventgroup.getNumGroups(ref numChildrenGroups);
		for (int k = 0; k < numChildrenGroups; k++) {
			result = eventgroup.getGroupByIndex(k, false, ref childEventgroup);
			ERRCHECK(result);
			child = FmodEventGroup.CreateInstance("FmodEventGroup") as FmodEventGroup;
			child.Initialize(childEventgroup, this, asset);
			m_children.Add(child);
		}
		name = getFullName();
	}
	
	public void CreateAsset(string assetFile, FmodEventAsset asset) {
#if UNITY_EDITOR
		AssetDatabase.AddObjectToAsset(this, asset);
//		AssetDatabase.ImportAsset(assetFile); // force a save
		foreach (FmodEventGroup g in m_children) {
			g.CreateAsset(assetFile, asset);
		}
#endif
	}

	
	public string getName() {
		return (m_name ?? "/");
	}
	
	public string getFullName() {
		string ret = string.Empty;
		
		if (m_parent != null) {
			ret += m_parent.getFullName() + "/";
		} else if (m_projectName != null && m_projectName != "") {
			ret = String.Format("{0}/", m_projectName);
		}
		return ret + getName();
	}
	
	public List<FmodEvent> getAllEvents() {
		List<FmodEvent> ret = new List<FmodEvent>();
		
		ret.AddRange(m_events);
		foreach (FmodEventGroup child in m_children) {
			ret.AddRange(child.getAllEvents());
		}
		return (ret);
	}
	
	public List<FmodEvent> getEvents() {
		return (m_events);
	}
	
	public List<FmodEventGroup> getChildrenGroups() {
		return (m_children);
	}
	
	public bool isInit() {
		return (m_runtimeEventGroup != null);
	}
	
	public void setEventGroup(FMOD.EventGroup eventGroup) {
		if (!isInit()) {
			m_runtimeEventGroup = eventGroup;
		}
	}
	
	public FMOD.EventGroup getRuntimeEventGroup() {
		return (m_runtimeEventGroup);
	}
	
	public void freeWholeGroup() {
		if (m_runtimeEventGroup != null) {
			ERRCHECK(m_runtimeEventGroup.freeEventData(false));
			using (FmodEventSystemHandle handle = new FmodEventSystemHandle()) {
				if (handle.getEventSystem() != null && handle.getEventSystem().wasCleaned() == false) {
					handle.getEventSystem().releaseAllEventsFromGroup(this);
				}
			}
			m_runtimeEventGroup = null;
		}
	}
}
