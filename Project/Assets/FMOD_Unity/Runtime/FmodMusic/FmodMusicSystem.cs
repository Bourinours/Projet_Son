using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FmodMusicSystem {
	static void ERRCHECK(FMOD.RESULT result)
    {
		if (result != FMOD.RESULT.OK)
        {
            Debug.Log("FMOD_Unity: FmodMusicSystem: " + result + " - " + FMOD.Error.String(result));
        }
    }

	
	private FMOD.MusicSystem m_musicSystem;
	private List<FmodMusicCue> m_cues = null;
	private List<FmodMusicParameter> m_parameters = null;
	
	public FmodMusicSystem(FMOD.MusicSystem musicSystem) {
		if (m_musicSystem == null) {
			m_musicSystem = musicSystem;
		}
	}
	
	public void loadSoundData() {
		loadSoundData(FMOD.EVENT_RESOURCE.STREAMS_AND_SAMPLES, FMOD.EVENT_MODE.DEFAULT);
	}

	public void loadSoundData(FMOD.EVENT_RESOURCE resource, FMOD.EVENT_MODE mode) {
		if (m_musicSystem != null) {
			ERRCHECK(m_musicSystem.loadSoundData(resource, mode));
		}		
	}
	
	public void release() {
		if (m_musicSystem != null) {
			m_musicSystem.freeSoundData(true);
			m_musicSystem = null;
		}
	}
	
	public void freeSoundData(bool waitUntilReady) {
		if (m_musicSystem != null) {
			m_musicSystem.freeSoundData(waitUntilReady);
		}
	}
	
	public List<FmodMusicCue> getCues() {
		_loadData();
		return (m_cues);
	}
	
	public List<FmodMusicParameter> getParameters() {
		_loadData();
		return (m_parameters);
	}
	
	private void _loadCues() {
		FMOD.MUSIC_ITERATOR it = new FMOD.MUSIC_ITERATOR();
		FMOD.RESULT result = FMOD.RESULT.OK;
		
		if (m_cues != null || m_musicSystem == null) {
			return ;
		}
		m_cues = new List<FmodMusicCue>();
		result = m_musicSystem.getCues(ref it, ""); //TODO: crash here. bad it init ? bad filter ? NOPE, just that a project must be loaded first. At least one. We might have to check on that.
		if (result == FMOD.RESULT.OK) {
			while (it.value.ToInt32() != 0) {
				FMOD.MUSIC_ENTITY entity = FmodMusicEntityBuilder.getMusicEntity(it);
				FmodMusicCue cue = new FmodMusicCue(entity, this);
				
				m_cues.Add(cue);
			}
		} else {
			Debug.LogError("FMOD_Unity: Error while loading music cues: " + FMOD.Error.String(result));
		}
	}

	private void _loadParameters() {
		FMOD.MUSIC_ITERATOR it = new FMOD.MUSIC_ITERATOR();
		FMOD.RESULT result = FMOD.RESULT.OK;
		
		if (m_parameters != null || m_musicSystem == null) {
			return ;
		}
		m_parameters = new List<FmodMusicParameter>();
		result = m_musicSystem.getParameters(ref it, "");
		if (result == FMOD.RESULT.OK) {
			while (it.value.ToInt32() != 0) {
				FMOD.MUSIC_ENTITY entity = FmodMusicEntityBuilder.getMusicEntity(it);
				FmodMusicParameter param = new FmodMusicParameter(entity, this);
				
				m_parameters.Add(param);
			}
		} else {
			Debug.LogError("FMOD_Unity: Error while loading music parameters: " + FMOD.Error.String(result));
		}
	}
	
	private void _loadData() {
		loadSoundData();
		_loadCues();
		_loadParameters();
	}
	
	public void loadMusicCue(FmodMusicCue cue) {
		if (m_musicSystem != null && !cue.isLoaded()) {
			FMOD.RESULT result = FMOD.RESULT.OK;
			FMOD.MusicPrompt prompt = new FMOD.MusicPrompt();
			
			result = m_musicSystem.prepareCue((uint)cue.Id, ref prompt);
			if (result == FMOD.RESULT.OK) {
				cue.setMusicPrompt(prompt);
			}
		}
	}
	
	public void promptMusicCue(FmodMusicCue cue) {
		FMOD.RESULT result = FMOD.RESULT.OK;
		
		if (cue != null && m_musicSystem != null) {
			result = m_musicSystem.promptCue((uint)cue.Id);
			ERRCHECK(result);
		}
	}
	
	public FmodMusicCue getMusicCue(string name) {
		_loadData();
		if (m_cues == null) {
			return (null);
		}
		foreach (FmodMusicCue c in m_cues) {
			if (c.Name == name) {
				return (c);
			}
		}
		return (null);
	}
	
	public void beginCue(string cueName) {
		FmodMusicCue cue = getMusicCue(cueName);
		
		if (cue != null) {
			loadMusicCue(cue);
			cue.begin();
		}
	}

	public void endCue(string cueName) {
		FmodMusicCue cue = getMusicCue(cueName);
		
		if (cue != null) {
			loadMusicCue(cue);
			cue.end();
		}
	}
	
	public FmodMusicParameter getParameter(string name) {
		_loadData();
		
		foreach (FmodMusicParameter p in m_parameters) {
			if (p.Name == name) {
				return (p);
			}
		}
		return (null);
	}
	
	public float getParameterValue(string name) {
		FmodMusicParameter p = getParameter(name);
		
		if (p != null) {
			return (p.getValue());
		}
		return (0);
	}

	public void setParameterValue(string name, float val) {
		FmodMusicParameter p = getParameter(name);
		
		if (p != null) {
			p.setValue(val);
		}
	}
	
	public float getParameterValue(FmodMusicParameter param) {
		float ret = 0;
		
		if (m_musicSystem != null) {
			m_musicSystem.getParameterValue((uint)param.Id, ref ret);
		}
		return (ret);
	}

	public void setParameterValue(FmodMusicParameter param, float newVal) {
		if (m_musicSystem != null) {
			m_musicSystem.setParameterValue((uint)param.Id, newVal);
		}
	}
	
	/*
	public void reset() {
		if (m_musicSystem != null) {
			m_musicSystem.reset();
			foreach (FmodMusicCue cue in m_cues) {
			}
		}
	}
	*/
}
