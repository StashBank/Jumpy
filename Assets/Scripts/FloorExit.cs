using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

public class FloorExit : MonoBehaviour {
    public delegate void ResetLevelDelegate();
    static public event ResetLevelDelegate ResetLevel = delegate () { };
    public Ball m_ball;
    public GameObject gameOverText;
    bool m_isSpawn = false;
	// Use this for initialization
	void Start () {
        if(m_ball == null)
            m_ball = GameObject.Find("Ball").GetComponent<Ball>();
        m_ball.GameOver += OnGameOver;
	}
	
	// Update is called once per frame
	void Update () {
	    if (m_isSpawn)
        {
            m_isSpawn = false;
            gameOverText.SetActive(false);
            m_ball.SetState(Ball.BallStateType.SHOW);
        }
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
            m_ball.SetState(Ball.BallStateType.HIDE);
            gameOverText.SetActive(true);
            ResetLevel();
            new Thread(new ThreadStart(Spawn)).Start();
        }
        else
        {
            Application.LoadLevel("Mission_Menu");
        }
    }
    void Spawn() {
        Thread.Sleep(1000);
        m_isSpawn = true;
    }
}
