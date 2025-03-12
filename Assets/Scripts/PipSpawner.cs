using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;
using System.IO;
using System;

public class PipSpawner : MonoBehaviour
{
	
	// video player objects, one for the main sphere and one for the thumbnails, which has the 2D texture
	public VideoPlayer videoPlayer, thumbnailPlayer;

	// public Text timeCurr;
    // public Text timeTotal;

    public Slider timeSlider;

	public GameObject thumbnailCanvas;

	public Canvas thumbnail;
	public GameObject background;

	public Canvas surveycanvas;
	
	private Canvas[] pips;
	private GameObject[] planes;
	private bool sliderdown = false, jumped = false, started = false, updated = false, wasplaying = false;
	private float timer = 0;
	private System.Random rnd;
	private int secondstyle, currentvideo, secondvideo;
	//private int sett = 8; //Update with each participant
	private string path;
	
	AnswerCollector survey;
	TimeHandler timeHandler;
	InputHandler inputHandler;

    private void Awake()
    {
		//this.timeSlider.onValueChanged.AddListener(this.OnSliderChanged);
	}

	//private void OnSliderChanged(float value)
 //   {
	//	videoPlayer.time = value * videoPlayer.length;
	//	timeHandler.UpdateDraggedTime();
	//	Debug.Log(value);
	//}

    // Start is called before the first frame update.
    // 
    void Start()
    {
		
		//thumbnail.GetComponent<Canvas>().enabled = true;
		//background.SetActive(false);
		//thumbnail.GetComponent<Canvas>().enabled = false;

		thumbnailCanvas.SetActive(false);

		timeHandler = GetComponent<TimeHandler>();
		inputHandler = GetComponent<InputHandler>(); 
		thumbnailPlayer.Pause();
		videoPlayer.Pause();

		survey = surveycanvas.GetComponent<AnswerCollector>();

		currentvideo = 1;
		secondvideo = 2;


		/* Old code for reading in a settings file and basing the experiment process based on that number.
		rnd = new System.Random();
		// Parse the setting from the text file
		path = Application.streamingAssetsPath + "\\Setting.txt";
		StreamReader sr = new StreamReader(path);
		string line = sr.ReadLine();

		int setting = int.Parse(line);
		secondstyle = 1 + (setting /7);
		currentvideo = 1 + (((setting -1)%6) /2);
		secondvideo = 1 + ((setting%3) + ((setting-1)/3)%2)%3;
		
		Debug.Log("Testing setting:" + setting + " Style: " + secondstyle + " video: " + currentvideo + " 2nd video: " +secondvideo);
		*/
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
    }

	// Update is called once per frame
	void Update()
	{
		// timeCurr.text = (System.Math.Floor(videoPlayer.time / 60)).ToString() + ':' + System.Math.Floor(videoPlayer.time % 60).ToString("00");

		// If we interact with the slider (either by dragging or letting go)
		if (timer < 0.1f)
		{
			if (sliderdown)
			{
				videoPlayer.time = timeSlider.normalizedValue * videoPlayer.length;
				timeHandler.UpdateDraggedTime();
				//Debug.Log(videoPlayer.frame);
			}
			timer = 1.0f;
		}
		else
        {
			timer -= Time.deltaTime;
		}
	}

	// Check if more than one second has passed.
	//if (timer < 0.5f)
	//{
	//	// If we did not jump (yet), update the thumbnail player to be at the same time as the slider.
	//	if(!jumped)
	//	{
	//		thumbnailPlayer.Pause();

	//		thumbnailPlayer.time = (timeSlider.normalizedValue) * thumbnailPlayer.length;
	//		//timeHandler.ForceUpdateTime(preview.time);

	//		timer = 1.0f;
	//		//preview.Play();

	//	}
	//else if(timer < 0.5f)
	//{
	//	//If we jumped in time, disable the thumbnail canvas and update the main video player.
	//	videoPlayer.time = thumbnailPlayer.time;

	//	videoPlayer.Play();
	//	thumbnailPlayer.Play();

	//	jumped = false;

	//	//background.SetActive(false);
	//	//thumbnail.GetComponent<Canvas>().enabled = false;
	//	thumbnailCanvas.SetActive(false);

	//}
	//	}
	//	timer -= Time.deltaTime;
	//} 
	//else
	//	timer = 1.5f;

public void PauseButton()
	{

		//thumbnail.GetComponent<Canvas>().enabled = true;
		//background.SetActive(true);

		thumbnailCanvas.SetActive(true);

		videoPlayer.Pause();
		thumbnailPlayer.Pause();
		
		
		
	}
	
	//When jumping, pause the main video player and activate previews. Once the user stops tapping jump for a second,
	//The preview windows go away and the main video jumps there.
	public void Jumpback()
	{
		timer = 1.5f;

		if (!jumped)
		{
			videoPlayer.Pause();
			thumbnailPlayer.Pause();

		}

		//thumbnail.GetComponent<Canvas>().enabled = true;
		//background.SetActive(true);

		thumbnailCanvas.SetActive(true);

		jumped = true;


		if (thumbnailPlayer.time - 5f > 0f)
        {
			
			thumbnailPlayer.time = thumbnailPlayer.time -5f;
        }
        else
        {
			
			thumbnailPlayer.time = 0f;
        }
		
		timeHandler.ForceUpdateSlider(thumbnailPlayer.time);
		
		
		
		
	}
	
