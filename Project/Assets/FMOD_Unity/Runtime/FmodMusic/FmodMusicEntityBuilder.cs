using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;

public class FmodMusicEntityBuilder
{
	public static FMOD.MUSIC_ENTITY getMusicEntity(FMOD.MUSIC_ITERATOR it) {
		FMOD.MUSIC_ENTITY ret = new FMOD.MUSIC_ENTITY();
		
		if (it.value.ToInt32() != 0) {
			Marshal.PtrToStructure(it.value, ret);
		}
		return (ret);
	}
}

