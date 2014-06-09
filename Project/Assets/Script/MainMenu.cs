using UnityEngine;
using System.Collections;

public class MainMenu : MonoBehaviour {
	
	//VARIABLES
	public float timeBeforePlayIntro;
	bool bIntroIsPlaying = false;
	public FmodEventAudioSource source;

	//TEXTURES
	public Texture2D background;
	public Texture2D loading;
	public Texture2D title;
	public Texture2D pressButton;
	
	//ON GUI DISPLAY
	void OnGUI()
	{
		//Draw Background
		GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), background);
		
		//Test if the level is loading 
		if (Application.isLoadingLevel)
		{
			//Draw loading title
			GUI.DrawTexture(new Rect((Screen.width / 2) - ((loading.width + 150) / 2), (Screen.height / 2) - (loading.height / 2), loading.width + 150, loading.height), loading);
			
		}
		else
		{
			//Draw Title
			GUI.DrawTexture(new Rect((Screen.width / 2) - (title.width / 2), (Screen.height / 5) - (title.height / 2), title.width, title.height), title);
			
			//Draw press button to continue
			GUI.DrawTexture(new Rect((Screen.width / 2) - (pressButton.width / 2), (Screen.height * 0.7f) - (pressButton.height / 2), pressButton.width, pressButton.height), pressButton);
			
		}
		
	}
	
	//COMMANDS FOR GUI
	void Update()
	{
		//test for playing intro
		if (!bIntroIsPlaying)
		{
			//Call for intro sound activation
			testTimerBeforeIntroStart();
			
		}
		
		//Check for button input (DEBUG ONLY !!! - To replace with Wands buttons)
		if (Input.GetKeyDown(KeyCode.Space) && source.CurrentStatus == FmodEventAudioSource.Status.Stopped)
		{
			//Load game level
			Application.LoadLevel("Main");
		}
		
	}
	
	//FUNCTIONS
	void testTimerBeforeIntroStart()
	{
		//Decrease timer with delta time
		timeBeforePlayIntro -= Time.deltaTime;
		
		//Test value of timer
		if (timeBeforePlayIntro <= 0)
		{
			//Activate intro sound
			//Play sound
			
			//Switch boolean
			bIntroIsPlaying = true;
			
		}
		
	}
	
}
