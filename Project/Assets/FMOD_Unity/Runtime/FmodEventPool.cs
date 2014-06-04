using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FmodEventPool
{
	private FmodEvent m_event;
	private List<FMOD.Event> m_availableEvents = new List<FMOD.Event>();
	private List<FmodEventAudioSource> m_activeSources = new List<FmodEventAudioSource>();
	
	public FmodEventPool(FmodEvent source) {
		m_event = source;
	}
	
	public FMOD.RESULT getEvent(FmodEventAudioSource wantedSource) {
		if (wantedSource == null || wantedSource.getSource() == null) {
			Debug.LogError(getErrorPrefix() + "Invalid data was passed for FmodEventAudioSource");
			return (FMOD.RESULT.ERR_EVENT_FAILED);
		}
		if (wantedSource.getSource() != m_event) {
			Debug.LogError(getErrorPrefix() + "FmodEventAudioSource tried to load event '" + wantedSource.getSource().getName() + "' from the bad pool.");
			return (FMOD.RESULT.ERR_EVENT_FAILED);			
		}
		if (wantedSource.isRuntimeEventLoaded()) {
			return (FMOD.RESULT.OK);
		}
		if (m_availableEvents.Count > 0) {
			// below, we take back an event that was loaded and unused
			FMOD.Event oldestEvent = m_availableEvents[0];
			m_availableEvents.RemoveAt(0);
			
			wantedSource.SetEvent(oldestEvent);
			if (m_activeSources.Contains(wantedSource)) {
				Debug.LogWarning (getErrorPrefix() + "FmodEventAudioSource '" + wantedSource.name + "' loaded an event but was already active. Are you sure this should happen ?");
			} else {
				m_activeSources.Add(wantedSource);
			}
		} else {
			// here we have no event loaded, so we must load a new one.
			using (FmodEventSystemHandle handle = new FmodEventSystemHandle()) {
				FmodEventSystem system = handle.getEventSystem();

				if (system != null && system.wasCleaned() == false) {
					FMOD.RESULT result = FMOD.RESULT.OK;
					
					result = system.loadEventFromFile(wantedSource);
					if (result == FMOD.RESULT.OK) {
						m_activeSources.Add(wantedSource);						
					}
					return (result);
				}
			}
		}
		return (FMOD.RESULT.OK);
	}
	
	public void releaseRunningInstance(FmodEventAudioSource runningSource) {
		if (runningSource == null || runningSource.getSource() == null) {
			Debug.LogError(getErrorPrefix() + "Invalid data was passed for FmodEventAudioSource");
			return ;
		}
		if (runningSource.getSource() != m_event) {
			Debug.LogError(getErrorPrefix() + "FmodEventAudioSource tried to load event '" + runningSource.getSource().getName() + "' from the bad pool.");
			return ;
		}
		FMOD.Event instance = runningSource.getRuntimeEvent();
		
		if (m_activeSources.Contains(runningSource)) {
			if (instance != null) {
				m_availableEvents.Add(instance);				
			}
			m_activeSources.Remove(runningSource);
		}  else {
			Debug.LogError("this should not happen; Ever.");
		}
		
	}
	
	public void releaseAllHandles() {
		if (m_activeSources.Count > 0) {
			Debug.LogWarning(getErrorPrefix() + "Entire pool was released while there were active running instances ! Are you sure this is normal ?");
		}
		m_availableEvents.Clear();
	}
	
	public int getNumberRunningInstances() {
		return (m_activeSources.Count);
	}
	
	public List<FmodEventAudioSource> getActiveSources() {
		return (m_activeSources);
	}
			
	private string getErrorPrefix() {
		return ("FMOD_Unity: FmodEventPool(" + m_event.getName() + ") : ");
	}
	
	private void ERRCHECK(FMOD.RESULT result)
    {
		if (result != FMOD.RESULT.OK) {
            Debug.LogError(getErrorPrefix() + result + " - " + FMOD.Error.String(result));
        }
    }
}