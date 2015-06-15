using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FloorExit : MonoBehaviour {

    public Ball m_ball;
	// Use this for initialization
	void Start () {
        /*
        Transform[] currentLevelTransforms = m_currentLevel.GetComponentsInChildren<Transform>();
        foreach (Transform item in currentLevelTransforms)
        {
            }
            if (item.ToString().IndexOf("StartBallPosition") >= 0)
            {
                m_ball.transform.position = item.position;
            }
        }*/
	}
	
	// Update is called once per frame
	void Update () {
	
	}
    void OnTriggerEnter2D(Collider2D inCollider)
    {
        if (PlayerPrefs.GetInt("lives") > 0)
        {
            PlayerPrefs.SetInt("lives", PlayerPrefs.GetInt("lives") - 1);

            m_ball.transform.position = new Vector2(PlayerPrefs.GetFloat("levelPositionX"), PlayerPrefs.GetFloat("levelPositionY"));
        }
        else
        {
            Application.LoadLevel("Mission_Menu");
        }
        
    }
}
