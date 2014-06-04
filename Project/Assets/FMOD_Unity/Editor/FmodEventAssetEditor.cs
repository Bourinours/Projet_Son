/*
 * +-------------------------------------------------------------------------------------+
 *  Project: FMOD_Unity
 *  Description: The logic behind the FmodEventAsset Custom Inspector.
 * 				 Handles the inspector, drag & drops of events into the scene
 * 				 and instantiation of FmodEventAudioSource.
 *  Company: ISART Digital (www.isartdigital.com)
 *  Author: Galaad BROD (galaad.brod@gmail.com)
 * +-------------------------------------------------------------------------------------+
 */

#if UNITY_EDITOR
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

[ExecuteInEditMode]
[CustomEditor(typeof(FmodEventAsset))]
public class FmodEventAssetEditor : Editor {
	private const string ONGOING_DRAG_KEY = "FModEventAssetEditor.ongoingDrag";
	private const string SOURCE_ASSET_KEY = "FModEventAssetEditor.sourceAsset";
	
	private bool m_showReverbs = true;
	private bool m_showEvents = true;
	private int m_nbEvents = 0;
	private Rect[] m_eventsRectangle;
	private FmodEvent[] m_events;
	private Rect[] m_reverbsRectangle;
	private bool m_ongoingDrag = false;
	private GUIStyle m_style = null;
	private int m_curNbEvents = 0; // to know the number of events currently displayed, for indexing events and their drawing rectangle
	
	void OnEnable() {
		serializedObject.Update();
		FmodEventAsset asset = this.serializedObject.targetObject as FmodEventAsset;
		m_nbEvents = asset.getEvents().Count;
		m_eventsRectangle = new Rect[m_nbEvents];
		m_reverbsRectangle = new Rect[asset.getReverbs().Count];
		m_events = new FmodEvent[m_nbEvents];
		SceneView.onSceneGUIDelegate += this.OnSceneGUI;
	}
	
	void OnDisable () {
		SceneView.onSceneGUIDelegate -= this.OnSceneGUI;
		_clearDragAndDrop();
	}
	
	private FmodEvent _getHoveredEvent() {
		if (m_showEvents && Event.current.isMouse) {
			Vector2 mousePos = Event.current.mousePosition;
			int i = 0;
			
			foreach (Rect r in m_eventsRectangle) {
				if (r.Contains(mousePos)) {
					return (m_events[i]);
				}
				i++;
			}
		}
		return (null);
	}
	
	private FmodReverb _getHoveredReverb() {
		if (m_showReverbs && Event.current.isMouse) {
			FmodEventAsset asset = this.serializedObject.targetObject as FmodEventAsset;
			Vector2 mousePos = Event.current.mousePosition;
			int i = 0;
			
			foreach (Rect r in m_reverbsRectangle) {
				if (r.Contains(mousePos)) {
					return (asset.getReverbs()[i]);
				}
				i++;
			}
		}
		return (null);
	}
	
	private void _SetupDragAndDrop(FmodEventAsset asset) {
		Event e = Event.current;
		
		if (e.type == EventType.MouseUp || e.type == EventType.MouseDown) {
			_clearDragAndDrop();
		}
		if (e.type == EventType.MouseDrag) {
			FmodEvent fmodEvent = _getHoveredEvent();
			FmodReverb reverb = _getHoveredReverb();
			
			if (fmodEvent != null) {
				DragAndDrop.PrepareStartDrag();
				DragAndDrop.paths = new string[0];
				DragAndDrop.objectReferences = new Object[1] { fmodEvent };
				DragAndDrop.SetGenericData(ONGOING_DRAG_KEY, true);
				DragAndDrop.SetGenericData(SOURCE_ASSET_KEY, asset);
				DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
				DragAndDrop.StartDrag("Creating sound from event '" + fmodEvent.getName() + "'");
				m_ongoingDrag = true;
				e.Use();
			} else if (reverb != null) {
				DragAndDrop.PrepareStartDrag();
				DragAndDrop.paths = new string[0];
				DragAndDrop.objectReferences = new Object[1] { reverb };
				DragAndDrop.SetGenericData(ONGOING_DRAG_KEY, true);
				DragAndDrop.SetGenericData(SOURCE_ASSET_KEY, asset);
				DragAndDrop.visualMode = DragAndDropVisualMode.Move;
				DragAndDrop.StartDrag("Creating reverb zone from reverb '" + reverb.getName() + "'");
				m_ongoingDrag = true;
				e.Use();
			}
		}
	}
	
