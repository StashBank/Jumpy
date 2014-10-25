using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Levels : MonoBehaviour
{

    Queue<GameObject> m_levels = new Queue<GameObject>();
    List<Key> m_keys = new List<Key>();
    List<MonoBehaviour> m_shelfs = new List<MonoBehaviour>();
    public Ball Ball;
    Ball m_ball;
    GameObject m_currentLevel;
    Exit m_exit;
    float m_H = 60.0f;
    float m_W = 55.0f;

    Queue<MoveInfo> m_MoveInfo = new Queue<MoveInfo>();
    public float Speed = 0.5f;
    bool toNextLevel = false;

    void OnKeyGet(Key sender)
    {
        m_keys.Remove(sender);
        if (m_keys.Count == 0)
        {
            m_exit.SetState(ExitStateTytpes.SHOW);
        }

    }
    void OnLevelExit(Exit sender)
    {
        toNextLevel = true;
        m_ball.SetState(BallStateType.HIDE);

        int cnt = (int)(m_W / Speed);
        //float dif = m_H % Speed;
        float dif = m_W % Speed;
        while (cnt > 0)
        {
            m_MoveInfo.Enqueue(new MoveInfo(new Vector2(-1, 0), Speed));
            cnt--;
        }
        m_MoveInfo.Enqueue(new MoveInfo(new Vector2(-1, 0), dif));

        /*foreach (GameObject item in m_levels)
        {
            item.transform.Translate(new Vector3(0, -m_H, 0));
        }
        Destroy(m_currentLevel.gameObject);
        NextLevel();*/
    }
    // Use this for initialization
    void Start()
    {
        Camera cam = GameObject.Find("MainCamera").GetComponent<Camera>();
        m_H = cam.ScreenToWorldPoint(new Vector2(Screen.width,Screen.height)).y * 2;
        m_W = cam.ScreenToWorldPoint(new Vector2(Screen.width, Screen.height)).x * 2;
        m_W -= (m_W % GameInfo.cellSide);
        if (Ball != null)
            m_ball = Ball;
        else
            m_ball = GameObject.Find("Ball").GetComponent<Ball>();

        Transform[] levels = GameObject.Find("Levels").GetComponentsInChildren<Transform>();
        foreach (Transform item in levels)
        {
            GameObject obj = item.gameObject;
            if (obj.ToString().IndexOf("Level_") >= 0)
            {
                m_levels.Enqueue(obj);
            }            
        }        
        NextLevel();
    }

    void NextLevel()
    {
        if (m_levels.Count < 1) // Win mission
            return;
        m_currentLevel = m_levels.Dequeue();
        m_shelfs.Clear();
        Transform[] currentLevelTransforms = m_currentLevel.GetComponentsInChildren<Transform>();
        foreach (Transform item in currentLevelTransforms)
        {

            if (item.ToString().IndexOf("Key_") >= 0)
            {
                Key temp = item.gameObject.GetComponent<Key>();
                temp.keyGet += OnKeyGet;
                m_keys.Add(temp);
            }
            if (item.ToString().IndexOf("Ceiling") >= 0)
            {
                Ceiling temp = item.gameObject.GetComponent<Ceiling>();
                m_shelfs.Add(temp);
            }
            if (item.ToString().IndexOf("Shelf_") >= 0)
            {
                Shelf temp = item.gameObject.GetComponent<Shelf>();
                m_shelfs.Add(temp);
            }
            if (item.ToString().IndexOf("Exit") >= 0)
            {
                m_exit = item.gameObject.GetComponent<Exit>();
                m_exit.levelExit += OnLevelExit;
            }
            if (item.ToString().IndexOf("StartBallPosition") >= 0)
            {
                m_ball.transform.position = item.position;
            }
        }
        m_ball.Shelfs = m_shelfs;
        m_ball.SetState(BallStateType.SHOW);
    }
    // Update is called once per frame
    void Update()
    {
        if (toNextLevel) 
        {
            if (m_MoveInfo.Count > 0)
            {
                MoveInfo move = m_MoveInfo.Dequeue();
                m_currentLevel.transform.Translate(move.dist * move.speed);
                foreach (GameObject item in m_levels)
                {
                    item.transform.Translate(move.dist * move.speed);
                }
            }
            else
            {
                Destroy(m_currentLevel.gameObject);
                NextLevel();
                toNextLevel = false;
            }
        }        
    }
}
