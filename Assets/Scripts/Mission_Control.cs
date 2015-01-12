using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;


public class Mission_Control : MonoBehaviour {

    public Ball ball;
    MissionControlData m_data;
    int cntFramesDelay = 5;
    bool init = false;
	// Use this for initialization
	void Start () {
        m_data = new MissionControlData();
        MemoryStream ms = StreamSaver.stream;
        PlayerPrefs.SetInt("lives", 5);
        if(ms != null)
        {
            BinaryFormatter bf = new BinaryFormatter();
            ms.Seek(0, SeekOrigin.Begin);
            m_data = (MissionControlData)bf.Deserialize(ms);
            ball.transform.position = m_data.ballPosition;
            ball.DownKey();
        }
        else
        {
            m_data.ballPosition = ball.transform.position;
            PlayerPrefs.DeleteAll();
            PlayerPrefs.SetInt("lives", 5);
        }
	}

	// Update is called once per frame
	void Update () {

        if (cntFramesDelay > 0)
        {
            cntFramesDelay--;
        }
        else
        {
            if (!init)
            {
                init = true;
                Transform[] starList = GameObject.Find("Stars").GetComponentsInChildren<Transform>();

                List<Star> stars = new List<Star>();
                foreach (Transform item in starList)
                {
                    GameObject obj = item.gameObject;
                    if (obj.ToString().Contains("start_level"))
                    {
                        stars.Add(obj.gameObject.GetComponent<Star>());
                    }
                }
                foreach (Star star in stars)
                {
                    star.starStart += OnStarStarted;
                }
            }
        }
	}
    void OnStarStarted(Star star)
    {
        m_data.ballPosition = ball.transform.position;
        ball.DownKey();
        print(String.Format("old ball pos {0}", m_data.ballPosition));
        BinaryFormatter bf = new BinaryFormatter();
        MemoryStream ms = new MemoryStream();
        bf.Serialize(ms, m_data);
        StreamSaver.stream = ms;
        Application.LoadLevel(star.missionName);
    }
}

[Serializable]
public class MissionControlData
{
    float ballPositionY;
    float ballPositionX;
    float ballPositionZ;

    public Vector3 ballPosition
    {
        get
        {
            return new Vector3(ballPositionX, ballPositionY, ballPositionZ);
        }
        set
        {
            ballPositionX = value.x;
            ballPositionY = value.y;
            ballPositionZ = value.z;
        }
    }
}

static class StreamSaver
{
    static public MemoryStream stream;
}
