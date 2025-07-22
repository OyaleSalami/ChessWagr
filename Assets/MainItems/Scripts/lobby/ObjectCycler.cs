using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ObjectCycler : MonoBehaviour
{
    public static ObjectCycler instance;

    public GameObject objects,EULA; // List of 5 objects to cycle through

    private float currentIndex,Terms = 0;    // Current index in the objects list

    private void Awake()
    {
        instance = this;
        currentIndex = PlayerPrefs.GetFloat("Tutorial");
        Terms = PlayerPrefs.GetFloat("Terms");

        if (Terms == 0)
        {
            EULA.SetActive(true);
        }
        else if (Terms == 1)
        {
            EULA.SetActive(false);
        }
    }


    public void CheckTutorial()
    {
        if(currentIndex == 0)
        {
            objects.SetActive(true);
        }else if(currentIndex == 1)
        {
            objects.SetActive(false);
        }
    }


    // Save the index of the last active object (you can modify this to save to PlayerPrefs or database)
    public void SaveLastActiveObject()
    {
        currentIndex = 1;
        objects.SetActive(false);
        PlayerPrefs.SetFloat("Tutorial", currentIndex);
    }
    public void SaveLastActiveTerms()
    {
        Terms = 1;
        EULA.SetActive(false);
        PlayerPrefs.SetFloat("Terms", Terms);
    }
}
