using UnityEngine;
using System.Collections;

public class FmodMusicParameter {
	public int Id { get { return m_id; } }
	public string Name { get { return m_name; } }
	private int m_id;
	private string m_name;

	private float m_value;
	private bool m_valueKnown = false;
	private FmodMusicSystem	m_musicSystem;
	
	public FmodMusicParameter(FMOD.MUSIC_ENTITY cue, FmodMusicSystem musicSystem) {
		m_id = (int)cue.id;
		m_name = cue.name;
		
		m_musicSystem = musicSystem;
	}
	
	public float getValue() {
		if (!m_valueKnown) {
			m_value = m_musicSystem.getParameterValue(this);
			m_valueKnown = true;
		}
		return (m_value);
	}
	
	public void setValue(float val) {
		m_musicSystem.setParameterValue(this, val);
	}
}
