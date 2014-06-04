/*
 * +-------------------------------------------------------------------------------------+
 *  Project: FMOD_Unity
 *  Description: A wrapper around the FMOD.EventSystem, and also the core of
 * 				 what happens between Unity and the FMOD Engine. Low level objects
 * 				 are all retrieved from this object, and it's made so that these object
 * 				 are as little accessible as possible.
 * 				 You should not access this class directly ; instead, you should
 * 				 instantiate a FmodEventSystemHandle and use its GetEventSystem()
 * 				 method to ensure proper initialization.
 *  Company: ISART Digital (www.isartdigital.com)
 *  Author: Galaad BROD (galaad.brod@gmail.com)
 * +-------------------------------------------------------------------------------------+
 */

using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

public class FmodEventSystem
{
	private static FmodEventSystem     						m_FmodEventSystem    = null;
	private static FmodEventSystemHolder					m_holder = null;
	private static FmodReverbManager						m_reverbManager = null;
	public static bool 										WasCleaned = false;
	
	public static void getEventSystem(FmodEventSystemHandle handle) {
		if (handle.getEventSystem() == null) {
			if (m_holder == null) {
				m_holder = GameObject.FindObjectOfType(typeof(FmodEventSystemHolder)) as FmodEventSystemHolder;
				if (m_holder == null) {
					UnityEngine.GameObject holderObject = new UnityEngine.GameObject("FmodEventSystemHolder");
					holderObject.AddComponent(typeof(FmodEventSystemHolder));
					m_holder = holderObject.GetComponent<FmodEventSystemHolder>();
				}
			}
			if (m_holder == null) {
				Debug.LogError("Could not find FmodEventSystemHolder; FMOD integration will not work");
			} else {
				if (m_FmodEventSystem == null) {
					m_FmodEventSystem = new FmodEventSystem();
					m_FmodEventSystem._createEventSystem();
						m_holder.SetFmodEventSystemHandle(new FmodEventSystemHandle());						
				}
				handle.setEventSystem(m_FmodEventSystem);
				GetReverbManager();
			}
		}
	}
	
	public static FmodEventSystemHolder getEventSystemHolder() {
		return (m_holder); 
	}
	
	public static FmodReverbManager GetReverbManager() {
		if (m_reverbManager == null) {
			m_reverbManager = GameObject.FindObjectOfType(typeof(FmodReverbManager)) as FmodReverbManager;
			if (m_reverbManager == null) {
				UnityEngine.GameObject holderObject = new UnityEngine.GameObject("FmodReverbManager");
				holderObject.AddComponent(typeof(FmodReverbManager));
				m_reverbManager = holderObject.GetComponent<FmodReverbManager>();						
			}
		}
		return (m_reverbManager);
	}	
	
	public static void ClearHolder() {
		if (m_holder != null) {
			m_holder.OnDestroy();
		}
	}
	//FMOD Error checking from return codes
	static void ERRCHECK(FMOD.RESULT result)
    {
		if (result == FMOD.RESULT.ERR_INITIALIZED)
		{
			Debug.LogError("FMOD_Unity: Reference to FMOD lost. Please restart Unity so FMOD can work again.");
		} else if (result != FMOD.RESULT.OK)
        {
            Debug.LogError("FMOD_Unity: FmodEventSystem: " + result + " - " + FMOD.Error.String(result));
        }
    }
	private const int									MAX_SOUND_BANKS_PER_FILE = 200;
	private FMOD.EventSystem   							m_eventSystem    = null; 
	private FmodEventPoolManager						m_eventPoolManager = new FmodEventPoolManager();
	private bool										m_eventSystemWasInit = false;
	private bool										m_eventSystemWasCleaned = false;
	private Dictionary<string, FMOD.EventProject> 		m_loadedProjects = new Dictionary<string, FMOD.EventProject>();
	private Dictionary<string, FMOD.EVENT_LOADINFO> 	m_loadInfos = new Dictionary<string, FMOD.EVENT_LOADINFO>();
	
