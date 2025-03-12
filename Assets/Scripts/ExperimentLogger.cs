using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Video;
using UnityEngine.UI;
using System.IO;    // For file output

public class ExperimentLogger : MonoBehaviour
{
    string currentParticipantPath;
    int participantID;

    public VideoPlayer vp;
    private int currentVideoID;

    private bool limitTime;
    private bool answerGiven;
    public Button answerButton;
    public Button[] interfaceButtons;
    private long timeVideoStarted; // The global system time on which the video was started the first time. To be compared to time on which the answer was confirmed.
    //private long timeSinceVideoStart;
    private long timeAnswerConfirmed; // The global system time on which the answer was confirmed.
    //private int answeredFrame;      // The frame answered by the user.
    private string[] correctFrames;
    private long correctFrame;       // The frame that is the correct answer, as loaded from the list of times per video.
    private int numberOfTimesCorrectAnswerPassed;

    private long previousFrame;

    private bool tutorialOn;
    public GameObject ERPTutImage;
    public GameObject CMPTutImage;
    public GameObject WrongAnswerImage;

    private int tut3Tolerance = 120;
    private int tut5Tolerance = 40;
    private bool tutorialCompleted;

    // Keep a copy of the executing coroutine
    private IEnumerator co;
    private IEnumerator wrongAnswerCo;

    private void Awake()
    {
        // Read which particpant ID we're currently up to.
        //currentParticipantPath = Application.dataPath + "/currentParticipant";
        //string contents = File.ReadAllText(currentParticipantPath);
        //Debug.Log(contents);
        participantID = ((int)(UnityEngine.Random.value * 10000000000));
        Debug.Log(participantID);

        // Retrieve all correct answers from the file that contains them and put them in an array.
        correctFrames = File.ReadAllLines(Application.dataPath + "/correctAnswers");

        previousFrame = -1; // Initialise previous value to -1 to prevent a correct answer on frame 0 being missed
    }

    private void Update()
    {
        // Check whether the target frame has been passed (if we weren't already on the correct frame, count +1 to the number of times we've passed the correct frame).
        //if (vp.frame != previousFrame && vp.frame == correctFrame)
        //if (previousFrame != correctFrame && vp.frame == correctFrame) 
        if (previousFrame != correctFrame && ((vp.frame <= correctFrame && previousFrame > correctFrame)||(previousFrame < correctFrame && vp.frame >= correctFrame))) 
        {
            numberOfTimesCorrectAnswerPassed++;
            //Debug.Log("Passed correct answer!");
        }
        previousFrame = vp.frame;

        // Check whether the previous or next participant key has been pressed.
        // Nah, let's just close the program between participants and update the "currentParticipant" file. That's more fool-proof.
    }

    ///// <summary>
    ///// To increase the number of times the correct answer (frame) has been passed. The object this function is called from must be aware of the correct answer for this video (loaded from txt file).
    ///// </summary>
    //public void CorrectAnswerPassed()
    //{
    //    numberOfTimesCorrectAnswerPassed++;
    //}

    /// <summary>
    /// Reset the relevant variables to prepare for a new video and answer.
    /// </summary>
    public void ResetOnVideoLoad()
    {
        if (GetComponentInParent<ExperimentToggle>().experimenting)
        {
            answerGiven = false;

            foreach (Button b in interfaceButtons)
            {
                b.interactable = true;      // And turn on all play control buttons regardless of setting, of course!
            }

            // Get video id from currently playing URL: the file name of the loaded video (e.g. 3,ERP), splitting on the comma and picking the first segment.
            string[] fileName = Path.GetFileNameWithoutExtension(vp.url).Split(',');
            currentVideoID = int.Parse(fileName[0]);

            // Now we use the video ID to retrieve the correct answer (frame) from the array of all answers
            if (currentVideoID != 3)        // In the case of tutorial 3, two answers are considered correct (due to slight ambiguity), so we'll do it differently in that case
            {
                correctFrame = long.Parse(correctFrames[currentVideoID - 1]);
            }

            numberOfTimesCorrectAnswerPassed = 0;

            if (currentVideoID == 3 || currentVideoID == 5)     // If the current video is one of the tutorial vids
            {
                if (fileName[1] == "CMP")                       // Now check whether it's ERP or CMP
                {
                    CMPTutImage.SetActive(true);
                }
                else                                            // If it's not CMP, it has to be ERP
                {
                    ERPTutImage.SetActive(true);
                }
                tutorialOn = true;
                //wrongAnswerCo = ShowWrongAnswer();              // Instantiate the wrong answer coroutine so that we can only ever have one of them active at a time and they replace each other when clicked multiple times.
                tutorialCompleted = false;
            }


            if (GetComponentInParent<ExperimentToggle>().experimenting)
            {
                answerButton.interactable = true;   // Only make the button interactable if we're running the experiment

                if (!tutorialOn)                    // And only set the time limit if we're not running a tutorial.
                {
                    co = LimitTime(120);         // Assign a new coroutine instance
                    StartCoroutine(co);          // And start the countdown
                }
            }
            timeVideoStarted = DateTime.Now.Ticks;

            //correctFrame = load from txt now?
        }
    }

    private IEnumerator LimitTime(int time)
    {
        //Debug.Log("Starting 2-minute countdown");
        yield return new WaitForSeconds(time);
        //Debug.Log("Time's up!");
        TimeUp();
    }

