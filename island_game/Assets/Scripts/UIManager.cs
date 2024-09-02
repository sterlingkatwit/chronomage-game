using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.Unity.VisualStudio.Editor;
using TMPro;
using Unity.PlasticSCM.Editor.WebApi;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Image = UnityEngine.UI.Image;

public class UIManager : MonoBehaviour
{

    public Image uiBuildingSquare;
    public TextMeshProUGUI uiBuildingText;
    public string currentBuilding;
    public TextMeshProUGUI timerText;

    private Color[] buildingColors = {new Color(0.75f, 0.8f, 0.85f), new Color(0.56f, 0.93f, 0.56f), Color.blue, Color.red};
    private int uiCurrent = 0;

    public class UIObject{
        public string name;
        public Color color;
    }


    public List<UIObject> objectsList = new List<UIObject>();

    


    // This class will need to be modified for better sprites.

    void Start(){

        objectsList.Add(addItem("Drill", buildingColors[0]));
        objectsList.Add(addItem("Recycler", buildingColors[1]));
        objectsList.Add(addItem("Reclaimer", buildingColors[2]));
        objectsList.Add(addItem("The Cure", buildingColors[3]));



        currentBuilding = DisplaySelectedObject(uiCurrent);

    }

    void Update(){

        

        if (Input.GetKeyDown(KeyCode.Q)){
            uiCurrent++;
            if(uiCurrent >= objectsList.Count){
                uiCurrent = 0;
            }
            currentBuilding = DisplaySelectedObject(uiCurrent);
            Debug.Log("CURRENT BUILDING: " + currentBuilding);
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            uiCurrent--;
            if (uiCurrent < 0)
            {
                uiCurrent = 3;
            }
            currentBuilding = DisplaySelectedObject(uiCurrent);
            Debug.Log("CURRENT BUILDING: " + currentBuilding);
        }

        UpdateTimerText();


    }

    public void UpdateTimerText()
    {
        if (Timer.TimerStatus())
        {
            timerText.text = FormatTime(Timer.GetCurrentTime());
        }
    }

    string FormatTime(float time)
    {
        // Convert time to minutes, seconds, and milliseconds
        int minutes = Mathf.FloorToInt(time / 60);
        int seconds = Mathf.FloorToInt(time % 60);
        int milliseconds = Mathf.FloorToInt((time * 1000) % 1000);

        // Return the formatted time string
        return string.Format("{0:00}:{1:00}:{2:000}", minutes, seconds, milliseconds);
    }


    public string DisplaySelectedObject(int index){
        UIObject selectedObject = objectsList[index];
        uiBuildingSquare.color = selectedObject.color;
        uiBuildingText.text = selectedObject.name;
        return selectedObject.name;
    }


    private UIObject addItem(string name, Color color){
        UIObject item = new UIObject();
        item.name = name;
        item.color = color;
        return item;
    }

}