	public void Jumpforward()
	{

		timer = 1.5f;

		if (!jumped)
		{
			videoPlayer.Pause();
			thumbnailPlayer.Pause();
			
		}

		//thumbnail.GetComponent<Canvas>().enabled = true;
		//background.SetActive(true);

		thumbnailCanvas.SetActive(true);

		jumped = true;
		
	    if (thumbnailPlayer.time + 5f < thumbnailPlayer.length)
        {
			thumbnailPlayer.time = thumbnailPlayer.time +5f;
        }
        else
        {
			thumbnailPlayer.time = thumbnailPlayer.length;
        }
		
		timeHandler.ForceUpdateSlider(thumbnailPlayer.time);
		
	}
	
	
	// Plays the video.
	public void Playbutton()
	{

		// If the video was not yet started, the correct video is selected and started.
		if(!started)
		{
			started = true;
			survey.Setstyle2();
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
			//jumped = false;
		}



		//thumbnail.GetComponent<Canvas>().enabled = false;
		//background.SetActive(false);

		thumbnailCanvas.SetActive(false);

		videoPlayer.Play();	
		thumbnailPlayer.Play();
		
	}
	
	private IEnumerator Waiter()
	{
		videoPlayer.Pause();
		thumbnailPlayer.Pause();
		yield return new WaitForSeconds(1);

		Debug.Log("STARTED WAITING");

		//yield return new WaitForSeconds(1);
		
		
		inputHandler.PlayButton();
		Debug.Log("DONE WAITING");
		timeHandler.UpdateTotalTime();
		yield return new WaitForSeconds(1);

	}
	
	private IEnumerator WaiterUpdate()
	{
		yield return new WaitForSeconds(1);
		updated = false;
		
	}
	
	
	// Activates when the slider is pressed.
	public void Sliderdown()
	{
		wasplaying = videoPlayer.isPlaying;
		//thumbnailPlayer.Pause();
		videoPlayer.Pause();
		//thumbnailCanvas.SetActive(true);
		sliderdown = true;

		//thumbnailPlayer.time = (timeSlider.normalizedValue) * thumbnailPlayer.length;
		//timeHandler.UpdateDraggedTime();

		if (!updated)
		{
			//thumbnailPlayer.time = (timeSlider.normalizedValue) * thumbnailPlayer.length;
			//timeHandler.UpdateDraggedTime();
			//StartCoroutine(WaiterUpdate()); //If there is no delay, it will attempt to update faster than the thumbnails can update, making it seem unresponsive.
		}
		
	}
	
   // Activates when the slider is released
    public void Sliderup()
    {
		
		//preview.time = (timeSlider.normalizedValue) * preview.length;
		//timeHandler.UpdateDraggedTime();
		
		//timeCurr.text = (System.Math.Floor(vp.time / 60)).ToString() + ':' + System.Math.Floor(vp.time % 60).ToString("00");

		sliderdown = false;

		//thumbnail.GetComponent<Canvas>().enabled = false;
		//background.SetActive(false);

		//thumbnailCanvas.SetActive(false);

		//videoPlayer.time = thumbnailPlayer.time;
		jumped = false;
		if(wasplaying)
		{
			videoPlayer.Play();
			//thumbnailPlayer.Play();
			wasplaying = false;
		}
		
    }

	public void VideoOne()
	{
		currentvideo = 1;
		videoPlayer.Pause();
		thumbnailPlayer.Pause();
		videoPlayer.time = 0;
		thumbnailPlayer.time = 0;

		videoPlayer.url = Application.streamingAssetsPath  + "/Lake-Baikal-Hovercraft-Tour.mp4";
		thumbnailPlayer.url = Application.streamingAssetsPath  + "/Lake-Baikal-Hovercraft-Tour.mp4";
	}
	
	public void VideoTwo()
	{
		currentvideo = 2;
		videoPlayer.Pause();
		thumbnailPlayer.Pause();
		videoPlayer.time = 0;
		thumbnailPlayer.time = 0;
		
		videoPlayer.url = Application.streamingAssetsPath + "/360 Campus Tour   Wageningen University & Research.mp4";
		thumbnailPlayer.url = Application.streamingAssetsPath  + "/360 Campus Tour   Wageningen University & Research.mp4";
	}

	public void VideoThree()
	{
		
		currentvideo = 3;
		videoPlayer.Pause();
		thumbnailPlayer.Pause();
		videoPlayer.time = 0;
		thumbnailPlayer.time = 0;
		
		
		//vp.Source = VideoSource.Url;
		videoPlayer.url = Application.streamingAssetsPath  + "/Catcafe.mp4";
		thumbnailPlayer.url = Application.streamingAssetsPath  + "/Catcafe.mp4";
		
	}
		
	
}
