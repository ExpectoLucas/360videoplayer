using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;
using System.IO;
using System;

public class PipSpawner : MonoBehaviour
{
	
	public VideoPlayer vp, preview;
	public Text timeCurr;
    public Text timeTotal;
    public Slider timeSlider;
	public Canvas pip, pipl, pipr, pipm1;
	public GameObject planel, planem, planer,planem1;
	public Canvas surveycanvas;
	
	private Canvas[] pips;
	private GameObject[] planes;
	private bool sliderdown = false, jumped = false, started = false, updated = false, wasplaying = false;
	private float timer =0;
	private System.Random rnd;
	private int startingstyle, currentvideo, secondvideo;
	private int sett = 2; //Update with each participant
	
	AnswerCollector survey;
	TimeHandler timeHandler;
	InputHandler inputHandler;
	
    // Start is called before the first frame update
    void Start()
    {
		pips = new Canvas[3];
		pips[0] = pipl;
		pips[1] = pip;
		pips[2] = pipr;
		
		//Saving it to cache memory
		foreach(Canvas player in pips)
			{
				player.GetComponent<Canvas>().enabled = true;
			}
			
		pipm1.GetComponent<Canvas>().enabled = true;
		
		
			
		
		
		planes = new GameObject[4];
		planes[0] = planel;
		planes[1] = planem;
		planes[2] = planer;
		planes[3] = planem1;
		foreach(GameObject plane in planes)
			{
				plane.SetActive(false);
			}
			
		
		//Debug.Log("Videothumbnail loaded?");
		
		
		
		timeHandler = GetComponent<TimeHandler>();
		inputHandler = GetComponent<InputHandler>(); 
		preview.Pause();
		vp.Pause();
		
		foreach(Canvas player in pips)
			{
				player.GetComponent<Canvas>().enabled = false;
			}
		pipm1.GetComponent<Canvas>().enabled = false;
		
			
		
		survey = surveycanvas.GetComponent<AnswerCollector>();
		
		rnd = new System.Random();
		
		
		
		// Parse the setting from the text file
		//StreamReader sr = new StreamReader(path);
		//string sett = sr.ReadLine();
		
		
		Debug.Log("Testing setting: " + sett);
		
		//int setting = int.Parse(sett);
		int setting = sett;
		startingstyle = 1 + (setting /7);
		currentvideo = 1 + (((setting -1)%6) /2);
		secondvideo = 1 + ((setting%3) + ((setting-1)/3)%2)%3;
		
		//Debug.Log("Testing setting:" + sett + " Style: " + startingstyle + " video: " + currentvideo + " 2nd video: " +secondvideo);
		
		/* This is how the setting from the text file (Numbered 1 to 12) are calculated to a combination of videos and interfaces.
		string[] combos = new string[13];
		for(int i = 1; i <13; i++)
		{
			startingstyle = 1 + (i /7);
			currentvideo = 1 + (((i -1)%6) /2);
			secondvideo = 1 + ((i%3) + ((i-1)/3)%2)%3;
			
			combos[i] = (string)("Testing i:" + i + " Style: " + startingstyle + " video: " + currentvideo + " 2nd video: " +secondvideo);
	     	
		}
		for(int i = 1; i <13; i++)
		{
			Debug.Log(combos[i]);
		}
		*/
		
		//int setting = sett;
		
		
		switch(currentvideo)
		{
			case 1:
				VideoOne();
				break;
			case 2: 
				VideoTwo();
				break;
			case 3:
				VideoThree();
				break;
			default: 
				break;			
		}
		
		switch(startingstyle)
		{
			case 1:
				StyleOne();
				break;
			case 2: 
				StyleTwo();
				break;
			default: 
				break;			
		}
    }
	
	 // Update is called once per frame
	 void Update()
    {
		// timeCurr.text = (System.Math.Floor(vp.time / 60)).ToString() + ':' + System.Math.Floor(vp.time % 60).ToString("00");
		
		if(sliderdown || jumped)
		{
			//Non-blocking way to wait.
			
			
			 
			if(timer < 0.5f)
			{
				if(!jumped)
				{
					preview.Pause();
				
					preview.time = (timeSlider.normalizedValue) * preview.length;
					//timeHandler.ForceUpdateTime(preview.time);
					
					timer = 1.0f;
					//preview.Play();
			 
				}
				else if(timer < 0.5f)
				{
					//If we jumped back or forward 10 seconds, remove pip screens and play main video.
					vp.time = preview.time;
					
					vp.Play();
					preview.Play();
					
					jumped = false;
					
					foreach(Canvas player in pips)
					{
						player.GetComponent<Canvas>().enabled = false;
					}
					
					foreach(GameObject plane in planes)
					{
						plane.SetActive(false);
					}
					
				}
			}
			
			timer -= Time.deltaTime;
		}
		else
			timer = 1.5f;
	 
		
	  
    }
	