	private AudioListener								m_audioListener = null;
	private Vector3										m_previousListenerPos = new Vector3();
//	private Vector3										m_previousListenerForward = new Vector3();
//	private Vector3										m_previousListenerUp = new Vector3();
	private float										m_previousListenerUpdateTime = 0;
	
	
	private FMOD.System									m_system = null;
	private FmodMusicSystem								m_musicSystem = null;
	
	protected void _createEventSystem() {
		if (m_eventSystem == null) {
			
			m_eventSystemWasCleaned = false;
			FMOD.RESULT result = FMOD.Event_Factory.EventSystem_Create(ref m_eventSystem);
			ERRCHECK(result);

			result = FMOD.Factory.System_Create(ref m_system);
			ERRCHECK(result);
			
			if (result == FMOD.RESULT.OK) {
#if UNITY_EDITOR
		        EditorApplication.playmodeStateChanged += cleanFmodBeforeChangingPlaymodeState;
#endif
			}
		}
	}
	
	private FMOD.EventSystem getEventSystem() {
		_createEventSystem();
		return (m_eventSystem);
	}
	
	public FmodMusicSystem getMusicSystem() {
		if (m_musicSystem == null) {
			if (m_eventSystem == null) {
				
			}
			FMOD.MusicSystem musicSystem = null;
			FMOD.RESULT result = m_eventSystem.getMusicSystem(ref musicSystem);
			ERRCHECK(result);
			m_musicSystem = new FmodMusicSystem(musicSystem);
		}
		return (m_musicSystem);
	}
		
	public FmodEventSystem() {
	}	
	
	public void init() {
		init(64, FMOD.INITFLAGS.NORMAL, (IntPtr)null, FMOD.EVENT_INITFLAGS.NORMAL | FMOD.EVENT_INITFLAGS.USE_GUIDS);
	}
	
	public void init(int maxChannels, FMOD.INITFLAGS flags, IntPtr extraDriverData, FMOD.EVENT_INITFLAGS eventFlags) {
		if (m_eventSystemWasInit == false) {
			m_eventSystemWasInit = true;
			FMOD.RESULT result = getEventSystem().init(maxChannels, flags, extraDriverData, eventFlags);
			ERRCHECK(result);
			
			result = m_system.init(maxChannels, flags, extraDriverData);
			ERRCHECK(result);
			
            result = m_system.set3DSettings(1.0f, 1.0f, 1.0f);
            ERRCHECK(result);
		} else {
			Debug.LogWarning ("FMOD_Unity: FmodEventSystem: FMOD Event System already init");
		}
	}
	
	protected void _getSoundBankNames(FmodEventAsset asset) {
		FMOD.EVENT_SYSTEMINFO 		sysinfo 		= new FMOD.EVENT_SYSTEMINFO();
		FMOD.EVENT_WAVEBANKINFO[] 	bankinfos 		= new FMOD.EVENT_WAVEBANKINFO[MAX_SOUND_BANKS_PER_FILE];
		IntPtr 						bankInfosPtr 	= Marshal.AllocHGlobal(Marshal.SizeOf(typeof(FMOD.EVENT_WAVEBANKINFO)) * bankinfos.Length);
		IntPtr 						c 				= new IntPtr(bankInfosPtr.ToInt32());
		FMOD.RESULT 				result 			= FMOD.RESULT.OK;
		List<string>				soundBankList = new List<string>();

		
		for (int i = 0; i < bankinfos.Length; i++)
		{
		    Marshal.StructureToPtr(bankinfos[i], c, true);
		    c = new IntPtr(c.ToInt32() + Marshal.SizeOf(typeof(FMOD.EVENT_WAVEBANKINFO)));
		}
		sysinfo.maxwavebanks = MAX_SOUND_BANKS_PER_FILE;
		sysinfo.wavebankinfo = bankInfosPtr;
		result = getEventSystem().getInfo(ref sysinfo);
		ERRCHECK (result);
		if (result == FMOD.RESULT.OK) {
			for (int i = 0; i < sysinfo.maxwavebanks; i++)
			{
				bankinfos[i] = (FMOD.EVENT_WAVEBANKINFO)Marshal.PtrToStructure(new IntPtr(bankInfosPtr.ToInt32() +
					i * Marshal.SizeOf(typeof(FMOD.EVENT_WAVEBANKINFO))), typeof(FMOD.EVENT_WAVEBANKINFO));
				string newString = new string(bankinfos[i].name);
				int index = newString.IndexOf('\0');
				string adjustedString = new string(bankinfos[i].name, 0, index);
				soundBankList.Add(adjustedString);
			}
			asset.setSoundBankList(soundBankList);
		}
	}
	