	private void _clearDragAndDrop() {
		if (m_ongoingDrag) {
			m_ongoingDrag = false;
			DragAndDrop.PrepareStartDrag();
			DragAndDrop.paths = new string[0];
			DragAndDrop.objectReferences = new Object[0];
		}
	}
	
	public void _eventHierarchyGUI(FmodEventGroup curGroup) {
		List<FmodEvent> events = curGroup.getEvents();
		
		if (m_style == null) {
			m_style = new GUIStyle(GUI.skin.label);
		}

		curGroup.showEventsInEditor = EditorGUILayout.Foldout(curGroup.showEventsInEditor, curGroup.getName());
		if (curGroup.showEventsInEditor) {
			EditorGUI.indentLevel += 1;
			foreach (FmodEvent e in events) {
				if (e.WasLoaded()) {
					EditorGUILayout.BeginHorizontal();
					EditorGUILayout.LabelField(e.getName(), m_style, GUILayout.ExpandWidth(true));
					if (Event.current.type == EventType.Repaint) {
						Rect lastRect = GUILayoutUtility.GetLastRect();
						m_eventsRectangle[m_curNbEvents].Set(lastRect.xMin, lastRect.yMin, lastRect.width, lastRect.height);
						m_events[m_curNbEvents] = e;
					}
					EditorGUILayout.LabelField((e.getSourceType() == FmodEvent.SourceType.SOURCE_2D ? "2D" : "3D"));
					EditorGUILayout.EndHorizontal();
					m_curNbEvents++;
				} else {
					EditorGUILayout.HelpBox("Could not load this event. Are you missing a .fsb ?", MessageType.Error);
				}
			}
			foreach (FmodEventGroup child in curGroup.getChildrenGroups()) {
				_eventHierarchyGUI(child);
			}
			EditorGUI.indentLevel -= 1;					
		}
	}
	
