/*
 * +-------------------------------------------------------------------------------------+
 *  Project: FMOD_Unity
 *  Description: The logic behind the FmodEventAudioSource Custom Inspector.
 * 				 Handles the inspector and the range gizmos in the scene.
 *  Company: ISART Digital (www.isartdigital.com)
 *  Author: Galaad BROD (galaad.brod@gmail.com)
 * +-------------------------------------------------------------------------------------+
 */

#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

[CustomEditor(typeof(FmodEventAudioSource))]
public class FmodEventAudioSourceEditor : Editor {
	private bool m_showParams = true;
	private List<SerializedObject> m_parameters = new List<SerializedObject>();
	private SerializedObject m_event;
	private SerializedObject m_source;
	private bool m_show3DSettings = true;
	private bool m_showUnderlyingParameters = false;
		
	public override void OnInspectorGUI() {
		if (target == null) {
			return ;
		}
		FmodEventAudioSource source = target as FmodEventAudioSource;
		string typeString;
		List<FmodRuntimeEventParameter> parameters;
		bool shouldRepaint = false;
		
		source.CheckForOldFormat();
		if (source.getSource() == null) {
			parameters = new List<FmodRuntimeEventParameter>();
			typeString = "No event is loaded";
			m_show3DSettings = false;
		} else {
			typeString = "This is a " + (source.type == FmodEvent.SourceType.SOURCE_2D ? "2D" : "3D") + " event.";
			m_show3DSettings = (source.type == FmodEvent.SourceType.SOURCE_2D ? false : true);
			parameters = source.getParameters();
			if (m_parameters.Count == 0 && parameters.Count > 0) {
				foreach (FmodRuntimeEventParameter p in parameters) {
					m_parameters.Add(new SerializedObject(p));
				}
			}
			if (m_event == null && source.eventClip != null) {
				m_event = new SerializedObject(source.eventClip.getSource());
			}
			if (m_event != null) {
				m_event.Update();
			}
			if (m_source != null && !(m_source.targetObject is FmodEventAudioSource)) {
				m_source = null;
			}
			if (m_source == null && source.getSource() != null) {
				m_source = new SerializedObject(source);
			}
			m_source.Update();
			foreach (SerializedObject p in m_parameters) {
				p.Update();
				((FmodRuntimeEventParameter)p.targetObject).getUnderlyingValue();
			}
		}
		
		EditorGUILayout.ObjectField("Source event", source.getSource(), typeof(FmodEvent), false);
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.PrefixLabel(" ");
		EditorGUILayout.LabelField(typeString, GUI.skin.textField, GUILayout.ExpandWidth(false));
		EditorGUILayout.EndHorizontal();
		
		if (source.getSource() != null) {
			FmodEventAudioSource.Status status = source.CurrentStatus;
			bool paused = source.Paused;
			
			EditorGUILayout.LabelField("Status", source.getStatus(), GUI.skin.textField);
			
			GUI.enabled = (Application.isPlaying);
			if (paused) {
				if (GUILayout.Button("Unpause")) {
					source.Unpause();
				}
			} else {
				if (GUILayout.Button("Pause")) {
					source.Pause();
				}					
			}
			if (status == FmodEventAudioSource.Status.Stopped) {
				if (GUILayout.Button("Play")) {
					source.Play();
				}
			} else if (status == FmodEventAudioSource.Status.Playing) {
				if (GUILayout.Button("Stop")) {
					source.Stop();
				}
			}
			GUI.enabled = true;
		}
		
		m_showParams = EditorGUILayout.Foldout(m_showParams, "Parameters");
		if (m_showParams) {
			EditorGUI.indentLevel += 2;
			if (m_parameters.Count > 0) {
				foreach (SerializedObject o in m_parameters) {
					FmodRuntimeEventParameter p = o.targetObject as FmodRuntimeEventParameter;
					SerializedProperty prop = o.FindProperty("m_value");
					
					if (p.getName() == "(distance)" || p.getName() == "(listener angle)" || p.getName() == "(event angle)") {
						EditorGUILayout.BeginHorizontal();
						EditorGUILayout.PrefixLabel(p.getName());
						EditorGUILayout.HelpBox("Parameters named '" + p.getName() + "' are reserved by FMOD for 3D sounds", MessageType.None);
						EditorGUILayout.EndHorizontal();
					} else {
						EditorGUILayout.Slider(prop, p.MinRange, p.MaxRange, p.getName());
					}
					if (p == null || p.m_parameter == null) {
						Debug.LogWarning ("Error happening in gameObject " + source.gameObject);						
					}
					shouldRepaint = (shouldRepaint || p.Velocity != 0);
				}
			} else {
				if (source.eventClip == null) {
					EditorGUILayout.LabelField("Load an event to see its parameters");					
				} else {
					EditorGUILayout.LabelField("This event has no parameters.");
				}
			}
			EditorGUI.indentLevel -= 2;
		}
		
		if (m_source != null && source.getSource()) {
			if (m_source.FindProperty("m_volume") == null) {
				Debug.Log ("NULL volume !");
			}
			EditorGUILayout.Slider(m_source.FindProperty("m_volume"), 0, 100, "Volume");
			
			m_show3DSettings = EditorGUILayout.Foldout(m_show3DSettings, "3D Sound Settings");
			if (m_show3DSettings) {
				EditorGUI.indentLevel += 2;
				FmodEvent.RolloffType rolloffType = ((FmodEvent)source.getSource()).Rolloff;
				SerializedProperty minRangeProp = m_source.FindProperty("m_minRange");
				SerializedProperty maxRangeProp = m_source.FindProperty("m_maxRange");
				
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.PrefixLabel("Rolloff");
				EditorGUILayout.LabelField(((FmodEvent)source.getSource()).getRolloffTypeString(), GUI.skin.textField, GUILayout.ExpandWidth(false));
				EditorGUILayout.EndHorizontal();
				if (rolloffType == FmodEvent.RolloffType.CUSTOM) {
					EditorGUILayout.HelpBox("This event has a custom rolloff : the Min Distance parameter is unused and the Max Distance parameter acts as a distance multiplier.", MessageType.Warning);
				}
				EditorGUILayout.PropertyField(minRangeProp, new GUIContent("Min Distance"));
				EditorGUILayout.PropertyField(maxRangeProp, new GUIContent("Max Distance"));
				EditorGUI.indentLevel -= 2;
			}
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.PrefixLabel("Play On Awake");
			source.playOnAwake = EditorGUILayout.Toggle(source.playOnAwake);
			EditorGUILayout.EndHorizontal();
			
			/*
			 * UNDERLYING VALUES
			 */
			EditorGUILayout.Separator();
			m_showUnderlyingParameters = EditorGUILayout.Foldout(m_showUnderlyingParameters, "Underlying parameters value");
			if (m_showUnderlyingParameters) {
				EditorGUI.indentLevel += 2;
				if (m_parameters.Count > 0) {
					GUI.enabled = false;
					foreach (SerializedObject o in m_parameters) {
						FmodRuntimeEventParameter p = o.targetObject as FmodRuntimeEventParameter;
						
						SerializedProperty underlyingValueProp = o.FindProperty("m_underlyingValue");
						EditorGUILayout.Slider(underlyingValueProp, p.MinRange, p.MaxRange, p.getName());
					}
					GUI.enabled = true;
				} else {
					if (source.eventClip == null) {
						EditorGUILayout.LabelField("Load an event to see its parameters");					
					} else {
						EditorGUILayout.LabelField("This event has no parameters.");
					}
				}
	
				EditorGUI.indentLevel -= 2;				
			}
			/*
			 * END UNDERLYING VALUES
			 */
			foreach (SerializedObject o in m_parameters) {
				if (o.ApplyModifiedProperties()) {
					FmodRuntimeEventParameter p = o.targetObject as FmodRuntimeEventParameter;
					p.UpdateValue();
				}
			}
			if (m_source.ApplyModifiedProperties()) {
				FmodEvent evt = source.getSource();
				if (evt.getSourceType() == FmodEvent.SourceType.SOURCE_3D) {
					source.setMinRange(source.getMinRange());
					source.setMaxRange(source.getMaxRange());				
				}
				source.SetVolume(source.GetVolume());
			}
			
			if (GUI.changed) {
				EditorUtility.SetDirty(target);
			}
			if (m_showUnderlyingParameters || shouldRepaint) {
				Repaint();
			}
		}
	}
	