	protected void _loadFile(string assetPath, string assetName, ref FMOD.EVENT_LOADINFO loadInfo, ref FMOD.EventProject project) {
		FMOD.RESULT result = FMOD.RESULT.OK;
		string key = assetPath + "/" + assetName;
		
		if (m_loadedProjects.ContainsKey(key)) {
			loadInfo = m_loadInfos[key];
			project = m_loadedProjects[key];
			return ;
		}
		if (m_eventSystemWasInit == false) {
			init ();			
		}
		result = getEventSystem().setMediaPath(assetPath + "/");
		ERRCHECK(result);
		//load a .fev file exported from FMOD Designer
		result = getEventSystem().load(assetName, ref loadInfo, ref project); // we should only pass here once per file
		ERRCHECK(result);
		if (result == FMOD.RESULT.ERR_EVENT_ALREADY_LOADED) {
			throw new Exception("Error: Event file already loaded: " + assetPath + "/" + assetName);
		}
		if (result == FMOD.RESULT.OK) {
			m_loadedProjects[key] = project;
			m_loadInfos[key] = loadInfo;
		}
	}
	
	protected void _unloadFile(string assetPath, string assetName) {
		string key = assetPath + "/" + assetName;
		FMOD.RESULT 	result 			= FMOD.RESULT.OK;
		FMOD.EventProject project = null;
		
		if (m_loadedProjects.ContainsKey(key)) {
			project = m_loadedProjects[key];
			result = project.release();
			ERRCHECK(result);
			if (result == FMOD.RESULT.OK) {
				m_loadedProjects.Remove(key);
				m_loadInfos.Remove(key);
			}
		}
	}
	
	protected bool _unloadAllFiles() {
		bool filesUnloaded = false;
		FMOD.RESULT result = FMOD.RESULT.OK;
		string key;
		FMOD.EventProject project = null;
		List<string> toRemove = new List<string>();

		foreach (KeyValuePair<string, FMOD.EventProject> pair in m_loadedProjects) {
			key = pair.Key;
			project = pair.Value;
			if (project != null) {
				result = project.release();
				ERRCHECK(result);
				if (result == FMOD.RESULT.OK) {
					filesUnloaded = true;
					toRemove.Add(key);
				}
			}
		}
		foreach (string k in toRemove) {
			m_loadedProjects.Remove(k);
			m_loadInfos.Remove(k);
		}
		return (filesUnloaded);
	}
	
	public List<FmodEvent> loadEventsFromFile(string assetPath, string assetName, FmodEventAsset asset) {
        FMOD.EventGroup eventgroup     		= null;
		FMOD.RESULT 	result 				= FMOD.RESULT.OK;
		List<FmodEvent> events 				= new List<FmodEvent>();
		List<FmodEventGroup> eventGroups	= new List<FmodEventGroup>();
		int 			numGroups 			= 0;
		FmodEventGroup 		toAdd 			= null;
		FMOD.EVENT_LOADINFO loadInfo 		= new FMOD.EVENT_LOADINFO();
		FMOD.EventProject project 			= null;
		
		_loadFile(assetPath, assetName, ref loadInfo, ref project);
		_getSoundBankNames(asset);
		
		FMOD.EVENT_PROJECTINFO projectInfo = new FMOD.EVENT_PROJECTINFO();
		project.getInfo(ref projectInfo);
		asset.setProjectName(new string(projectInfo.name));
		
		result = project.getNumGroups(ref numGroups);
		ERRCHECK(result);
		for (int i = 0; i < numGroups; i++) {
			result = project.getGroupByIndex(i, false, ref eventgroup);
			ERRCHECK(result);
			toAdd = FmodEventGroup.CreateInstance("FmodEventGroup") as FmodEventGroup;
			toAdd.Initialize(eventgroup, null, asset);
			eventGroups.Add(toAdd);
			events.AddRange(toAdd.getAllEvents());
		}
		asset.setEventGroups(eventGroups);
		loadReverbsFromFile(asset);
		return (events);
	}