	public override void OnInspectorGUI() {
		serializedObject.Update();
		FmodEventAsset asset = this.serializedObject.targetObject as FmodEventAsset;
		List<string> soundBankList = asset.getSoundBankList();
		string currentPath = asset.getMediaPath();
		string projectRootPath = System.IO.Path.GetDirectoryName(Application.dataPath);
		string totalPath = "";
		bool soundBankExists = false;
		
		EditorGUILayout.LabelField("Name", asset.getName ());
		EditorGUILayout.LabelField("Sound banks referenced");
		EditorGUI.indentLevel += 1;
		if (soundBankList.Count == 0) {
			EditorGUILayout.LabelField("No Sound banks referenced in this file");
		} else {
			foreach (string soundBank in soundBankList) {
				System.Text.StringBuilder sb = new System.Text.StringBuilder();
				sb.Append(projectRootPath);
				sb.Append("/");
				sb.Append(currentPath);
				sb.Append("/");
				sb.Append(soundBank);
				sb.Append(@".fsb");
				sb.Replace('/', '\\');
				
				totalPath = sb.ToString();
//				totalPath = string.Concat(projectRootPath, "/", currentPath, "/", soundBank, @".fsb");
//				totalPath = totalPath.Replace("/", "\\");
				soundBankExists = System.IO.File.Exists(totalPath);
				EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(false));
				EditorGUILayout.PrefixLabel(soundBank);
				if (soundBankExists) {
					EditorGUILayout.HelpBox("(Found)", MessageType.None);					
				} else {
					EditorGUILayout.HelpBox("The sound bank could not be found at path '" + totalPath + "'. The events relying on it won't work.", MessageType.Error);
				}
				EditorGUILayout.EndHorizontal();
			}
			if (GUILayout.Button("Refresh")) {
				AssetDatabase.ImportAsset(currentPath, ImportAssetOptions.ForceUpdate);
					Repaint();
			}
		}
		EditorGUI.indentLevel -= 1;
		EditorGUILayout.HelpBox("Drag the event name onto the scene to create a source ; Alt + Drag for adding the source as component to target GameObject", MessageType.Info);
		m_showEvents = EditorGUILayout.Foldout(m_showEvents, "Events");
		m_curNbEvents = 0;
		if (m_showEvents) {
			EditorGUI.indentLevel += 1;
			foreach (FmodEventGroup eventGroup in asset.getEventGroups()) {
				_eventHierarchyGUI(eventGroup);
			}
			EditorGUI.indentLevel -= 1;
		}
		_SetupDragAndDrop(asset);
		EditorGUILayout.Separator();
		EditorGUILayout.HelpBox("Drag the reverb name onto the scene to create a reverb zone", MessageType.Info);
		m_showReverbs = EditorGUILayout.Foldout(m_showReverbs, "Reverb presets");
		if (m_showReverbs) {
			if (asset.getReverbs().Count > 0) {
				int curNbReverbs = 0;
				
				EditorGUI.indentLevel += 2;
				foreach (FmodReverb reverb in asset.getReverbs()) {
					EditorGUILayout.LabelField(reverb.getName());
					if (Event.current.type == EventType.Repaint) {
						Rect lastRect = GUILayoutUtility.GetLastRect();
						m_reverbsRectangle[curNbReverbs++].Set(lastRect.xMin, lastRect.yMin, lastRect.width, lastRect.height);
					}
				}
				EditorGUI.indentLevel -= 2;
			} else {
				EditorGUILayout.LabelField("No reverb preset in this file");
			}
		}
	}
	
	public void OnSceneGUI(SceneView sceneView) {
		if (m_ongoingDrag && Event.current.type != EventType.MouseMove) {
			if (Event.current.type == EventType.MouseUp || Event.current.type == EventType.MouseDown) {
				_clearDragAndDrop();
			}
			if (Event.current.type == EventType.DragUpdated) {
				DragAndDrop.AcceptDrag();
				DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
				Vector2 mousePos = Event.current.mousePosition;
				Ray ray = HandleUtility.GUIPointToWorldRay(mousePos);
				RaycastHit hitInfo = new RaycastHit();
				
				if (Physics.Raycast(ray, out hitInfo)) {
					EditorGUIUtility.PingObject(hitInfo.collider.gameObject);
				}
				// todo: preview of object creation ?
				Event.current.Use();
			} else if (Event.current.type == EventType.DragPerform) {
				Vector2 mousePos = Event.current.mousePosition;
				Vector3 worldPos = new Vector3();
				Ray ray = HandleUtility.GUIPointToWorldRay(mousePos);
				RaycastHit hitInfo = new RaycastHit();
				
				if (Physics.Raycast(ray, out hitInfo)) {
					worldPos = hitInfo.point;
				} else {
					worldPos = ray.origin + ray.direction.normalized * 10;
				}
				
				FmodEvent srcEvent = DragAndDrop.objectReferences[0] as FmodEvent;
				FmodReverb srcReverb = DragAndDrop.objectReferences[0] as FmodReverb;
				
				if (srcEvent != null) {
					CreateEventInstance(srcEvent, worldPos, hitInfo);
				} else if (srcReverb) {
					CreateReverbZone(srcReverb, worldPos, hitInfo);
				}
				
				_clearDragAndDrop();
				Event.current.Use();
			}
		}
	}

	protected void CreateEventInstance (FmodEvent srcEvent, Vector3 worldPos, RaycastHit hitInfo)
	{
		FmodEventAudioSource audioSource = null;
		if (Event.current.alt) { // if alt is on, we add the audio source as a component
			GameObject dest = hitInfo.collider.gameObject;
			
			audioSource = dest.GetComponent<FmodEventAudioSource>();
			if (audioSource == null || audioSource.eventClip != null) {
				audioSource = dest.AddComponent(typeof(FmodEventAudioSource)) as FmodEventAudioSource;
			}
		} else { // else we create a GameObject to act as source
			GameObject obj = GameObject.Instantiate(Resources.Load("FmodEventAudioSource"), worldPos, Quaternion.identity) as GameObject;
			audioSource = obj.GetComponent<FmodEventAudioSource>();
			if (audioSource == null) {
				Debug.LogError("Prefab for FmodEventAudioSource should have component FmodEventAudioSource !");
			} else {
				obj.name = "FmodEventSource (" + srcEvent.name + ")";
			}
		}
		audioSource.SetSourceEvent(srcEvent);
	}

	protected void CreateReverbZone (FmodReverb srcReverb, Vector3 worldPos, RaycastHit hitInfo)
	{
		GameObject obj = GameObject.Instantiate(Resources.Load("FmodReverbZone"), worldPos, Quaternion.identity) as GameObject;
		FmodReverbZone reverbZone = obj.GetComponent<FmodReverbZone>();
		if (reverbZone == null) {
			Debug.LogError("Prefab for FmodReverbZone should have component FmodReverbZone !");
		} else {
			reverbZone.SetReverb(srcReverb);
			obj.name = "FmodReverbZone (" + srcReverb.name + ")";
		}
	}
}
#endif
