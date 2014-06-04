using UnityEngine;
using System.Collections;

public class CallbacksTests : MonoBehaviour {
	
	public FmodEventAudioSource source;
	public bool forbidEventStealth = true;
	
	void Start() {
		if (source != null) {
			source.EventStarted += LogEventStart;
			source.EventFinished += LogEventFinished;;
			source.EventStolen += LogEventStolen;
			source.EventStolen += ForbidEventStolen;
			source.SoundDefStart += LogSoundDefStart;
			source.SoundDefEnd += LogSoundDefEnd;
		}
	}
	
	FMOD.RESULT LogEventStart(FmodEventAudioSource src) {
		Debug.Log("TEST: Audiosource '" + src.name + "' just started event '" + src.getSource().getName());
		return (FMOD.RESULT.OK);
	}
	FMOD.RESULT LogEventFinished(FmodEventAudioSource src) {
		Debug.Log("TEST: Audiosource '" + src.name + "' just finished event '" + src.getSource().getName());
		return (FMOD.RESULT.OK);
	}
	FMOD.RESULT LogEventStolen(FmodEventAudioSource src) {
		Debug.Log("TEST: Audiosource '" + src.name + "' event '" + src.getSource().getName() + " is being stolen");
		return (FMOD.RESULT.OK);
	}
	
	FMOD.RESULT LogSoundDefStart(FmodEventAudioSource src, string soundDefName, int waveIndexInSoundDef) {
		Debug.Log("TEST: Audiosource '" + src.name + "' event '" + src.getSource().getName() +
			" just started sound def '" + soundDefName + "' with sound of index '" + waveIndexInSoundDef + "'"
			);
		return (FMOD.RESULT.OK);
	}
	FMOD.RESULT LogSoundDefEnd(FmodEventAudioSource src, string soundDefName, int waveIndexInSoundDef) {
		Debug.Log("TEST: Audiosource '" + src.name + "' event '" + src.getSource().getName() +
			" just ended sound def '" + soundDefName + "' with sound of index '" + waveIndexInSoundDef + "'"
			);
		return (FMOD.RESULT.OK);
	}

	FMOD.RESULT ForbidEventStolen(FmodEventAudioSource src) {
		if (forbidEventStealth) {
			Debug.Log("TEST: Audiosource '" + src.name + "' event '" + src.getSource().getName() + " won't be stolen on my watch !");
			return (FMOD.RESULT.ERR_EVENT_FAILED);
		}
		return (FMOD.RESULT.OK);
	}
}