	protected void loadReverbsFromFile (FmodEventAsset asset)
	{
		List<FmodReverb> reverbs = new List<FmodReverb>();
		FMOD.REVERB_PROPERTIES curReverb = new FMOD.REVERB_PROPERTIES();
		IntPtr curReverbName = new IntPtr();
		string curReverbNameAsString;
		int numReverbs = 0;
		FMOD.RESULT result = FMOD.RESULT.OK;
		FmodReverb newReverb = null;
		
		result = getEventSystem().getNumReverbPresets(ref numReverbs);
		ERRCHECK(result);
		if (result == FMOD.RESULT.OK) {
			for (int i = 0; i < numReverbs; i++) {
				result = getEventSystem().getReverbPresetByIndex(i, ref curReverb, ref curReverbName);
				ERRCHECK(result);
				if (result == FMOD.RESULT.OK) {
					curReverbNameAsString = Marshal.PtrToStringAnsi(curReverbName);
					newReverb = ScriptableObject.CreateInstance(typeof(FmodReverb)) as FmodReverb;
					newReverb.Initialize(curReverbNameAsString, curReverb);
					reverbs.Add(newReverb);
				}
			}
		}
		asset.setReverbs(reverbs);
	}

	public void _loadEventGroup (FmodEvent evt)
	{
		if (evt.getEventGroup() == null || evt.getEventGroup().isInit()) {
			return ;
		}
		FMOD.RESULT result = FMOD.RESULT.OK;
		FMOD.EventGroup eventGroup = null;
		string groupName = evt.getEventGroup().getFullName();
		
		result = getEventSystem().getGroup(groupName, true, ref eventGroup);
		ERRCHECK(result);
		if (eventGroup != null) {
			result = eventGroup.loadEventData();
			ERRCHECK(result);
			if (result == FMOD.RESULT.OK) {
				evt.getEventGroup().setEventGroup(eventGroup);				
			}
		}
	}
	
	public bool wasCleaned() {
		return (m_eventSystemWasCleaned);
	}
		
	private FMOD.RESULT _loadEvent(FmodEvent evt, ref FMOD.Event fmodEvent) {
		string guidString = evt.getGUIDString();
		FMOD.RESULT result = FMOD.RESULT.OK;
		
		if (guidString != FmodEvent.EMPTY_GUIDSTRING) {
			result = getEventSystem().getEventByGUIDString(guidString, FMOD.EVENT_MODE.DEFAULT | FMOD.EVENT_MODE.ERROR_ON_DISKACCESS, ref fmodEvent);			
		} else {
			string fullName = evt.getFullName();
			
			result = getEventSystem().getEvent(fullName, FMOD.EVENT_MODE.DEFAULT | FMOD.EVENT_MODE.ERROR_ON_DISKACCESS, ref fmodEvent);
		}
		return (result);
	}
	