	public void PauseButton()
	{
		
			foreach(Canvas player in pips)
			{
				player.GetComponent<Canvas>().enabled = true;
			}
			//Debug.Log("Players enabled!");
		
			foreach(GameObject plane in planes)
			{
				plane.SetActive(true);
			}
		
		vp.Pause();
		preview.Pause();
		
		
		
	}
	
	
	//When jumping, pause the main video player and activate previews. Once the user stops tapping jump for a second,
	//The preview windows go away and the main video jumps there.
	public void Jumpback()
	{
		if(!jumped)
		{
			vp.Pause();
			preview.Pause();
			
		}
		
		foreach(Canvas player in pips)
		{
			player.GetComponent<Canvas>().enabled = true;
		}
		foreach(GameObject plane in planes)
		{
			plane.SetActive(true);
		}
		
		timer = 1.5f;
		jumped = true;
		

		if (preview.time - 5f > 0f)
        {
			
			preview.time = preview.time -5f;
        }
        else
        {
			
			preview.time = 0f;
        }
		
		timeHandler.ForceUpdateSlider(preview.time);
		
		
		
		
	}
	
	public void Jumpforward()
	{
		
		if(!jumped)
		{
			vp.Pause();
			preview.Pause();
			
		}
		
		
		
		foreach(Canvas player in pips)
		{
			player.GetComponent<Canvas>().enabled = true;
		}
		foreach(GameObject plane in planes)
		{
			plane.SetActive(true);
		}
		
		
		timer = 1.5f;
		jumped = true;
		
		
	    if (preview.time + 5f < preview.length)
        {
			preview.time = preview.time +5f;
        }
        else
        {
			preview.time = preview.length;
        }
		
		timeHandler.ForceUpdateSlider(preview.time);
		
		
	}
	
	
	public void Playbutton()
	{
		if(!started)
		{
			started = true;
			
			//Start with 1 of the two thumbnail styles
			if(startingstyle == 1)
			{
				survey.Setstyle1();
			}
			else if(startingstyle == 2)
			{
				survey.Setstyle2();
			}
			
			//Start with a video
			switch(currentvideo)
	    	{
				case 1:
					VideoOne();
					survey.Setvideo1();
					break;
				case 2: 
					VideoTwo();
					survey.Setvideo2();
					break;
				case 3:
					VideoThree();
					survey.Setvideo3();
					break;
				default: 
					break;	
			}					
		
			
			
			
			StartCoroutine(Waiter()); // Debugging purposes. Otherwise slider will not be loaded in, which could cause more problems on slower devices.
		}
		else
		{
			jumped = false;
		}
		
		
	
		
		
		foreach(Canvas player in pips)
		{
			player.GetComponent<Canvas>().enabled = false;
		}
		
		foreach(GameObject plane in planes)
		{
			plane.SetActive(false);
		}	
		
		vp.Play();	
		preview.Play();
		
	}
	
	private IEnumerator Waiter()
	{
		Debug.Log("STARTED WAITING");
		yield return new WaitForSeconds(1);
		vp.Pause();
		preview.Pause();
		
		
		 yield return new WaitForSeconds(1);
		
		
		inputHandler.PlayButton();
		Debug.Log("DONE WAITING");
		timeHandler.UpdateTotalTime();
		
	}
	
