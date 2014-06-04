using UnityEngine;
using System.Collections;

public class FmodMusicCue {
	public int Id { get { return m_id; } }
	public string Name { get { return m_name; } }
	private int m_id;
	private string m_name;

	private FmodMusicSystem m_musicSystem;
	private FMOD.MusicPrompt m_prompt;
	private bool m_isActive = false;
	
	public FmodMusicCue(FMOD.MUSIC_ENTITY cue, FmodMusicSystem musicSystem) {
		m_id = (int)cue.id;
		m_name = cue.name;
		
		m_musicSystem = musicSystem;
		loadMusicCue();
	}
	
	public bool isLoaded() {
		return (m_prompt != null);
	}
	
	public void loadMusicCue() {
		if (m_musicSystem != null && m_prompt == null) {
			m_musicSystem.loadMusicCue(this);
		}
	}

	public void setMusicPrompt(FMOD.MusicPrompt prompt) {
		if (m_prompt == null) {
			m_prompt = prompt;
		}
	}
	
	public bool isActive() {
		return (m_isActive);
	}
	
	/**
	 * Gets the active status of the cue from the underlying FMOD engine.
	 * This method should always return the same value as isActive().
	 */
	public bool isActiveInFMOD() {
		bool active = isActive();
		
		m_prompt.isActive(ref active);
		return (active);
	}
	
	public void begin() {
		if (m_prompt == null) {
			loadMusicCue();
		}
		m_isActive = true;
		m_prompt.begin();
	}

	public void end() {
		if (m_prompt == null) {
			loadMusicCue();
		}
		m_isActive = false;
		m_prompt.end();
	}

	public void prompt() {
		if (m_prompt == null) {
			loadMusicCue();
		}
		m_musicSystem.promptMusicCue(this);
	}
}