	public FMOD.RESULT loadEventFromFile(FmodEventAudioSource src) {
		FmodEvent evt = src.getSource();
		FmodEventAsset asset = evt.getAsset();
		FMOD.RESULT result = FMOD.RESULT.OK;
		FMOD.EVENT_LOADINFO loadInfo = new FMOD.EVENT_LOADINFO();
		FMOD.EventProject project = null;
		FMOD.Event fmodEvent = null;
		
		_loadFile(asset.getMediaPath(), asset.getName(), ref loadInfo, ref project);
		_loadEventGroup(evt);
		result = _loadEvent(evt, ref fmodEvent);
		ERRCHECK(result);
		if (result == FMOD.RESULT.OK) {
			src.SetEvent(fmodEvent);
		}
		return (result);
	}
	
	public void loadEvent(FmodEventAudioSource src) {
		if (src != null && src.getSource() != null) {
			FmodEventPool pool = m_eventPoolManager.getEventPool(src.getSource());
			
			ERRCHECK(pool.getEvent(src));
		}
	}
	
	public AudioListener getAudioListener() {
		if (m_audioListener == null) {
			AudioListener[] listeners = UnityEngine.Object.FindObjectsOfType(typeof(AudioListener)) as AudioListener[];
			
			if (listeners.Length == 0) {
				Debug.LogWarning("FMOD_Unity: FmodEventSystem: Could not find an AudioListener component in the scene. 3D sounds might not work.");
			} else {
				m_audioListener = listeners[0];
				m_previousListenerPos = m_audioListener.transform.position;
//				m_previousListenerForward = m_audioListener.transform.forward;
//				m_previousListenerUp = m_audioListener.transform.up;
				updateSystem(true);
				if (listeners.Length > 1) {
					Debug.LogWarning("FMOD_Unity: FmodEventSystem: More than one AudioListener component was found in the scene. Assuming the real one is the one on GameObject '" + m_audioListener.gameObject.name + "'");
				}
			}
		}
		return (m_audioListener);
	}
	
	public void updateSystem() {
		updateSystem(false);
	}

	public void updateSystem (bool force3DSet)
	{
		FMOD.RESULT result = FMOD.RESULT.OK;
		
		if (m_eventSystem != null && m_eventSystemWasInit) {
			if (getAudioListener() != null &&
				(force3DSet || (Time.timeSinceLevelLoad - m_previousListenerUpdateTime) > 0.001f)) {
					FMOD.VECTOR pos = new FMOD.VECTOR();
					FMOD.VECTOR vel = new FMOD.VECTOR();
					FMOD.VECTOR forward = new FMOD.VECTOR();
					FMOD.VECTOR up = new FMOD.VECTOR();
					
					pos.x = m_audioListener.transform.position.x;
					pos.y = m_audioListener.transform.position.y;
					pos.z = m_audioListener.transform.position.z;
					if (Time.timeSinceLevelLoad - m_previousListenerUpdateTime > 0) {
						vel.x = (pos.x - m_previousListenerPos.x) / (Time.timeSinceLevelLoad - m_previousListenerUpdateTime);
						vel.y = (pos.y - m_previousListenerPos.y) / (Time.timeSinceLevelLoad - m_previousListenerUpdateTime);
						vel.z = (pos.z - m_previousListenerPos.z) / (Time.timeSinceLevelLoad - m_previousListenerUpdateTime);
					}
					forward.x = m_audioListener.transform.forward.x;
					forward.y = m_audioListener.transform.forward.y;
					forward.z = m_audioListener.transform.forward.z;
					up.x = m_audioListener.transform.up.x;
					up.y = m_audioListener.transform.up.y;
					up.z = m_audioListener.transform.up.z;
					
					result = m_eventSystem.set3DListenerAttributes(0, ref pos, ref vel, ref forward, ref up);
					ERRCHECK(result);
					result = m_system.set3DListenerAttributes(0, ref pos, ref vel, ref forward, ref up);
					ERRCHECK(result);
					
					
					m_previousListenerPos = m_audioListener.transform.position;
//					m_previousListenerForward = m_audioListener.transform.forward;
//					m_previousListenerUp = m_audioListener.transform.up;
				m_previousListenerUpdateTime = Time.timeSinceLevelLoad;
			}
			result = m_eventSystem.update();
			ERRCHECK(result);
			result = m_system.update();
			ERRCHECK(result);
		}
	}
	
