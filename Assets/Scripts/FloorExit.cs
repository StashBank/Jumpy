using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FloorExit : MonoBehaviour {

    public Ball m_ball;
	// Use this for initialization
	void Start () {
        if(m_ball == null)
            m_ball = GameObject.Find("Ball").GetComponent<Ball>();
        m_ball.GameOver += OnGameOver;
	}
	
	// Update is called once per frame
	void Update () {
	
	}
    void OnTriggerEnter2D(Collider2D inCollider)
    {
        OnGameOver();        
    }

    void OnGameOver()
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