	public void OnSceneGUI() {
		FmodEventAudioSource source = target as FmodEventAudioSource;
		
		source.CheckForOldFormat();
		if (source != null && source.getSource() != null && source.getSourceType() == FmodEvent.SourceType.SOURCE_3D) {
			float prevVal;
			float val;
			SerializedProperty minRangeProp;
			SerializedProperty maxRangeProp;
			
			if (m_source == null) {
				m_source = new SerializedObject(source.getSource());
			}
			minRangeProp = m_source.FindProperty("m_minRange");
			maxRangeProp = m_source.FindProperty("m_maxRange");
			
			prevVal = source.getMinRange();
			val = Handles.RadiusHandle(source.transform.rotation, source.transform.position, prevVal);
			minRangeProp.floatValue = val;
			if (val != prevVal) {
				source.setMinRange(val);
			}
			prevVal = source.getMaxRange();
			val = Handles.RadiusHandle(source.transform.rotation, source.transform.position, prevVal);
			maxRangeProp.floatValue = val;
			if (val != prevVal) {
				source.setMaxRange(val);
			}
			if (m_source.ApplyModifiedProperties()) {
				source.setMinRange(source.getMinRange());
				source.setMaxRange(source.getMaxRange());
			}
			if (GUI.changed) {
				EditorUtility.SetDirty(target);
			}
		}
	}
}
#endif
