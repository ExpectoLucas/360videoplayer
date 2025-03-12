using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class QuestionnaireController : MonoBehaviour
{
    public bool testingVideo = false;
    public GameObject videoPlayerControlPane;

    public GameObject[] questionnaire1Pages;
    public GameObject[] questionnaire2Pages;
    public GameObject[] experimentStartPages;
    public GameObject backPanel;
    public GameObject nextPanel;
    public GameObject warningText;

    public GameObject gridPlane;
    public GameObject videoSphereEAC;

    public ExperimentLoggerNew experimentLogger;

    private GameObject[] questionnairePages;
    private int currentPage;
    private int currentSet;

    private GameObject containerObject;
    private InputHandlerNew inputHandler;

    private SFTP sftpClient;

    // Start is called before the first frame update
    void Start()
    {
        // Prepare container object
        containerObject = GetComponent<Transform>().transform.Find("Container").gameObject;       // Transform component houses hierarchy structure, so this searches only children.
        inputHandler = videoPlayerControlPane.GetComponent<InputHandlerNew>();

        // Prepare FTP client
        //sftpClient = new SFTP(@"gemini.science.uu.nl", "6493769", "6Dzeta&7Eta");
        sftpClient = new SFTP(@"test.rebex.net", "demo", "password");
        Debug.Log("Doing SFTP now");
        sftpClient.ListFiles();
        sftpClient.DownloadFile("readme.txt");

        // Prepare other stuff
        if (!testingVideo)
        {
            gridPlane.SetActive(true);
            videoSphereEAC.SetActive(false);

            currentSet = 0;
            backPanel.SetActive(false);
            nextPanel.SetActive(false);
            foreach (GameObject g in questionnaire1Pages)
            {
                g.SetActive(false);
            }
            foreach (GameObject g in questionnaire2Pages)
            {
                g.SetActive(false);
            }
            foreach (GameObject g in experimentStartPages)
            {
                g.SetActive(false);
            }

            StartQuestionnaire1();
        } 
        else
        {
            StartPreExperiment();
            //ResetAndHideQuestionnaire();
            //inputHandler.PlayButton();
        }

        
    }

    public void StartQuestionnaire1()
    {
        questionnairePages = questionnaire1Pages;
        currentSet = 1;
        StartQuestionnaire();
    }

    public void StartQuestionnaire2()
    {
        questionnairePages = questionnaire2Pages;
        currentSet = 2;
        StartQuestionnaire();
    }

    public void StartQuestionnaire()
    {
        backPanel.SetActive(true);
        nextPanel.SetActive(currentSet!=3);
        warningText.SetActive(false);

        currentPage = 0;
        foreach (GameObject g in questionnairePages)
        {
            g.SetActive(false);
        }
        questionnairePages[currentPage].SetActive(true);

        //if (currentSet != 1)
        //{
            Debug.Log("Current question: " + questionnairePages[currentPage].GetComponentInChildren<Text>().text); // Finds first object with Text component. Should be the question.
        //}
    }

    public void StartPreExperiment()
    {
        questionnairePages = experimentStartPages;
        currentSet = 3;
        nextPanel.SetActive(false);
        StartQuestionnaire();
    }

    public void NextPage()
    {
        // First check the given answers and collect them:
        ToggleGroup[] toggleGroups = questionnairePages[currentPage].GetComponentsInChildren<ToggleGroup>();
        if (toggleGroups.Length > 0)
        // If there is at least one toggle group, then this page was a likert scale or exclusive selection list.
        // Gather all toggle groups and get the active toggles from each.
        {
            foreach (ToggleGroup tg in toggleGroups)
            {
                // Check if necessary answers given.
                if (!tg.AnyTogglesOn())
                {
                    // Necessary answers were not given: display warning and don't proceed.
                    warningText.SetActive(true);
                    return;
                }
                else
                {
                    // Necessary answers were given: collect answers.
                    //IEnumerator<Toggle> questionToggles = tg.ActiveToggles().GetEnumerator();
                    foreach (Toggle currentToggle in tg.ActiveToggles())
                    {

                        //questionToggles.MoveNext();

                        //Toggle currentToggle = questionToggles.Current;
                        Text toggleText = currentToggle.GetComponentInChildren<Text>();
                        Debug.Log(tg.GetComponent<Text>().text + " : " + toggleText.text.Replace("\r", "").Replace("\n", " "));
                    }
                    warningText.SetActive(false);
                }
            }
        } 
        else 
        // If there are no toggle groups, then this page was just a free checklist.
        // Gather all toggles and return the text of the ones that are active.
        {
            foreach (Toggle currentToggle in questionnairePages[currentPage].GetComponentsInChildren<Toggle>())
            {
                if (currentToggle.isOn)
                {
                    Text toggleText = currentToggle.GetComponentInChildren<Text>();
                    Debug.Log(toggleText.text.Replace("\r", "").Replace("\n", " "));
                }
            }
        }


        // Now go to next page:
        //if (currentSet != 3)
        //{
            // First, check page index against number of pages
            if (currentPage + 1 < questionnairePages.Length)
            {
                // Adjust active states of pages
                questionnairePages[currentPage].SetActive(false);

                currentPage += 1;

                questionnairePages[currentPage].SetActive(true);
                //if (currentSet != 1)
                //{
                Debug.Log("Current question: " + questionnairePages[currentPage].GetComponentInChildren<Text>().text);
                //}
            }
            else if (currentPage + 1 >= questionnairePages.Length)
            {
                // We've reached the end
                questionnairePages[currentPage].SetActive(false);
                nextPanel.SetActive(false);

                if (currentSet == 1)
                {
                    StartQuestionnaire2();
                }
                else if (currentSet == 2)
                {
                    StartPreExperiment();
                }
                else
                {
                    // Send signal to finish questionnaire.
                    Debug.Log("That's the end of the questionnaire.");
                    backPanel.SetActive(false);
                    nextPanel.SetActive(false);
                }
            }
        //}
    }

    public void StartExperiment()
    {
        NextPage(); // Advance to the next page in advance for when we return
        PauseAndHideQuestionnaire();

        //Use saved seed to pick first video
        string videoName = ExperimentLoggerNew.GetNextRandomVideo();
        Debug.Log("Picked video:" + videoName);

        //Set up first video for playback
        //inputHandler.OpenFile(videoName);

        // (first actually set and load video)
        inputHandler.PlayButton();
    }

    public void PauseAndHideQuestionnaire()
    {
        // Simply hide the questionnaire, effectively storing its current state for later
        containerObject.gameObject.SetActive(false);

        // Hide grid, show video.
        gridPlane.SetActive(false);
        videoSphereEAC.SetActive(true);
    }

    public void ResumeQuestionnaire()
    {
        // Restore all stored questionnaire element states
        containerObject.gameObject.SetActive(true);

        // And bring back the grid
        gridPlane.SetActive(true);
        videoSphereEAC.SetActive(false);
    }

    public void ResetAndHideQuestionnaire()
    {
        /*foreach (GameObject go in GetComponentsInChildren<GameObject>())
        {
            go.SetActive(false);
        }*/
        foreach (GameObject g in questionnaire1Pages)
        {
            g.SetActive(false);
        }
        foreach (GameObject g in questionnaire2Pages)
        {
            g.SetActive(false);
        }
        foreach (GameObject g in experimentStartPages)
        {
            g.SetActive(false);
        }
        backPanel.SetActive(false);
        nextPanel.SetActive(false);
    }

    public void ShowQuestionnaire()
    {
        backPanel.SetActive(true);
        //preVideoPlane.SetActive(true);
        //And activate more components
    }

    public void NotifyEnded()
    {
        Debug.Log("END");
        ResumeQuestionnaire();
        
    }

    public void Upload()
    {
        
    }


/* Uploading results.
 * Generatre long, random string as participant ID and filename
 * Make PHP/FTP request to server
 * 
 */
}