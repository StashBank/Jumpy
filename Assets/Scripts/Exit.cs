using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum ExitStateTytpes { SHOW,HIDE}
interface ExitState
{
    void OnTrigger();
    void UpdateParameters();
}
class ExitStateHide : ExitState
{
    Exit m_context;
    public ExitStateHide(Exit context)
    {
        m_context = context;
    }
    public void OnTrigger()
    {
        return;
    }
    public void UpdateParameters()
    {
        m_context.renderer.enabled = false;
    }
    public override string ToString()
    {
        return "HIDE";
    }
}
class ExitStateShow : ExitState
{
    Exit m_context;
    public ExitStateShow(Exit context)
    {
        m_context = context;
    }
    public void OnTrigger()
    {
        m_context.ExitFormLevel();
    }
    public void UpdateParameters()
    {
        m_context.renderer.enabled = true;
    }
    public override string ToString()
    {
        return "SHOW";
    }
}
public delegate void LevelExit(Exit sender);
public class Exit : MonoBehaviour {

    public event LevelExit levelExit = null;

    Dictionary<ExitStateTytpes, ExitState> m_States;
    ExitState m_State;
	// Use this for initialization
	void Start () {
        m_States = new Dictionary<ExitStateTytpes, ExitState>(2);
        m_States[ExitStateTytpes.HIDE] = new ExitStateHide(this);
        m_States[ExitStateTytpes.SHOW] = new ExitStateShow(this);
        SetState(ExitStateTytpes.HIDE);
	}
	
	// Update is called once per frame
	void Update () {
	
	}
    void OnTriggerEnter2D(Collider2D inCollider)
    {
        m_State.OnTrigger();
    }

    public void SetState(ExitStateTytpes type)
    {
        if (m_States.ContainsKey(type))
        {
            m_State = m_States[type];
            m_State.UpdateParameters();
        }
    }

    public void ExitFormLevel()
    {
        if(levelExit != null)
            levelExit(this);
        Destroy(this.gameObject);
    }
}
