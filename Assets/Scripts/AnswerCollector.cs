using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;
using System.IO;
using System;

public class AnswerCollector : MonoBehaviour
{
	public ToggleGroup row1,row2,row3,row4,row5,row6;
	public ToggleGroup row12,row22,row32,row42,row52,row62;
	public ToggleGroup row13,row23,row33,row43;//row53,row63;
	public GameObject surveyStyle1;
	
	private ToggleGroup[,] rowlist;
	// private Toggle[] togglesrow1,togglesrow2,togglesrow3,togglesrow4,togglesrow5;
	private Toggle[,][] fullpage;
	private Canvas[] tasks;
	private int currentstyle = 1, currentvideo = 1;
	private string[] questions, lines;
	private string path, alllines;
	private StreamWriter writer;
	private float tasktimer = 0f;
	private bool taskOn = false;
	private TextEditor t;

	public GameObject videoPlayerHolder;
	public GameObject thumbnailPlayerHolder;

	private VideoPlayer videoPlayer;
	private VideoPlayer thumbnailPlayer;
	
    // Start is called before the first frame update
    void Start()
    {
		path = Application.dataPath + "\\ANSWERS.txt";
		
		// 5 rows with 5 answers each
		// This looks brute forced, however, it's the simplest.
		
		rowlist = new ToggleGroup[3,6];
		rowlist[0,0] = row1;
		rowlist[0,1] = row2;
		rowlist[0,2] = row3;
		rowlist[0,3] = row4;
		rowlist[0,4] = row5;
		rowlist[0,5] = row6;
		
		// 3 Survey pages, 5 rows, 5 buttons.
		fullpage = new Toggle[3,6][];
		
        for(int i = 0; i<6; i++)
		{
			fullpage[0,i] = rowlist[0,i].GetComponentsInChildren<Toggle>();
		}
		
		rowlist[1,0] = row12;
		rowlist[1,1] = row22;
		rowlist[1,2] = row32;
		rowlist[1,3] = row42;
		rowlist[1,4] = row52;
		rowlist[1,5] = row62;
		
		
        for(int i = 0; i<6; i++)
		{
			fullpage[1,i] = rowlist[1,i].GetComponentsInChildren<Toggle>();
		}
		
		rowlist[2,0] = row13;
		rowlist[2,1] = row23;
		rowlist[2,2] = row33;
		rowlist[2,3] = row43;
		//rowlist[2,4] = row53;
		
        for(int i = 0; i<4; i++)
		{
			fullpage[2,i] = rowlist[2,i].GetComponentsInChildren<Toggle>();
		}
		
		
		
		
		//Now, fullpage contains all toggles..
		
		surveyStyle1.SetActive(false);
		t = new TextEditor();
		writer = new StreamWriter(path, true);

		videoPlayer = videoPlayerHolder.GetComponent<VideoPlayer>();
		thumbnailPlayer = videoPlayerHolder.GetComponent<VideoPlayer>();
	}

    // Update is called once per frame
    void Update()
    {
		if(taskOn)
		{
			tasktimer += Time.deltaTime;
		}
    }
	
	public void DoneButton()
	{
		writer.WriteLine("Interface style of this questionnaire: " + currentstyle + "--");
		writer.WriteLine("Video: " + currentvideo + "--");
		writer.WriteLine("Answers to the survey --");
		
		for(int j = 0; j<6; j++)
		{
			//writer.WriteLine("Question: " + questions[j]);
			if(j == 4 && currentstyle == 3)
			{
				break;
			}
			for(int i = 0; i<5; i++)
			{
				if(fullpage[currentstyle-1,j][i].isOn)
					writer.WriteLine((i+1) + /*" Out of 5"*/  "--");
			
			}
			
		}
		
		writer.Close();

		surveyStyle1.SetActive(false);
		lines = File.ReadAllLines(path);
		alllines = string.Join("", lines);
		alllines = alllines.Replace("--", "\n");
		t.text = alllines;
		t.SelectAll();
		t.Copy();

		// Old code for multiple tasks and multiple questionnaires
		/*switch (currentstyle)
		{
			case 1:
				surveyStyle1.SetActive(false);
				lines = File.ReadAllLines(path);
				alllines = string.Join("",lines);
				alllines = alllines.Replace("--", "\n");
				t.text = alllines;
				t.SelectAll();
				t.Copy();
				completed.GetComponent<Canvas>().enabled = true;
				break;
			case 2:
				surveyStyle2.SetActive(false);
				lines = File.ReadAllLines(path);
				alllines = string.Join("",lines);
				alllines = alllines.Replace("--", "\n");
				t.text = alllines;
				t.SelectAll();
				t.Copy();
				completed.GetComponent<Canvas>().enabled = true;
				break;
			case 3: // Style 3 is tested FIRST FROM NOW ON
				surveyStyle3.SetActive(false);
				/*
				string[] lines = File.ReadAllLines(path);
				string alllines = string.Join("",lines);
				alllines = alllines.Replace("--", "\n");
				t.text = alllines;
				t.SelectAll();
				t.Copy();
				completed.GetComponent<Canvas>().enabled = true;
				
				break;	
			default:
				break;
		}*/
		writer = new StreamWriter(path, true);
		Application.Quit();
		UnityEditor.EditorApplication.isPlaying = false;

	}
	
	public void SurveyButton()
	{

		surveyStyle1.SetActive(true);
		videoPlayer.Pause();
		thumbnailPlayer.Pause();

		// Old code for multiple tasks and multiple questionnaires
		/*
		switch (currentstyle)
		{
			case 1:
				surveyStyle1.SetActive(true);
				break;
			case 2:
				surveyStyle2.SetActive(true);
				break;
			case 3:
				surveyStyle3.SetActive(true);
				break;
			default:
				break;
		}
		*/
	}

	/*
	public void TaskButton()
	{
		tasks[currentvideo-1].GetComponent<Canvas>().enabled = true;
		
		if(taskOn)
		{
			writer.WriteLine("Interface style: " + currentstyle + " Video style: " + currentvideo + "Task time: --" + tasktimer + "--");
		}
		
		taskOn = true;
	}
	
	public void EndTaskButton()
	{
		tasks[currentvideo-1].GetComponent<Canvas>().enabled = false;
		
		if(taskOn)
		{
			writer.WriteLine("Interface style: " + currentstyle + " Video style: " + currentvideo + "Task time: --" + tasktimer + "--");
			tasktimer = 0f;
		}
		
		taskOn = false;
		
	}
	*/


	public void Setstyle1()
	{
		currentstyle = 1;
	}
	public void Setvideo1()
	{
		currentvideo = 1;
	}
	public void Setstyle2()
	{
		currentstyle = 2;
	}
	public void Setvideo2()
	{
		currentvideo = 2;
	}
	public void Setstyle3()
	{
		currentstyle = 3;
	}
	public void Setvideo3()
	{
		currentvideo = 3;
	}
	
}
