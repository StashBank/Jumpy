using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Levels : MonoBehaviour
{
    struct MoveInfo // Структура для хранения информации о передвижении игрока
    {
        public MoveInfo(Vector2 v, float f) { dist = v; speed = f; }
        public Vector2 dist; // Направление движения
        public float speed; // Растояние которое пройдет игрок за один фрейм (кадр)
    }

    Queue<GameObject> m_levels = new Queue<GameObject>();
    Queue<Heart> hearts = new Queue<Heart>();
    List<Key> m_keys = new List<Key>();
    List<MonoBehaviour> m_shelfs = new List<MonoBehaviour>();
    public Ball Ball;
    Ball m_ball;
    GameObject m_currentLevel;
    Exit m_exit;
    float m_H = 0.0f;
    float m_W = 0.0f;

    Queue<MoveInfo> m_MoveInfo = new Queue<MoveInfo>();
    public float Speed = 0.5f;
    bool toNextLevel = false;

    public GameObject backGround;
    Queue<MoveInfo> m_backGroundMoveInfo = new Queue<MoveInfo>();
    int levelNum = 0;
    float _timer = 0;
    const int SECONDS_IN_HOUR = 3600;
    const int SECONDS_IN_MINUTE = 60;
    int Points = 0;
    void OnKeyGet(Key sender)
    {
        Points++;
        m_keys.Remove(sender);
        if (m_keys.Count == 0)
        {
            m_exit.SetState(ExitStateTytpes.SHOW);
        }

    }
    void OnLevelExit(Exit sender)
    {
        toNextLevel = true;
        m_ball.SetState(Ball.BallStateType.HIDE);

        //int cnt = (int)(m_W / Speed) - 60;
        int cnt = (int)(m_W / Speed);
        //float dif = m_H % Speed;
        float dif = m_W % Speed;

        while (cnt > 0)
        {
            m_MoveInfo.Enqueue(new MoveInfo(new Vector2(-1, 0), Speed));
            cnt--;
        }
        m_MoveInfo.Enqueue(new MoveInfo(new Vector2(-1, 0), dif));

        if (backGround != null)
        {
            float speed = Speed * 0.45f;
            cnt = (int)(m_W / speed);
            dif = m_W % speed;
            while (cnt > 0)
            {
                m_backGroundMoveInfo.Enqueue(new MoveInfo(new Vector2(-1, 0), speed));
                cnt--;
            }
            m_backGroundMoveInfo.Enqueue(new MoveInfo(new Vector2(-1, 0), dif));
        }
        /*foreach (GameObject item in m_levels)
        {
            item.transform.Translate(new Vector3(0, -m_H, 0));
        }
        Destroy(m_currentLevel.gameObject);
        NextLevel();*/
    }

    
    void OnGUI()
    {
        string levelCaption = "Level " + levelNum;
        GUIStyle style = new GUIStyle();
        style.border = new RectOffset(0, 0, 0, 0);
        style.fontSize = 24;
        GUI.Box(new Rect(420,20,60,30),levelCaption,style);

        _timer += Time.deltaTime;
        int timer = (int)_timer;
        //System.DateTime time = new System.DateTime(0, 0, 0, 0, 0, timer,0);
        int hours = (int)timer / SECONDS_IN_HOUR;
        timer %= SECONDS_IN_HOUR;
        int minutes = (int)timer / SECONDS_IN_MINUTE;
        timer %= SECONDS_IN_MINUTE;
        int seconds = (int)timer;
        string displayTime = string.Format("{0}:{1}:{2}", 
            (hours > 9) ? hours.ToString() : "0" + hours.ToString(), 
            (minutes > 9) ? minutes.ToString() : "0" + minutes.ToString(),
            (seconds > 9) ? seconds.ToString() : "0" + seconds.ToString()
            );
        GUI.Box(new Rect(120, 20, 60, 30), displayTime, style);

        GUI.Box(new Rect(650, 20, 60, 30), Points.ToString(), style);
    }

    // Use this for initialization
    void Start()
    {
        if (backGround == null)
        {
            backGround = GameObject.Find("background1");
        }
        Camera cam = GameObject.Find("MainCamera").GetComponent<Camera>();
        m_H = cam.ScreenToWorldPoint(new Vector2(Screen.width, Screen.height)).y * 2;
        //m_W = cam.ScreenToWorldPoint(new Vector2(Screen.width, Screen.height)).x * 2;
        //m_W -= (m_W % GameInfo.cellSide);
        m_W = 10* GameInfo.cellSide;
        if (Ball != null)
            m_ball = Ball;
        else
            m_ball = GameObject.Find("Ball").GetComponent<Ball>();

        PlayerPrefs.SetFloat("levelPositionX", m_ball.transform.position.x);
        PlayerPrefs.SetFloat("levelPositionY", m_ball.transform.position.y);
        Transform[] levels = GameObject.Find("Levels").GetComponentsInChildren<Transform>();
        foreach (Transform item in levels)
        {
            GameObject obj = item.gameObject;
            if (obj.ToString().IndexOf("Level_") >= 0)
            {
                m_levels.Enqueue(obj);
            }
        }
        GameObject life = GameObject.Find("Life");
        if (life != null)
        {
            List<Transform> heartList = new List<Transform>(life.GetComponentsInChildren<Transform>());
            heartList.Reverse();
            foreach (Transform item in heartList)
            {
                GameObject obj = item.gameObject;
                if (obj.ToString().Contains("heart"))
                {
                    hearts.Enqueue(obj.gameObject.GetComponent<Heart>());
                }
            }
            foreach (Heart heart in hearts)
            {
                heart.GetComponent<Renderer>().enabled = true;
            }
        }
        NextLevel();
    }

    void temp_tryExit(FloorExit sender)
    {
        throw new System.NotImplementedException();
    }

    void NextLevel()
    {
        if (m_levels.Count < 1) // Win mission
            return;
        m_currentLevel = m_levels.Dequeue();
        m_shelfs.Clear();
        levelNum++;
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
                PlayerPrefs.SetFloat("levelPositionX", item.position.x);
                PlayerPrefs.SetFloat("levelPositionY", item.position.y);
            }
        }
        m_ball.Shelfs = m_shelfs;
        m_ball.SetState(Ball.BallStateType.SHOW);

    }
    // Update is called once per frame
    void Update()
    {
        if (toNextLevel)
        {
            if (m_backGroundMoveInfo.Count > 0)
            {
                MoveInfo move = m_backGroundMoveInfo.Dequeue();
                backGround.transform.Translate(move.dist * move.speed);
            }

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
        if (PlayerPrefs.GetInt("lives") < hearts.Count)
        {
            hearts.Peek().GetComponent<Renderer>().enabled = false;
            hearts.Dequeue();
        }

    }
}
