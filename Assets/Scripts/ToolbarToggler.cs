using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToolbarToggler : MonoBehaviour
{
    public GameObject sliderPanel;
    public GameObject buttonPanel;

    public void MakeVisible()
    {
        //if (Screen.fullScreen)
        //{
            GetComponent<LayoutElement>().ignoreLayout = false;
            sliderPanel.SetActive(true);
            buttonPanel.SetActive(true);
        //}
    }

    public void MakeInvisible()
    {
        //if (Screen.fullScreen)
        //{
            GetComponent<LayoutElement>().ignoreLayout = true;
            sliderPanel.SetActive(false);
            buttonPanel.SetActive(false);
        //}
    }

    public void ForceVisible()
    {
        GetComponent<LayoutElement>().ignoreLayout = false;
        sliderPanel.SetActive(true);
        buttonPanel.SetActive(true);
    }
}
