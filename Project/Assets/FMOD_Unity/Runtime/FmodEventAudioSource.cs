/*
 * +-------------------------------------------------------------------------------------+
 *  Project: FMOD_Unity
 *  Description: Represents an audio source, taking a FmodEvent from a FmodEventAsset
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

public class FmodEventAudioSource : MonoBehaviour
{
	public delegate FMOD.RESULT EVENT_CALLBACK (FmodEventAudioSource audioSource, FMOD.EVENT_CALLBACKTYPE type, IntPtr param1, IntPtr param2);
	public delegate FMOD.RESULT EVENT_CALLBACK_EVENTSTARTED (FmodEventAudioSource audioSource);
	public delegate FMOD.RESULT EVENT_CALLBACK_EVENTFINISHED (FmodEventAudioSource audioSource);
	public delegate FMOD.RESULT EVENT_CALLBACK_EVENTSTOLEN (FmodEventAudioSource audioSource);
	public delegate FMOD.RESULT EVENT_CALLBACK_SOUNDDEFSTART (FmodEventAudioSource audioSource, string soundDefName, int waveIndexInSoundDef);
	public delegate FMOD.RESULT EVENT_CALLBACK_SOUNDDEFEND (FmodEventAudioSource audioSource, string soundDefName, int waveIndexInSoundDef);
	
	
	/// <summary>
	/// Occurs at several points in an event's life. These occurences are :
	/// <list type="bullet">
	/// 	<item><term>FMOD_EVENT_CALLBACKTYPE_SYNCPOINT				</term><description>	Called when a syncpoint is encountered. Can be from wav file markers. </description></item>
	///     <item><term>FMOD_EVENT_CALLBACKTYPE_SOUNDDEF_START			</term><description>	Called when a sound definition inside an event is triggered. </description></item>
	///     <item><term>FMOD_EVENT_CALLBACKTYPE_SOUNDDEF_END<			</term><description>	Called when a sound definition inside an event ends or is stopped. </description></item>
	///     <item><term>FMOD_EVENT_CALLBACKTYPE_STOLEN					</term><description>	Called when a getEventXXX call steals an event instance that is in use. </description></item>
	///     <item><term>FMOD_EVENT_CALLBACKTYPE_EVENTFINISHED			</term><description>	Called when an event is stopped for any reason. </description></item>
	///     <item><term>FMOD_EVENT_CALLBACKTYPE_NET_MODIFIED			</term><description>	Called when a property of the event has been modified by a network-connected host. </description></item>
	///     <item><term>FMOD_EVENT_CALLBACKTYPE_SOUNDDEF_CREATE			</term><description>	Called when a programmer sound definition entry is loaded. </description></item>
	///     <item><term>FMOD_EVENT_CALLBACKTYPE_SOUNDDEF_RELEASE		</term><description>	Called when a programmer sound definition entry is unloaded. </description></item>
	///     <item><term>FMOD_EVENT_CALLBACKTYPE_SOUNDDEF_INFO			</term><description>	Called when a sound definition entry is loaded. </description></item>
	///     <item><term>FMOD_EVENT_CALLBACKTYPE_EVENTSTARTED			</term><description>	Called when an event is started. </description></item>
	///     <item><term>FMOD_EVENT_CALLBACKTYPE_SOUNDDEF_SELECTINDEX	</term><description>	Called when a sound definition entry needs to be chosen from a "ProgrammerSelected" sound definition. </description></item>
	///     <item><term>FMOD_EVENT_CALLBACKTYPE_OCCLUSION				</term><description>	Called when an event's channel is occluded with the geometry engine. </description></item>
	/// </list>
	/// </summary>
	public event EVENT_CALLBACK					EventCallback;
	/// <summary>
	/// An FMOD_EVENT_CALLBACKTYPE_EVENTSTARTED callback is generated whenever an event is started. This callback will be called before any sounds in the event have begun to play.
	/// </summary>
	public event EVENT_CALLBACK_EVENTSTARTED	EventStarted;
	/// <summary>
	/// An FMOD_EVENT_CALLBACKTYPE_EVENTFINISHED callback is generated whenever an event is stopped for any reason including when the user calls FmodEvent::Stop().
	/// </summary>
	public event EVENT_CALLBACK_EVENTFINISHED	EventFinished;
	/// <summary>
	/// An FMOD_EVENT_CALLBACKTYPE_STOLEN callback is generated when a getEventXXX call needs to steal an event instance that is in use because 
    /// the event's "Max playbacks" has been exceeded. This callback is called before the event is stolen and before the event 
    /// is stopped (if it is playing). An FMOD_EVENT_CALLBACKTYPE_EVENTFINISHED callback will be generated when the stolen event is stopped i.e. <b>after</b>
    /// the FMOD_EVENT_CALLBACKTYPE_STOLEN. If the callback function returns FMOD_ERR_EVENT_FAILED, the event will <b>not</b>
    /// be stolen, and the returned value will be passed back as the return value of the getEventXXX call that triggered the steal attempt.
	/// </summary>
	public event EVENT_CALLBACK_EVENTSTOLEN		EventStolen;
	/// <summary>
	/// <para>An FMOD_EVENT_CALLBACKTYPE_SOUNDDEF_START callback is generated each time a sound definition is played in an event.</para>
    /// <para>This happens every time a sound definition starts due to the event parameter entering the region specified in the 
    /// layer created by the sound designer.</para>
    /// <para>This also happens when sounds are randomly respawned using the random respawn feature in the sound definition 
    /// properties in FMOD designer.</para>
	/// <param name="name">
	/// name of sound definition being started
	/// </param>
	/// <param name="waveIndexInSoundDef">
	/// index of wave being started inside sound definition (ie for multi wave sound definitions)
	/// </param>
	/// </summary>
	public event EVENT_CALLBACK_SOUNDDEFSTART	SoundDefStart;
	/// <summary>
	/// An FMOD_EVENT_CALLBACKTYPE_SOUNDDEF_END callback is generated when a one-shot sound definition inside an event ends, 
    /// or when a looping sound definition stops due to the event parameter leaving the region specified in the layer created 
    /// by the sound designer.
	/// </summary>
	/// <param name="name">
	/// name of sound definition being stopped
	/// </param>
	/// <param name="waveIndexInSoundDef">
	/// index of wave being stopped inside sound definition (ie for multi wave sound definitions)
	/// </param>
	public event EVENT_CALLBACK_SOUNDDEFEND		SoundDefEnd;
	
	static List<FmodEventAudioSource> m_allSources = new List<FmodEventAudioSource>();
	public static List<FmodEventAudioSource> getAllAudioSources() {
		return (m_allSources);
	}
		
	[SerializeField]
	protected FmodEventAudioClip m_eventClip = null;
	[SerializeField]
	protected FmodEvent.SourceType m_type;
	public FmodEventAudioClip eventClip {
		get	{ return m_eventClip; }
		set {
			if (value != null) {
				FmodEventAudioClip newValue = value as FmodEventAudioClip;
				m_type = newValue.getSourceType();
			}
			m_eventClip = value;
			m_source = m_eventClip.getSource();
		}
	}
	public FmodEvent.SourceType type { get { return (m_type); } }
	public bool playOnAwake = true;
	
	[SerializeField]
	private string m_pathToAssetFile;
	[SerializeField]
	private string m_sourceEventFullName;
	[SerializeField]
	private string m_sourceEventGUID;
	
	public void UpdateRestorationData() {
		if (m_source != null) {
#if UNITY_EDITOR
			FmodEventAsset asset = m_source.getAsset();
			string tmp = AssetDatabase.GetAssetPath(asset);
			
			if (tmp != null && tmp != "") {
				m_pathToAssetFile = tmp;
			}
			m_sourceEventFullName = m_source.getFullName();
			m_sourceEventGUID = m_source.getGUIDString();
#endif
		}
	}
	public void RestoreAsset() {
		if (m_source == null && m_pathToAssetFile != null) {
#if UNITY_EDITOR
			FmodEventAsset asset = AssetDatabase.LoadMainAssetAtPath(m_pathToAssetFile) as FmodEventAsset;
			
			if (asset == null) {
				Debug.LogWarning("FmodEventAudioSource (" + name + "): Error while restoring source: could not find asset at path '" + m_pathToAssetFile + "'.");
			} else {
				FmodEvent evt = null;
				
				if (m_sourceEventGUID != null && m_sourceEventGUID != "" && m_sourceEventGUID != FmodEvent.EMPTY_GUIDSTRING) {
					evt = asset.getEventWithGUID(m_sourceEventGUID);
				}
				if (evt == null && m_sourceEventFullName != null && m_sourceEventFullName != "") {
					evt = asset.getEventWithFullName(m_sourceEventFullName);
				}
				if (evt == null) {
					Debug.LogWarning("FmodEventAudioSource (" + name + "): Error while restoring source: could not find event with GUID '" + m_sourceEventFullName + "' or at '" + m_sourceEventFullName + "'");
				} else {
					SetSourceEvent(evt);
				}
			}
#endif			
		}
	}
	
	public void Play() {
		CheckForOldFormat();
		if (m_source != null) {
			FMOD.RESULT result = FMOD.RESULT.OK;
			if (m_runtimeEvent == null) {
				if (m_eventSystemHandle == null) {
					m_eventSystemHandle = new FmodEventSystemHandle();					
				}
				m_eventSystemHandle.getEventSystem().loadEvent(this);
			}
			if (m_runtimeEvent != null) {
				SetVolume(m_volume);
				if (m_runtimeEvent != null) {// runtime event could be null after SetVolume if max_callback as been reached
					m_status = Status.Playing;
					if (Paused == false) {
						Unpause();
						result = m_runtimeEvent.start();
						ERRCHECK(result);
						if (result != FMOD.RESULT.OK) {
							m_status = Status.Stopped;
						}
					}
				}
			} else {
				m_eventSystemHandle.Dispose();
				m_eventSystemHandle = null;
			}
		} else {
			Debug.LogWarning("FMOD_Unity: FmodEventAudioSource '" + gameObject.name + "': Could not play : no sound set.");
		}
	}
	
	public void Stop() {
		if (m_source != null) {
			if (m_runtimeEvent == null) {
				Debug.LogWarning("FMOD_Unity : Event '" + m_source.getName() + "' was stopped but was not started.");
			} else {
				m_runtimeEvent.stop(true);
				m_status = Status.Stopped;
			}
		}
	}
	
	public void Pause() {
		if (m_source != null) {
			if (m_runtimeEvent != null) {
				if (m_eventSystemHandle.getEventSystem().wasCleaned() == false) {
					m_runtimeEvent.setPaused(true);
					m_paused = true;
				}
			}		
		}
	}

	public void Unpause() {
		if (m_source != null) {
			if (m_runtimeEvent != null) {
				if (m_eventSystemHandle.getEventSystem().wasCleaned() == false) {
					m_runtimeEvent.setPaused(false);
					m_paused = false;
				}
			}		
		}
	}
	
	protected FmodRuntimeEventParameter getParameter(string parameterName) {
		return (getParameter(parameterName, true));
	}
	
	protected FmodRuntimeEventParameter getParameter(string parameterName, bool showMessages) {
		if (m_source != null) {
			List<FmodRuntimeEventParameter> paramList = getParameters();
			
			foreach (FmodRuntimeEventParameter p in paramList) {
				if (p.getName() == parameterName) {
					return (p);
				}
			}
			if (showMessages) {
				Debug.LogWarning("FMOD_Unity: FmodEventAudioSource (" + gameObject.name + "): parameter '" + parameterName + "' was not found.");			
			}
			return (null);
		} else {
			if (showMessages) {
				Debug.LogWarning("FMOD_Unity: FmodEventAudioSource (" + gameObject.name + "): parameter '" + parameterName + "' can't exist since no event is set.");
			}
			return (null);
		}
	}
	
	public bool ParameterExists(string parameterName) {
		return (getParameter(parameterName, false) != null);
	}
	
	public float GetParameterValue(string parameterName) {
		FmodRuntimeEventParameter p = getParameter(parameterName);
		
		if (p == null) {
			return (0);
		}
		return (p.getUnderlyingValue());
	}

	public void KeyOffOnParameter(string parameterName) {
		FmodRuntimeEventParameter p = getParameter(parameterName);
		
		if (p == null) {
			return ;
		}
		p.KeyOff();
	}
	
	public float GetParameterMinRange(string parameterName) {
		FmodRuntimeEventParameter p = getParameter(parameterName);
		
		if (p == null) {
			return (0);
		}
		return (p.MinRange);		
	}
	
	public float GetParameterMaxRange(string parameterName) {
		FmodRuntimeEventParameter p = getParameter(parameterName);
		
		if (p == null) {
			return (0);
		}
		return (p.MaxRange);
	}
	
	public bool SetParameterValue(string parameterName, float val) {
		FmodRuntimeEventParameter p = getParameter(parameterName);
		
		if (p == null) {
			return (false);
		}
		p.SetValue(val);
		return (true);
	}

	public string getStatus() {
		if (m_source == null) {
			return ("Stopped");
		}
		FmodEventAudioSource.Status status = CurrentStatus;
		bool paused = Paused;
		
		return ((paused ? "Paused & " : "") + (status == FmodEventAudioSource.Status.Playing ? "Playing" : "Stopped"));
	}
	
	/**
	 * Returns the event volume, between 0 and 100
	 */
	public float GetVolume() {
		if (m_source != null) {
			return (m_volume);
		} else {
			Debug.LogWarning("FMOD_Unity: FmodEventAudioSource (" + gameObject.name + "): can't get volume since no event is set.");
			return (0);
		}
	}
	
	/**
	 * Sets the volume of the event. Value for volume should be between 0 and 100.
	 */
	public bool SetVolume(float volume) {
		CheckForOldFormat();
		if (m_source != null) {
			m_volume = (int)Mathf.Clamp(volume, 0, 100);
			if (m_runtimeEvent != null) {
				FMOD.RESULT result = FMOD.RESULT.OK;
				
				if (volume < 1) {
					volume = 0;
				}
				result = m_runtimeEvent.setVolume(volume / 100);
				ERRCHECK(result);
			}
			return (true);
		} else {
			Debug.LogWarning("FMOD_Unity: FmodEventAudioSource (" + gameObject.name + "): can't set volume since no event is set.");
			return (false);
		}		
	}
	
	public void OnDrawGizmos() {
		if (m_source == null) {
			Gizmos.DrawIcon(transform.position, "speaker_icon_error.png");
		} else {
			Gizmos.DrawIcon(transform.position, "speaker_icon.png");			
		}
	}
			
	public void CheckForOldFormat() {
		if (m_source == null && m_eventClip != null) {
			m_source = m_eventClip.getSource();
		}		
	}
	// Use this for initialization
	void Start ()
	{
		CheckForOldFormat();
		if (m_source != null) {
#if UNITY_EDITOR
			EditorApplication.playmodeStateChanged += cleanBeforeChangingPlaymodeState;
#endif
			if (playOnAwake) {
				Play();
			}			
		}
	}
	
	// Update is called once per frame
	void Update ()
	{
		if (m_source != null) {
			UpdateEvent(transform.position, Time.timeSinceLevelLoad);
		}
	}
	
	public void OnDisable() {
		Pause();
	}

	public void OnEnable() {
		Unpause();
	}
	
	
	// ---------------------------------------------------------
	
	
	[SerializeField]
	[Range(0, 100000)]
	public float m_minRange; // only makes sense for 3D sounds - those with m_sourceType == 3D
	[SerializeField]
	[Range(0, 100000)]
	public float m_maxRange; // only makes sense for 3D sounds - those with m_sourceType == 3D
	[SerializeField]
	protected float m_volume = 100;

	protected FMOD.Event m_runtimeEvent = null;
	protected GCHandle m_selfHandle;
	protected FmodEventSystemHandle m_eventSystemHandle = null;
	protected Vector3 m_previousPos = new Vector3();
	protected float m_previousUpdateTime = 0;
	
	public enum Status {
		Stopped,
		Playing
	}
	protected Status m_status;
	public Status CurrentStatus { get { return (m_status); } }
	protected bool m_paused = false;
	public bool Paused { get { return (m_paused); } }	
	
	[SerializeField]
	private FmodEvent m_source;
	[SerializeField]
	private List<FmodRuntimeEventParameter> m_parameters;

	public void Initialize(FmodEvent sourceEvent) {
		m_source = sourceEvent;
		name = m_source.name;
	}
	
	public FmodEvent.SourceType getSourceType() {
		if (m_source == null) {
			Debug.Log ("LOOK HERE ! This should not be null when reimporting !");
		}
		return (m_source.getSourceType());	
	}
	
	public List<FmodRuntimeEventParameter> getParameters() {
		int nbParamsInRuntimeParams = 0;
		
		foreach (FmodRuntimeEventParameter p in m_parameters) {
			if (p != null) {
				nbParamsInRuntimeParams++;
			}
		}
		if ((m_parameters == null || nbParamsInRuntimeParams == 0) &&
			m_source != null && m_source.getParameters().Count > 0) {
			SetSourceEvent(m_source);
		}
		return (m_parameters);
	}
	
	public FmodEvent getSource() {
		return (m_source);
	}
	
	public float getMinRange ()
	{
		return (m_minRange);
	}
	
	public float getMaxRange ()
	{
		return (m_maxRange);
	}
	
	public void setMinRange (float newValue)
	{
		if (newValue > m_maxRange) {
			newValue = m_maxRange;
		}
		m_minRange = newValue;
		if (m_runtimeEvent != null && getSourceType() == FmodEvent.SourceType.SOURCE_3D) {
			float[] buffer = { newValue };
			IntPtr tmp = Marshal.AllocHGlobal(sizeof(float));
			FMOD.RESULT result = FMOD.RESULT.OK;
		
			Marshal.Copy(buffer, 0, tmp, 1);
			result = m_runtimeEvent.setPropertyByIndex((int)FMOD.EVENTPROPERTY._3D_MINDISTANCE, tmp, true);
			ERRCHECK(result);
			Marshal.FreeHGlobal(tmp);
		}
	}
	
	public void setMaxRange (float newValue)
	{
		if (newValue < m_minRange) {
			newValue = m_minRange;
		}
		m_maxRange = newValue;
		if (m_runtimeEvent != null && getSourceType() == FmodEvent.SourceType.SOURCE_3D) {
			float[] buffer = { newValue };
			IntPtr tmp = Marshal.AllocHGlobal(sizeof(float));
			FMOD.RESULT result = FMOD.RESULT.OK;
		
			Marshal.Copy(buffer, 0, tmp, 1);
			result = m_runtimeEvent.setPropertyByIndex((int)FMOD.EVENTPROPERTY._3D_MAXDISTANCE, tmp, true);
			ERRCHECK(result);

			Marshal.FreeHGlobal(tmp);
		}
	}
	
	public void SetSourceEvent(FmodEvent srcEvent) {
		if (m_source == srcEvent) {
			return ;
		}
		CleanRuntimeEvent();
		m_source = srcEvent;
		if (m_source != null) {
			UpdateRestorationData();
			m_type = m_source.getSourceType();
			setMaxRange(m_source.m_maxRange);
			setMinRange(m_source.m_minRange);
			if (m_parameters != null) {
				CleanAndRemoveParameters();
			} else {
				m_parameters = new List<FmodRuntimeEventParameter>();
			}
			foreach (FmodEventParameter p in m_source.getParameters()) {
				FmodRuntimeEventParameter runtimeParam = gameObject.AddComponent<FmodRuntimeEventParameter>();
				runtimeParam.Initialize(p);
				m_parameters.Add(runtimeParam);
			}
		} else {
			CleanAndRemoveParameters();
		}
	}
	
	public void SetEvent(FMOD.Event evt) {
		if (evt == null) {
			if (m_runtimeEvent != null) {
				if (m_eventSystemHandle != null &&
					m_eventSystemHandle.getEventSystem() != null &&
					m_eventSystemHandle.getEventSystem().wasCleaned() == false) {
					Stop();
					CleanParameters();
					m_eventSystemHandle.getEventSystem().releaseRunningInstance(this);		
				}
				freeEventData();
				m_runtimeEvent = null;
			}
		} else {
			m_runtimeEvent = evt;
			foreach (FmodRuntimeEventParameter param in getParameters()) {
				param.SetEvent(m_runtimeEvent);
			}
			setMinRange(getMinRange());
			setMaxRange(getMaxRange());
			if (! m_selfHandle.IsAllocated) {
				m_selfHandle = GCHandle.Alloc(this, GCHandleType.Normal);
			}
			ERRCHECK(evt.setCallback(FmodEventAudioSource.EventStoppedCallbackStatic, (IntPtr)m_selfHandle));			
		}
	}
	
	public bool isRuntimeEventLoaded() {
		return (m_runtimeEvent != null);
	}
	
	public FMOD.Event getRuntimeEvent() {
		return (m_runtimeEvent);
	}
	
	public void UpdateExistingEvent(FmodEvent newEvent) {
#if UNITY_EDITOR
		List<FmodRuntimeEventParameter> oldParams = getParameters();
		List<FmodRuntimeEventParameter> toRemove = new List<FmodRuntimeEventParameter>();
		
		//checking for existing params
		foreach (FmodRuntimeEventParameter oldParam in oldParams) {
			FmodEventParameter matchingParam = newEvent.getParameter(oldParam.getName());
			
			if (matchingParam == null) {
				toRemove.Add(oldParam);
			} else {
				oldParam.UpdateExistingParam(matchingParam);
			}
		}
		//removing previously existing params that have been deleted
		foreach (FmodRuntimeEventParameter toDelete in toRemove) {
			m_parameters.Remove(toDelete);
			DestroyImmediate(toDelete, true);
		}
		PrefabType prefabType = PrefabUtility.GetPrefabType(gameObject);

		// checking for newly created params
		if (prefabType != PrefabType.PrefabInstance &&
			prefabType != PrefabType.ModelPrefabInstance) {
			foreach (FmodEventParameter newParam in newEvent.getParameters()) {
				if (ParameterExists(newParam.getName()) == false) {
						FmodRuntimeEventParameter runtimeParam = gameObject.AddComponent<FmodRuntimeEventParameter>();
						runtimeParam.Initialize(newParam);
						m_parameters.Add(runtimeParam);
				}
			}
		}
		m_source = newEvent;
#endif
	}
	

	static FMOD.RESULT EventStoppedCallbackStatic (IntPtr eventraw, FMOD.EVENT_CALLBACKTYPE type, IntPtr param1, IntPtr param2, IntPtr userdata) {
		GCHandle handle = (GCHandle)userdata;
		FmodEventAudioSource src = (handle.Target as FmodEventAudioSource);
		return (src.EventStoppedCallback(type, param1, param2));
	}
	
	FMOD.RESULT EventStoppedCallback(FMOD.EVENT_CALLBACKTYPE type, IntPtr param1, IntPtr param2) {
		FMOD.RESULT result = FMOD.RESULT.OK;
		
		if (EventCallback != null) {
			EventCallback(this, type, param1, param2);
		}
		//		Debug.Log ("event: " + m_source.getName() + "; name: " + name + "; type: " + type);
		if (type == FMOD.EVENT_CALLBACKTYPE.EVENTSTARTED) {
			m_allSources.Add(this); // adding the event to the list only here means paused events will appear as active... we'll see if anyone complains. it such is the case; we should also add/remove in OnEnable and OnDisable
			m_status = Status.Playing;
			
			// the code below should fire event for all listeners
			if (EventStarted != null) {
				EventStarted(this);
			}
		} else if (type == FMOD.EVENT_CALLBACKTYPE.EVENTFINISHED) {
			m_status = Status.Stopped;
			m_allSources.Remove(this);
			if (EventFinished != null) {
				EventFinished(this);
			}
		} else if (type == FMOD.EVENT_CALLBACKTYPE.STOLEN) {
			FMOD.RESULT tmp = FMOD.RESULT.OK;
			
			if (EventStolen != null) {
				foreach (Delegate del in EventStolen.GetInvocationList()) {
					tmp = (FMOD.RESULT)del.DynamicInvoke(this);
					if (tmp != FMOD.RESULT.OK) {
						result = tmp;
					}
				}				
			}
			if (result != FMOD.RESULT.ERR_EVENT_FAILED) {
				cleanInvalidHandle();
			}
		} else if (type == FMOD.EVENT_CALLBACKTYPE.SOUNDDEF_START) {
			string name = Marshal.PtrToStringAnsi(param1);
			int waveIndexInSoundDef = param2.ToInt32();
			
			if (SoundDefStart != null) {
				SoundDefStart(this, name, waveIndexInSoundDef);				
			}
		} else if (type == FMOD.EVENT_CALLBACKTYPE.SOUNDDEF_END) {
			string name = Marshal.PtrToStringAnsi(param1);
			int waveIndexInSoundDef = param2.ToInt32();

			if (SoundDefEnd != null) {
				SoundDefEnd(this, name, waveIndexInSoundDef);				
			}
		}
		return (result);
	}
	
	public void UpdateEvent(Vector3 curPos, float timeSinceLevelLoad) {
		if (m_runtimeEvent != null) {
			if (m_eventSystemHandle == null ||
				m_eventSystemHandle.getEventSystem() == null ||
				m_eventSystemHandle.getEventSystem().wasCleaned()) {
				return ;
			}
			FMOD.RESULT result = FMOD.RESULT.OK;
			
			foreach (FmodRuntimeEventParameter p in getParameters()) {
				p.UpdateParam(timeSinceLevelLoad - m_previousUpdateTime);
			}
			if (m_source.getSourceType() == FmodEvent.SourceType.SOURCE_3D) {
				FMOD.VECTOR pos = new FMOD.VECTOR();
				FMOD.VECTOR vel = new FMOD.VECTOR();
				
				pos.x = curPos.x;
				pos.y = curPos.y;
				pos.z = curPos.z;
				if (timeSinceLevelLoad - m_previousUpdateTime > 0) {
					vel.x = (curPos.x - m_previousPos.x) / (timeSinceLevelLoad - m_previousUpdateTime);
					vel.y = (curPos.y - m_previousPos.y) / (timeSinceLevelLoad - m_previousUpdateTime);
					vel.z = (curPos.z - m_previousPos.z) / (timeSinceLevelLoad - m_previousUpdateTime);
				}
				m_previousPos = curPos;
				result = m_runtimeEvent.set3DAttributes(ref pos, ref vel);
				ERRCHECK(result);
			}
			m_previousUpdateTime = timeSinceLevelLoad;
		}
	}
				
	public void OnDestroy() {
		Clean ();
	}

	protected void CleanParameters() {
		if (m_parameters != null) {
			foreach (FmodRuntimeEventParameter item in m_parameters) {
				if (item != null) {
					item.Clean();					
				}
			}
		}
	}
	
	protected void CleanAndRemoveParameters() {
		CleanParameters();
		foreach (FmodRuntimeEventParameter p in gameObject.GetComponents<FmodRuntimeEventParameter>()) {
			if (Application.isEditor && Application.isPlaying) {
				Destroy (p);
			} else {
				DestroyImmediate(p, true);				
			}
		}
		m_parameters.Clear();
	}
	
	protected void CleanRuntimeEvent() {
		CleanParameters();
		// replace the member handle by a tmp handle created here... or not. it could lead to another one being created.
		if (m_runtimeEvent != null && m_eventSystemHandle != null) {
			if (m_eventSystemHandle.getEventSystem().wasCleaned() == false) {
				FMOD.RESULT result = FMOD.RESULT.OK;
				
				result = m_runtimeEvent.stop(false);
				SetEvent(null);
				ERRCHECK(result);
				if (result != FMOD.RESULT.ERR_INVALID_HANDLE) {
					//m_runtimeEvent.release(true, false); //we should check EVENTPROPERTY_EVENTTYPE to know if it is a simple event, and only call it if the event is simple					
				}
			}
			m_runtimeEvent = null;
		}
	}
	
	private void freeEventData() {
		FmodEvent evt = getSource();
		
		if (evt != null && m_runtimeEvent != null) {
			FmodEventGroup evtGroup = evt.getEventGroup();
			
			if (m_eventSystemHandle != null &&
				m_eventSystemHandle.getEventSystem() != null &&
				m_eventSystemHandle.getEventSystem().wasCleaned() == false) {
				if (m_eventSystemHandle.getEventSystem().getNumberRunningInstancesInGroup(evtGroup) == 0) {
					evtGroup.freeWholeGroup();
				}
			}
		}
	}
	
	public void Clean() {
		CleanRuntimeEvent();
		if (m_eventSystemHandle != null) {
			m_eventSystemHandle.Dispose();
			m_eventSystemHandle = null;			
		}
		if (m_selfHandle.IsAllocated) {
			m_selfHandle.Free();
		}
	}
	
	private void cleanBeforeChangingPlaymodeState() {
#if UNITY_EDITOR
		if (EditorApplication.isPlaying && !EditorApplication.isPlayingOrWillChangePlaymode) {
			EditorApplication.playmodeStateChanged -= cleanBeforeChangingPlaymodeState;
			Clean ();
		}
#endif
	}
	
	void cleanInvalidHandle() {
		foreach (FmodRuntimeEventParameter p in m_parameters) {
			p.Clean();
		}
		// in the case of a invalid handle, we don't put the handle back in the pool, since it's invalid. The one that stole it should alreayd be in it anyway.
		// however, we should remove the audiosource from the list of active sources
		m_runtimeEvent = null;
		if (m_eventSystemHandle != null &&
			m_eventSystemHandle.getEventSystem() != null &&
			m_eventSystemHandle.getEventSystem().wasCleaned()) {
			m_eventSystemHandle.getEventSystem().releaseRunningInstance(this);
		}
		m_status = Status.Stopped;		
	}
	
	public void CheckForRemainOfDeletedParameters() {
#if UNITY_EDITOR
		if (PrefabUtility.GetPrefabType(gameObject) == PrefabType.Prefab) {
			string path = AssetDatabase.GetAssetPath(gameObject);
			UnityEngine.Object[] obj = AssetDatabase.LoadAllAssetsAtPath(path);
			
			foreach (UnityEngine.Object item in obj) {
				FmodRuntimeEventParameter p = item as FmodRuntimeEventParameter;
				if (p != null) {
					if (p.IsRemainOfDeletedEvent() && ParameterExists(p.getName()) == false) {
						Debug.Log("FmodEventAudioSource: " + name + ": Removing deleted parameter '" + p.getName() + "'");
						GameObject.DestroyImmediate(p, true);
					}
				}
			}
			AssetDatabase.ImportAsset(path);
			AssetDatabase.SaveAssets();
		}
#endif
	}
	
	//FMOD Error checking from return codes
	void ERRCHECK(FMOD.RESULT result)
    {
		if (result == FMOD.RESULT.ERR_INVALID_HANDLE) {
			Debug.LogError("FMOD_Unity: Event error: " + result +
				": have you set the MaxCallback on the event '" + m_source.getName() +
				"' high enough ? (original FMOD message : " +
				FMOD.Error.String(result) + ")");
			cleanInvalidHandle();
		} else if (result != FMOD.RESULT.OK) {
            Debug.LogError("FMOD_Unity: Event error: " + result + " - " + FMOD.Error.String(result));
        }
    }
}

