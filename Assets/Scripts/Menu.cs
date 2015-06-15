using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Menu : MonoBehaviour {

	// Use this for initialization
	void Start () {
        PlayerPrefs.SetInt("lives", 5);
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            Application.Quit();
        }
	}
    public void StartGame()
    {
        Application.LoadLevel("Mission_Menu");
    }
    public void ExitGame()
    {
        Application.Quit();
    }
}
