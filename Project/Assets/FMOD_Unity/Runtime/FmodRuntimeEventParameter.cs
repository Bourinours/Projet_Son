/*
 * +-------------------------------------------------------------------------------------+
 *  Project: FMOD_Unity
 *  Description: Keeps track of the individual values of each FmodEventParameter for each
 * 				 FmodEventAudioSource. This component is hidden, and parameters are visible
 * 				 on the FmodAudioSource inspector.
 *  Company: ISART Digital (www.isartdigital.com)
 *  Author: Galaad BROD (galaad.brod@gmail.com)
 * +-------------------------------------------------------------------------------------+
 */

using UnityEngine;
using System;
using System.Collections;

[System.Serializable]
public class FmodRuntimeEventParameter : MonoBehaviour
{
	[SerializeField]
	public FmodEventParameter m_parameter;
	[SerializeField]
	protected string m_name;
	[SerializeField]
	protected float m_value;
	[SerializeField]
	protected float m_underlyingValue;
	
	protected FMOD.EventParameter m_runtimeParam = null;

	public float MinRange { get { return (m_parameter.MinRange); } }
	public float MaxRange { get { return (m_parameter.MaxRange); } }
	public float Velocity { get { return (m_parameter.Velocity); } }
	public float SeekSpeed { get { return (m_parameter.SeekSpeed); } }
	
	public void OnLevelWasLoaded() {
		hideFlags = HideFlags.HideInInspector | HideFlags.HideInHierarchy;		
	}

	public void OnEnable() {
		hideFlags = HideFlags.HideInInspector | HideFlags.HideInHierarchy;	
	}
	
	public void Initialize(FmodEventParameter srcParam) {
		if (srcParam != null) {
			hideFlags = HideFlags.HideInInspector | HideFlags.HideInHierarchy;	
			m_parameter = srcParam;
			m_value = m_parameter.getValue();
			m_name = m_parameter.getName();
		}
	}

	public void UpdateExistingParam(FmodEventParameter newParam) {
		if (newParam != null) {
			hideFlags = HideFlags.HideInInspector | HideFlags.HideInHierarchy;	
			m_parameter = newParam;
			m_name = m_parameter.getName();
		}
	}
	
	public void SetEvent(FMOD.Event evt) {
		FMOD.RESULT result = FMOD.RESULT.OK;
		FMOD.EventParameter param = null;
		
		hideFlags = HideFlags.HideInInspector | HideFlags.HideInHierarchy;	
		if (m_parameter != null) {
			result = evt.getParameter(m_parameter.getName(), ref param);
			ERRCHECK(result);
			if (result == FMOD.RESULT.OK) {
				m_runtimeParam = param;
				result = m_runtimeParam.setValue(m_value);
				ERRCHECK(result);
			}			
		} else {
			Debug.Log ("Error: This component should not exist now if its parameter is null");
		}
	}
	
	public float getUnderlyingValue() {
		if (m_runtimeParam == null) {
			return (-42);
		}
		float ret = -42;
		FMOD.RESULT result = FMOD.RESULT.OK;
		result = m_runtimeParam.getValue(ref ret);
		ERRCHECK(result);
		m_underlyingValue = ret;
		return (ret);
	}
	
	public void SetValue(float val) {
		FMOD.RESULT result = FMOD.RESULT.OK;

		m_value = Mathf.Clamp(val, MinRange, MaxRange);
		if (m_runtimeParam != null) {
			result = m_runtimeParam.setValue(m_value);
			ERRCHECK(result);
		}
	}
	
	public void KeyOff() {
		FMOD.RESULT result = FMOD.RESULT.OK;

		if (m_runtimeParam != null) {
			result = m_runtimeParam.keyOff();
			ERRCHECK(result);
		}
	}
	
	public float getValue ()
	{
		return (m_value);
	}	

	public string getName ()
	{
		return (m_name);
	}	
	
	public void UpdateValue() {
		SetValue(m_value);
	}
	
	public void UpdateParam(float deltaTime) {
		if (m_parameter != null && m_parameter.Velocity != 0) {
//			SetValue(m_value + deltaTime * m_velocity);
		} else if (m_parameter != null && m_parameter.SeekSpeed != 0) {
			float currentValue = getUnderlyingValue();
			float newValue = currentValue;
			
			if (m_value != currentValue) {
				newValue = currentValue + deltaTime * (m_value > currentValue ? 1 : -1) / m_parameter.SeekSpeed;
				if ((currentValue < m_value && newValue > m_value) ||
					(currentValue < m_value && newValue > m_value)) {
					newValue = m_value;
				}
			}
		}
	}
	
	public void Clean() {
		if (m_runtimeParam != null) {
			m_runtimeParam = null;
		}
	}
	
	public bool IsRemainOfDeletedEvent() {
		return (m_parameter == null);
	}

	//FMOD Error checking from return codes
	void ERRCHECK(FMOD.RESULT result)
    {
        if (result != FMOD.RESULT.OK)
        {
            Debug.LogWarning("FMOD_Unity: FmodRuntimeEventParameter '" + m_name + "': Error: " + FMOD.Error.String(result));
        }
    }
}

