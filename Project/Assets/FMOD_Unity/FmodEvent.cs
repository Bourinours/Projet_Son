/*
 * +-------------------------------------------------------------------------------------+
 *  Project: FMOD_Unity
 *  Description: A wrapper around the FMOD Events. Is used as storage for all
 * 				  information extracted from the .fev file. This information will
 * 				  be used for finding the FMOD.Event at runtime, finding the good
 * 				  parameters and displaying the right gizmos in the scene.
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

public class FmodEvent : ScriptableObject
{
	public enum SourceType 
	{
		SOURCE_2D = 0x00000008, // Source : C++ file fmod/fmod.h
		SOURCE_3D = 0x00000010  // Source : C++ file fmod/fmod.h
	}
	public enum RolloffType
	{
		CUSTOM = 0,
		INVERSE,
		LINEAR,
		LINEARSQUARE,
		LOGARITHMIC,
	}
	protected static string[] RolloffTypeStrings = {
		"Custom",
		"Inverse",
		"Linear",
		"Linear Squared",
		"Logarithmic"
	};
	public const string EMPTY_GUIDSTRING = "{00000000-0000-0000-0000-000000000000}";

	[SerializeField]
	protected string m_name;
	[SerializeField]
	protected List<FmodEventParameter> m_parameters = new List<FmodEventParameter>();
	[SerializeField]
	protected FmodEventAsset m_asset;
	[SerializeField]
	protected FmodEventGroup m_group;
	[SerializeField]
	protected int m_indexInGroup;
	[SerializeField]
	protected SourceType m_sourceType;
	[SerializeField]
	public string m_guidString;
	[SerializeField]
	protected RolloffType m_rolloffType;
	[SerializeField]
	protected bool m_wasLoaded = false;

	[SerializeField]
	[Range(0, 100000)]
	public float m_minRange; // only makes sense for 3D sounds - those with m_sourceType == 3D
	[SerializeField]
	[Range(0, 100000)]
	public float m_maxRange; // only makes sense for 3D sounds - those with m_sourceType == 3D

	public RolloffType Rolloff{ get { return (m_rolloffType); } }
	
	public void Initialize(FmodEventGroup eventGroup, int indexInGroup, FmodEventAsset asset) {
		#if UNITY_EDITOR
			hideFlags = HideFlags.HideInHierarchy;
			m_group = eventGroup;
			m_indexInGroup = indexInGroup;
			m_asset = asset;
			m_name = "(This event could not be loaded. Are you missing a .fsb ?)";
		#endif
	}
	
	public void Initialize(FMOD.Event e, FmodEventGroup eventGroup, int indexInGroup, FmodEventAsset asset) {
#if UNITY_EDITOR
		FMOD.EVENT_INFO info = new FMOD.EVENT_INFO();
		FMOD.GUID guid = new FMOD.GUID();
		FMOD.EventParameter param = null;
		FMOD.RESULT result = FMOD.RESULT.OK;
		FmodEventParameter toAdd = null;
		IntPtr name = new IntPtr(0);
		int numParameters = 0;
		int index = 0;
		
		Initialize(eventGroup, indexInGroup, asset);
		int size = System.Runtime.InteropServices.Marshal.SizeOf(typeof(FMOD.GUID));
		info.guid = System.Runtime.InteropServices.Marshal.AllocHGlobal(size);
		result = e.getInfo(ref index, ref name, ref info);
		ERRCHECK(result);
		m_name = System.Runtime.InteropServices.Marshal.PtrToStringAnsi(name);
		this.name = m_name;
		guid = (FMOD.GUID)System.Runtime.InteropServices.Marshal.PtrToStructure(info.guid, typeof(FMOD.GUID));
		m_guidString = "{" + String.Format("{0:x8}-{1:x4}-{2:x4}-{3:x2}{4:x2}-{5:x2}{6:x2}{7:x2}{8:x2}{9:x2}{10:x2}", 
			guid.Data1, guid.Data2, guid.Data3,
			guid.Data4[0], guid.Data4[1],
			guid.Data4[2], guid.Data4[3], guid.Data4[4], guid.Data4[5], guid.Data4[6], guid.Data4[7]
		) + "}"; 
		
		int mode = 0;
		IntPtr modePtr = System.Runtime.InteropServices.Marshal.AllocHGlobal(sizeof(int));
		e.getPropertyByIndex((int)FMOD.EVENTPROPERTY.MODE, modePtr, false);
		mode = System.Runtime.InteropServices.Marshal.ReadInt32(modePtr);
		System.Runtime.InteropServices.Marshal.FreeHGlobal(modePtr);
		m_sourceType = (SourceType)mode;
		
		if (m_sourceType == SourceType.SOURCE_3D) {
			IntPtr range;
			float[] tmp = new float[1];
			int[] tmpInt = new int[1];
			
			range = System.Runtime.InteropServices.Marshal.AllocHGlobal(sizeof(int));
			result = e.getPropertyByIndex((int)FMOD.EVENTPROPERTY._3D_ROLLOFF, range, false);
			ERRCHECK(result);
			System.Runtime.InteropServices.Marshal.Copy(range, tmpInt, 0, 1);
			if (tmpInt[0] == (int)FMOD.MODE._3D_CUSTOMROLLOFF) {
				m_rolloffType = RolloffType.CUSTOM;
			} else if (tmpInt[0] == (int)FMOD.MODE._3D_INVERSEROLLOFF) {			
				m_rolloffType = RolloffType.INVERSE;
			} else if (tmpInt[0] == (int)FMOD.MODE._3D_LINEARROLLOFF) {			
				m_rolloffType = RolloffType.LINEAR;
			} else if (tmpInt[0] == (int)FMOD.MODE._3D_LINEARSQUAREROLLOFF) {			
				m_rolloffType = RolloffType.LINEARSQUARE;
			} else if (tmpInt[0] == (int)FMOD.MODE._3D_LOGROLLOFF) {			
				m_rolloffType = RolloffType.LOGARITHMIC;
			}
			System.Runtime.InteropServices.Marshal.FreeHGlobal(range);

			range = System.Runtime.InteropServices.Marshal.AllocHGlobal(sizeof(float));
			result = e.getPropertyByIndex((int)FMOD.EVENTPROPERTY._3D_MINDISTANCE, range, false);
			ERRCHECK(result);
			System.Runtime.InteropServices.Marshal.Copy(range, tmp, 0, 1);
			m_minRange = tmp[0];
			System.Runtime.InteropServices.Marshal.FreeHGlobal(range);
			range = System.Runtime.InteropServices.Marshal.AllocHGlobal(sizeof(float));
			result = e.getPropertyByIndex((int)FMOD.EVENTPROPERTY._3D_MAXDISTANCE, range, false);
			ERRCHECK(result);
			System.Runtime.InteropServices.Marshal.Copy(range, tmp, 0, 1);
			m_maxRange = tmp[0];
			System.Runtime.InteropServices.Marshal.FreeHGlobal(range);
		}

		
		e.getNumParameters(ref numParameters);
		for (int k = 0; k < numParameters; k++) {
			e.getParameterByIndex(k, ref param);
			toAdd = FmodEventParameter.CreateInstance("FmodEventParameter") as FmodEventParameter;
			toAdd.Initialize(param, this);
			m_parameters.Add(toAdd);
		}
		m_wasLoaded = true;
#endif
	}
	
	public void Initialize(FmodEvent src) {
#if UNITY_EDITOR
		List<FmodEventParameter> newParams = new List<FmodEventParameter>();

		EditorUtility.CopySerialized(src, this);
		hideFlags = HideFlags.HideInHierarchy;
		foreach (FmodEventParameter p in m_parameters) {
			FmodEventParameter newP = ScriptableObject.CreateInstance("FmodEventParameter") as FmodEventParameter;
			newP.Initialize(p, this);
			newParams.Add(newP);
		}
		m_parameters = newParams;
#endif
	}
	
	public void InitializeFromUpdatedEvent(FmodEvent src, FmodEvent oldEvent) {
#if UNITY_EDITOR
		List<FmodEventParameter> newParams = new List<FmodEventParameter>();
		List<FmodEventParameter> oldParams = oldEvent.getParameters();

		
		EditorUtility.CopySerialized(src, this);
		hideFlags = HideFlags.HideInHierarchy;
		foreach (FmodEventParameter p in m_parameters) {
			FmodEventParameter newP = ScriptableObject.CreateInstance("FmodEventParameter") as FmodEventParameter;
			newP.Initialize(p, this);
			foreach (FmodEventParameter oldP in oldParams) {
				if (p.name == oldP.name) {
					newP.SetValue(oldP.getValue());
					oldParams.Remove(oldP);
					break;
				}
			}
			newParams.Add(newP);
		}
		m_parameters = newParams;
		m_minRange = oldEvent.m_minRange;
		m_maxRange = oldEvent.m_maxRange;
#endif
	}
	
	public void CreateAsset(string assetFile, FmodEventAsset asset) {
#if UNITY_EDITOR
		AssetDatabase.AddObjectToAsset(this, asset);
//		AssetDatabase.ImportAsset(assetFile); // force a save
		foreach (FmodEventParameter param in m_parameters) {
			param.CreateAsset(assetFile, this);
		}
#endif
	}
	
	public string getName() {
		return m_name;
	}

	public string getFullName() {
		string ret = "";
		
		if (m_group != null) {
			ret = m_group.getFullName() + "/";
		}
		return (ret + getName());
	}
	
	public SourceType getSourceType() {
		return (m_sourceType);	
	}
	
	public string getRolloffTypeString() {
		return (RolloffTypeStrings[(int)m_rolloffType]);
	}
	
	public List<FmodEventParameter> getParameters() {
		return (m_parameters);
	}
	
	public FmodEventParameter getParameter(string parameterName) {
		foreach (FmodEventParameter p in m_parameters) {
			if (p.getName() == parameterName) {
				return (p);
			}
		}
		return (null);		
	}
	
	public bool parameterExists(string parameterName) {
		return (getParameter(parameterName) != null);
	}
	
	public FmodEventAsset getAsset() {
		return (m_asset);
	}
	
	public string getGUIDString() {
		return (m_guidString);
	}
	
	public FmodEventGroup getEventGroup() {
		return (m_group);
	}

	public bool WasLoaded() {
		return (m_wasLoaded);
	}
	
	//FMOD Error checking from return codes
	static void ERRCHECK(FMOD.RESULT result)
    {
        if (result != FMOD.RESULT.OK)
        {
            Debug.LogError("FMOD_Unity: Event error: " + result + " - " + FMOD.Error.String(result));
         }
    }
}

