using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class mainMenu : MonoBehaviour
{
    GameObject mainMenuObj;
    public DataTransfer dataTransfer;
    public TMP_InputField nameInputField;
    public GameObject resultWindow;

    // Start is called before the first frame update
    void Start()
    {
        nameInputField = GameObject.Find("Player Name").GetComponent<TMP_InputField>();
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKey(KeyCode.Escape) && resultWindow.activeInHierarchy)
        {
            resultWindow.SetActive(false);
        }
    }

    public void StartGame()
    {
        dataTransfer.playerName = nameInputField.text;
        SceneManager.LoadScene("GameScene", LoadSceneMode.Single);
    }



    public void ViewScores()
    {
        resultWindow.SetActive(true);
    }

    public void Quit()
    {
        Application.Quit();
    }
}