	public void setReverbImmediate(FMOD.REVERB_PROPERTIES targetProps) {
		FMOD.RESULT result = FMOD.RESULT.OK;
		FMOD.EventSystem evtSystem = getEventSystem();
		
		if (m_eventSystemWasInit == false) {
			init ();
		}
		result = evtSystem.setReverbProperties(ref targetProps);
		ERRCHECK(result);
		result = m_system.setReverbProperties (ref targetProps);
		ERRCHECK(result);
	}
	
	public void setReverb (FmodReverbZone reverbZone)
	{
		if (m_reverbManager != null) {
			m_reverbManager.StackReverb(reverbZone);
		}
	}
	
	public void unsetReverb(FmodReverbZone reverbZone) {
		if (m_reverbManager != null) {
			m_reverbManager.UnstackReverb(reverbZone);
		}
	}
	
	public void releaseRunningInstance(FmodEventAudioSource runningSource) {
		if (runningSource != null && runningSource.getSource() != null) {
			FmodEvent evt = runningSource.getSource();
			
			if (m_eventPoolManager.eventPoolExists(evt) == false) {
				Debug.LogError("CRITICAL ERROR: No pool was created for event " + evt.getName());
			} else {
				m_eventPoolManager.getEventPool(evt).releaseRunningInstance(runningSource);
			}
		}
	}
	
	public void releaseAllEventsFromGroup(FmodEventGroup grp) {
		if (grp != null) {
			foreach (FmodEvent evt in grp.getAllEvents()) {
				if (m_eventPoolManager.eventPoolExists(evt)) {
					m_eventPoolManager.getEventPool(evt).releaseAllHandles();
				}
			}
		}
	}
	
	public int getNumberRunningInstancesInGroup(FmodEventGroup grp) {
		if (grp == null) {
			return (0);
		}
		int total = 0;
		
		foreach (FmodEvent evt in grp.getAllEvents()) {
			if (m_eventPoolManager.eventPoolExists(evt)) {
				total += m_eventPoolManager.getEventPool(evt).getNumberRunningInstances();
			}
		}
		return (total);
	}
	
	public int getNumberRunningInstances(FmodEvent evt) {
		if (evt == null) {
			return (0);
		}
		if (m_eventPoolManager.eventPoolExists(evt) == false) {
			return (0);
		}
		return (m_eventPoolManager.getEventPool(evt).getNumberRunningInstances());
	}
	
 public void clean() {
        clean (true);
    }
   
    private void clean(bool checkForHandles) {
        int nbEventSystemHandles = FmodEventSystemHandle.NbHandles;
        if (m_eventSystem != null &&
            (checkForHandles == false || nbEventSystemHandles <= 1)) {
			
			List<FmodEventAudioSource> tmpList = m_eventPoolManager.getAllActiveSources();
			foreach (FmodEventAudioSource src in tmpList) {
				if (src != null) {
					src.Clean();
				}
			}
			if (m_musicSystem != null) {
				m_musicSystem.release();
				m_musicSystem = null;
			}
            if (_unloadAllFiles()) {
                ERRCHECK(m_eventSystem.unload());
            }
			if (m_eventSystem != null) {
	            ERRCHECK(m_eventSystem.release());
	            m_eventSystem = null;	
			}
           
			if (m_system != null) {
	            ERRCHECK(m_system.release());
	            m_system = null;				
			}
           
            m_eventSystemWasCleaned = true;
            m_eventSystemWasInit = false;
            WasCleaned = true;
			FmodEventSystem.m_FmodEventSystem = null;
        }
    }
   
    private void cleanFmodBeforeChangingPlaymodeState() {
#if UNITY_EDITOR
		if (EditorApplication.isPlaying && !EditorApplication.isPlayingOrWillChangePlaymode) {
			EditorApplication.playmodeStateChanged -= cleanFmodBeforeChangingPlaymodeState;
        	clean(false);
		}
#endif
    }
}

