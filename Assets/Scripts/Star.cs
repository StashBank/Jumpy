using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum StarStateTypes { SHOW, HIDE }

public delegate void StarEnter(Star sender);
public delegate void StarEntered();

abstract class StarState
{
    public event StarEntered starEnter = delegate{};
    public abstract void OnTrigger();
    protected void SendStarStartEvent()
    {
        starEnter();
    }
}

class StarStateHide : StarState
{
    Star m_context;

    public StarStateHide(Star context)
    {
        m_context = context;
        m_context.GetComponent<Renderer>().enabled = false;
    }
    public override void OnTrigger()
    {
        return;
    }
    public override string ToString()
    {
        return "HIDE";
    }
}

class StarStateShow : StarState
{
    Star m_context;
    public StarStateShow(Star context)
    {
        m_context = context;
        m_context.GetComponent<Renderer>().enabled = true;
    }
    public override void OnTrigger()
    {
        base.SendStarStartEvent();
        return;
    }
    public override string ToString()
    {
        return "SHOW";
    }
}

public class Star : MonoBehaviour {

    public event StarEnter starStart = delegate { };
    StarState m_State;
    public string missionName;
	// Use this for initialization
	void Start () {
        int flg = 0;
        try{flg = PlayerPrefs.GetInt(missionName);}catch{}
        if (flg == 1)
        {
            m_State = new StarStateHide(this);
        }
        else
        {
            m_State = new StarStateShow(this);
            m_State.starEnter += delegate() { 
                this.starStart(this);
            };
        }
	}
	public void Log (object msg)
    {
        print(msg);
    }
	// Update is called once per frame
	void Update () {
	
	}
    void OnTriggerEnter2D(Collider2D inCollider)
    {
        m_State.OnTrigger();
    }
}