	private IEnumerator WaiterUpdate()
	{
		yield return new WaitForSeconds(1);
		updated = false;
		
	}
	
	
	public void Sliderdown()
	{
		if(!sliderdown)
			wasplaying = vp.isPlaying;
		
		sliderdown = true;
		
			foreach(Canvas player in pips)
			{
				player.GetComponent<Canvas>().enabled = true;
			}
			foreach(GameObject plane in planes)
			{
				plane.SetActive(true);
			}
			
		
			// Show a preview, and go to that timestamp when letting go in the sliderup function
			
		wasplaying = vp.isPlaying;
			
		
		preview.Pause();
		vp.Pause();
		if(!updated)
		{
			preview.time = (timeSlider.normalizedValue) * preview.length;
			timeHandler.UpdateDraggedTime();
			StartCoroutine(WaiterUpdate()); //If there is no delay, it will attempt to update faster than the thumbnails can update, making it seem unresponsive.
		}
		
	}
	
   
    public void Sliderup()
    {
		
		//preview.time = (timeSlider.normalizedValue) * preview.length;
		
		
         
		timeHandler.UpdateDraggedTime();
		
		
		//timeCurr.text = (System.Math.Floor(vp.time / 60)).ToString() + ':' + System.Math.Floor(vp.time % 60).ToString("00");
		
		
		
		
		
		
		sliderdown = false;
		
		foreach(Canvas player in pips)
		{
			player.GetComponent<Canvas>().enabled = false;
		}
		foreach(GameObject plane in planes)
		{
			plane.SetActive(false);
		}
		vp.time = preview.time;
		jumped = false;
		if(wasplaying)
		{
		 vp.Play();
		 preview.Play();
		 wasplaying = false;
		}
		
    }
	
	public void Nextlayout()
	{
		switch(secondvideo)
		{
			case 1:
				VideoOne();
				survey.Setstyle3();
				survey.Setvideo1();
				break;
			case 2: 
				VideoTwo();
				survey.Setstyle3();
				survey.Setvideo2();
				break;
			case 3:
				VideoThree();
				survey.Setstyle3();
				survey.Setvideo3();
				break;
			default: 
				break;			
		}
		
		StyleThree();
		
	}
	
	
	public void StyleOne()
	{
		
		foreach(Canvas player in pips)
		{
			player.GetComponent<Canvas>().enabled = false;
		}
					
		foreach(GameObject plane in planes)
		{
			plane.SetActive(false);
		}
		
		
		pips = new Canvas[3];
		
		pips[0] = pipl;
		pips[1] = pip;
		pips[2] = pipr;
		
		planes = new GameObject[3];
		planes[0] = planel;
		planes[1] = planem;
		planes[2] = planer;
		
		//timeHandler.ForceUpdateSlider();
	}
	
	public void VideoOne()
	{
		currentvideo = 1;
		vp.Pause();
		preview.Pause();
		vp.time = 0;
		preview.time = 0;
		
		//vp.url = "file://Assets/Lake-Baikal-Hovercraft-Tour.mp4";
		//preview.url = "file://Assets/Lake-Baikal-Hovercraft-Tour.mp4";
		vp.url = Application.streamingAssetsPath  + "/Lake-Baikal-Hovercraft-Tour.mp4";
		preview.url = Application.streamingAssetsPath  + "/Lake-Baikal-Hovercraft-Tour.mp4";
	}
	
	public void StyleTwo()
	{
		
		foreach(Canvas player in pips)
		{
			player.GetComponent<Canvas>().enabled = false;
		}
					
		foreach(GameObject plane in planes)
		{
			plane.SetActive(false);
		}
		
		pips = new Canvas[1];
		pips[0] = pipm1;
		//pips[1] = null;
		//pips[2] = null; 
		
		planes = new GameObject[1];
		planes[0] = planem1;
		//pipm1.GetComponent<Canvas>().enabled = true;
		
		//timeHandler.ForceUpdateSlider();
	}
	
	public void VideoTwo()
	{
		currentvideo = 2;
		vp.Pause();
		preview.Pause();
		vp.time = 0;
		preview.time = 0;
		
		vp.url = Application.streamingAssetsPath + "/360 Campus Tour   Wageningen University & Research.mp4";
		preview.url = Application.streamingAssetsPath  + "/360 Campus Tour   Wageningen University & Research.mp4";
	}
	
	
	public void StyleThree()
	{
		
		foreach(Canvas player in pips)
		{
			player.GetComponent<Canvas>().enabled = false;
		}
					
		foreach(GameObject plane in planes)
		{
			plane.SetActive(false);
		}
		
		pips = new Canvas[0];
		
		planes = new GameObject[0];
		started = false;
		startingstyle = 3;
		//timeHandler.ForceUpdateSlider();
		
	}
	public void VideoThree()
	{
		
		currentvideo = 3;
		vp.Pause();
		preview.Pause();
		vp.time = 0;
		preview.time = 0;
		
		
		//vp.Source = VideoSource.Url;
		vp.url = Application.streamingAssetsPath  + "/Catcafe.mp4";
		preview.url = Application.streamingAssetsPath  + "/Catcafe.mp4";
		
	}
		
	
}
