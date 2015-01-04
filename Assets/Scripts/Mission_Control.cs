using UnityEngine;
using System.Collections;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

public class Mission_Control : MonoBehaviour {

    public string missionName;
    public Ball ball;
    MissionControlData m_data;
	// Use this for initialization
	void Start () {
        m_data = new MissionControlData();

        MemoryStream ms = StreamSaver.stream;
        if(ms != null)
        {
            BinaryFormatter bf = new BinaryFormatter();
            
            ms.Seek(0, SeekOrigin.Begin);
            m_data = (MissionControlData)bf.Deserialize(ms);
            print(String.Format("new ball pos {0}", m_data.ballPosition));
            ball.transform.position = m_data.ballPosition;
            ball.DownKey();
        }
        else
        {
            m_data.ballPosition = ball.transform.position;
        }        
	}

	// Update is called once per frame
	void Update () {
	
	}
    void OnTriggerEnter2D(Collider2D collaider)
    {
        m_data.ballPosition = ball.transform.position;
        ball.DownKey();
        print(String.Format("old ball pos {0}", m_data.ballPosition));
        BinaryFormatter bf = new BinaryFormatter();
        MemoryStream ms = new MemoryStream();
        bf.Serialize(ms, m_data);
        StreamSaver.stream = ms;

        Application.LoadLevel(missionName);
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
