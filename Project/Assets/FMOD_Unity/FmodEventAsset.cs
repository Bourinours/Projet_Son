/*
 * +-------------------------------------------------------------------------------------+
 *  Project: FMOD_Unity
 *  Description: A processed storage for information stored in the .fev file.
 * 				 You can inspect the events and reverb by inspecting this asset.
 *  Company: ISART Digital (www.isartdigital.com)
 *  Author: Galaad BROD (galaad.brod@gmail.com)
 * +-------------------------------------------------------------------------------------+
 */

using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

public class FmodEventAsset : ScriptableObject {
	
	[SerializeField]
	protected string m_fileName;
	[SerializeField]
	protected string m_filePath;
	[SerializeField]
	protected string m_projectName;
	[SerializeField]
	protected System.DateTime m_dateTime;
	[SerializeField]
	protected List<FmodEvent> m_events;
	[SerializeField]
	protected List<FmodEventGroup> m_eventGroups;
	[SerializeField]
	protected List<string> m_soundBankList;
	[SerializeField]
	protected List<FmodReverb> m_reverbs;
	
	public void Initialize(string fileTotalPath) {
		FmodEventSystemHandle eventSystemHandle = new FmodEventSystemHandle();
		string filePath;
		string fileName;
		
		System.Text.RegularExpressions.Match match = System.Text.RegularExpressions.Regex.Match(fileTotalPath, @"(.*)/([^/]+)$");
		if (match.Success)
		{
			filePath = match.Groups[1].Value;
			fileName = match.Groups[2].Value;
		} else {
			throw new Exception("ERROR: Can't extract name and path to .fev from the string '" + fileTotalPath + "'");
		}
		m_fileName = fileName;
		m_filePath = filePath;
		m_dateTime = System.DateTime.UtcNow;
		name = m_filePath + "/" + m_fileName;
		m_events = eventSystemHandle.getEventSystem().loadEventsFromFile(filePath, fileName, this);
		eventSystemHandle.Dispose();
	}
	
	public void setEventGroups(List<FmodEventGroup> groups) {
		m_eventGroups = groups;
	}
	
	public void CreateAsset(string srcFile) {
#if UNITY_EDITOR
		string assetFile = srcFile + @".asset";

		AssetDatabase.CreateAsset(this, assetFile);
//		AssetDatabase.ImportAsset(assetFile, ImportAssetOptions.ImportRecursive); // force a save
		foreach (FmodEventGroup g in m_eventGroups) {
			g.CreateAsset(assetFile, this);
		}
//		AssetDatabase.ImportAsset(assetFile, ImportAssetOptions.ImportRecursive); // force a save
		foreach (FmodEvent e in m_events) {
			e.CreateAsset(assetFile, this);
		}
//		AssetDatabase.ImportAsset(assetFile, ImportAssetOptions.ImportRecursive); // force a save
		foreach (FmodReverb r in m_reverbs) {
			r.CreateAsset(assetFile, this);
		}
//		AssetDatabase.ImportAsset(assetFile, ImportAssetOptions.ImportRecursive); // force a save
#endif
	}
	
	/**
	 * This method returns the event in an updated .fev asset file that matches an event from the previous
	 * version of that .fev asset file.
	 * The matching is based on the GUID first, then the path through event groups and the name of the event.
	 * Returns the event or null if no match could be found.
	 */
	public FmodEvent getMatchingEvent (FmodEvent oldEvent)
	{
		string guidString = oldEvent.getGUIDString();
		
		if (guidString != "" && guidString != FmodEvent.EMPTY_GUIDSTRING) {
			foreach (FmodEvent e in m_events) {
				if (e.WasLoaded() && e.getGUIDString() != "" && e.getGUIDString() != FmodEvent.EMPTY_GUIDSTRING) {
					if (e.getGUIDString() == guidString) {
						return (e);
					}
				}
			}
		}
		string fullName = oldEvent.getFullName();
		
		foreach (FmodEvent e in m_events) {
			string tmp = e.getFullName();
			
			if (e.WasLoaded() && tmp == fullName) {
				return (e);
			}
		}
		return (null);
	}
	
	public FmodReverb getMatchingReverb(FmodReverb oldReverb) {
		List<FmodReverb> reverbs = getReverbs();
		
		foreach (FmodReverb r in reverbs) {
			if (r.getName() == oldReverb.getName()) {
				return (r);
			}
		}
		return (null);
	}
	