    /// <summary>
    /// Logs the time this function is called and which video frame was selected as answer (passed as argument).
    /// </summary>
    public void LogDataOnAnswerConfirm()
    {
        try
        {
            StopCoroutine(co);       // Stop our previous instance of the time limit coroutine.
        }
        catch (NullReferenceException)
        {
            //Debug.Log("co empty");
        }

        if (!answerGiven)   // Only record data if this is the first time during this video that the participant has clicked this button!
        {
            Debug.Log(vp.frame);
            if (!tutorialOn)        // Only log answers if the tutorial was not on.
            {
                // Get the frame that was picked as the answer from the video time slider:
                long answeredFrame = vp.frame;

                // Record the time the answer was given on and calculate how long was taken:
                timeAnswerConfirmed = DateTime.Now.Ticks;
                long timeTaken = (timeAnswerConfirmed - timeVideoStarted) / 10000;          // One tick is 1/10,000 of a millisecond.
                                                                                            //Debug.Log("Time taken: " + timeTaken + " ms");

                // Get the video name:
                string videoID = Path.GetFileNameWithoutExtension(vp.url);
                //Debug.Log("Video answered: " + videoID);

                // Now that all "volatile" data has been captured, it is safe to stop the video:
                vp.Stop();

                // Compare the answered frame to the correct frame (Negative means answered frame was too early, positive means too late):
                //Debug.Log("Answered frame: frame " + answeredFrame);
                //Debug.Log("Distance to correct answer: " + (answeredFrame - correctFrame) + " frames");

                //Debug.Log("Times correct answer was passed: " + numberOfTimesCorrectAnswerPassed + " times");

                using (StreamWriter w = File.AppendText(Application.dataPath + "/Results/participant " + participantID + ".csv"))
                {
                    w.WriteLine(videoID + "," + timeTaken + "," + answeredFrame + "," + (answeredFrame - correctFrame) + "," + numberOfTimesCorrectAnswerPassed);
                }

                // Mark that the user has given an answer for this video and disable the button:
                answerGiven = true;
                LockControls();
            }
            else                                    // If the tutorial WAS on
            {
                // Check the given answer
                if (currentVideoID == 3)
                {
                    if (Math.Abs(vp.frame - long.Parse(correctFrames[currentVideoID-1].Split(',')[0])) > tut3Tolerance && Math.Abs(vp.frame - long.Parse(correctFrames[currentVideoID-1].Split(',')[1])) > tut3Tolerance && Math.Abs(vp.frame - long.Parse(correctFrames[currentVideoID - 1].Split(',')[2])) > tut3Tolerance)
                    {
                        try { StopCoroutine(wrongAnswerCo); }
                        catch (Exception) { }
                        wrongAnswerCo = ShowWrongAnswer();
                        StartCoroutine(wrongAnswerCo);
                    }
                    else
                    {
                        tutorialCompleted = true;
                    }
                }
                else
                {
                    if (Math.Abs(vp.frame - correctFrame) > tut5Tolerance)
                    {
                        try { StopCoroutine(wrongAnswerCo); }
                        catch (Exception) { }
                        wrongAnswerCo = ShowWrongAnswer();
                        StartCoroutine(wrongAnswerCo);
                    }
                    else
                    {
                        tutorialCompleted = true;
                    }
                }

                // Disable the tutorial and its associated images now.
                if (tutorialCompleted) {
                    CMPTutImage.SetActive(false);
                    ERPTutImage.SetActive(false);
                    tutorialOn = false;

                    vp.Stop();

                    LockControls();
                }
            }
        }
        else
        {
            //Debug.Log("No cheating, alright?");
        }
    }

    private void LockControls()
    {
        answerButton.interactable = false;  // Disable the answer button
        foreach (Button b in interfaceButtons)
        {
            b.interactable = false;         // And all other interface buttons
        }
        GetComponent<InputHandler>().ResetAfterExperimentAnswer();      // And finish by running the function that displays the thank-you screen
    }

    // Show the "wrong answer" image and then hide it again.
    private IEnumerator ShowWrongAnswer()  
    {
        WrongAnswerImage.SetActive(true);
        yield return new WaitForSeconds(2);
        WrongAnswerImage.SetActive(false);
    }

    public void TimeUp()
    {
        // The last frame that the player was on will be considered the answer:
        long answeredFrame = vp.frame;

        // Record the time the answer was given on and calculate how long was taken:
        long timeTaken = 120000;          // 120,000 milliseconds, or 2 minutes.

        // Get the video name:
        string videoID = Path.GetFileNameWithoutExtension(vp.url);
        //Debug.Log("Video answered: " + videoID);

        // Now that all "volatile" data has been captured, it is safe to stop the video:
        vp.Stop();

        // Compare the answered frame to the correct frame (Negative means answered frame was too early, positive means too late):
        //Debug.Log("Answered frame: frame " + answeredFrame);
        //Debug.Log("Distance to correct answer: " + (answeredFrame - correctFrame) + " frames");

        //Debug.Log("Times correct answer was passed: " + numberOfTimesCorrectAnswerPassed + " times");

        using (StreamWriter w = File.AppendText(Application.dataPath+"/Results/participant "+participantID+".csv"))
        {
            w.WriteLine(videoID + "," + timeTaken + "," + answeredFrame + "," + (answeredFrame - correctFrame) + "," + numberOfTimesCorrectAnswerPassed);
        }

        // Mark that the user has given an answer for this video and disable the button:
        answerGiven = true;
        answerButton.interactable = false;  // Disable the answer button
        foreach (Button b in interfaceButtons)
        {
            b.interactable = false;         // And all other interface buttons
        }

        GetComponent<InputHandler>().ResetAfterTimeUp();        // Display image about timing out and re-enable Open... button.
    }
}