	public List<FmodEvent> getEvents() {
		return (m_events);
	}
	
	/// <summary>
	/// Gets the first event encountered based on the name of the event
	/// </summary>
	/// <returns>
	/// The wanted event, or null if not found
	/// </returns>
	/// <param name='name'>
	/// The name of the event, e.g. "BasicSoundWithLooping"
	/// </param>
	public FmodEvent getEventWithName(string name) {
		foreach (FmodEvent evt in getEvents()) {
			if (evt.getName() == name) {
				return (evt);
			}
		}
		return (null);
	}
	
	public FmodEvent getEventWithGUID(string GUID) {
		foreach (FmodEvent evt in getEvents()) {
			if (evt.getGUIDString() != FmodEvent.EMPTY_GUIDSTRING &&
				evt.getGUIDString() == GUID) {
				return (evt);
			}
		}
		return (null);		
	}
	
	private FmodEventGroup _getChildGroupWithName(List<FmodEventGroup> groups, string childName) {
		foreach (FmodEventGroup grp in groups) {
			if (grp.getName() == childName) {
				return (grp);
			}
		}
		return (null);
	}

	private FmodEvent _getEventFromListWithName(List<FmodEvent> events, string name) {
		foreach (FmodEvent evt in events) {
			if (evt.getName() == name) {
				return (evt);
			}
		}
		return (null);
	}
	/// <summary>
	/// Gets an event based on the full path from the root of the asset. A starting slash is not required.
	/// Double slashes, i.e. "Group1//Group2/Event" are ignored.
	/// </summary>
	/// <returns>
	/// The event, or null if not found.
	/// </returns>
	/// <param name='fullName'>
	/// Full name of an event, meaning path from the root of the asset, e.g. "FeatureDemonstration/Basics/BasicSoundWithLooping"
	/// </param>
	public FmodEvent getEventWithFullName(string fullName) {
		List<FmodEventGroup>	curChildrenGroups;
		List<FmodEvent>			curChildrenEvents;
		string[]				explodedPath;
		
		explodedPath = fullName.Split(new char[1] {'/'});
		curChildrenGroups = m_eventGroups;
		curChildrenEvents = m_events;
		
		for (int i = 0; i < explodedPath.Length - 1; i++) {
			string pathElement = explodedPath[i];
			
			if (pathElement != "") { // we skip null paths in things such as "elem1/elem2//elem3". Also, enables not caring about a starting /
				FmodEventGroup grp = _getChildGroupWithName(curChildrenGroups, pathElement);
				
				if (grp == null) {
					Debug.LogWarning("FmodEventAsset (" + getName() + "): getEventWithFullName: Could not find event group with name '" + pathElement + "'");
					return (null);
				}
				curChildrenGroups = grp.getChildrenGroups();
				curChildrenEvents = grp.getEvents();
			}
		}
		string eventName = explodedPath[explodedPath.Length - 1];
		FmodEvent ret = _getEventFromListWithName(curChildrenEvents, eventName);
		
		if (ret != null) {
			return (ret);
		}
		Debug.LogWarning("FmodEventAsset (" + getName() + "): getEventWithFullName: Could not find event with name '" + eventName + "'");		
		return (null);
	}
	
	public List<FmodEventGroup> getEventGroups() {
		return (m_eventGroups);
	}
	
	public string getName() {
		return (m_fileName);	
	}
	
	public string getProjectName ()
	{
		return (m_projectName);
	}
	
	public void setProjectName(string projectName) {
		if (m_projectName == null) {
			int length = projectName.IndexOf('\0');
			
			m_projectName = new string(projectName.ToCharArray(), 0, length);
		}
	}

	public string getMediaPath() {
		return (m_filePath);
	}

	public void setSoundBankList (List<string> soundBankList)
	{
		if (m_soundBankList == null) {
			m_soundBankList = soundBankList;			
		}
	}
	
	public List<string> getSoundBankList() {
		return (m_soundBankList);
	}
	
	public void setReverbs(List<FmodReverb> reverbs) {
		if (m_reverbs == null) {
			m_reverbs = reverbs;
		}
	}
	
	public List<FmodReverb> getReverbs() {
		return (m_reverbs);
	}
	
	//FMOD Error checking from return codes
	static void ERRCHECK(FMOD.RESULT result)
    {
        if (result != FMOD.RESULT.OK)
        {
            Debug.Log("FMOD error! " + result + " - " + FMOD.Error.String(result));
         }
    }
}
