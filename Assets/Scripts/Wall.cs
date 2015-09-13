using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
public class Wall : MonoBehaviour {
	#region WallTypes
	public enum WallType {
		Standart,
		OnlyTwoEntry
	}
	#endregion
	#region states
	#region BaseState
	abstract class WallTypeState {
		protected Wall m_context;
		protected WallType m_type = WallType.Standart;
		protected Ball m_ball;
        public virtual void Reset()
        {
            if (m_context.animator != null)
            {
                m_context.animator.SetTrigger("Reset");
            }
        }
		public virtual void SetParam(Wall context, Ball ball) {
			this.m_context = context;
			this.m_ball = ball;
		}
		public virtual void OnTriggerEnter2D(Collider2D inCollider) {
			m_ball.OnWall();
		}
	}
	#endregion
	#region StandartWall
	class StandartState : WallTypeState {
	}
	#endregion
	#region OnlyTwoEntry
	class OnlyTwoEntryState : WallTypeState {
		int cnt = 2;
		public override void SetParam(Wall context, Ball ball) {
			base.SetParam(context, ball);
			context.gameObject.GetComponent<Collider2D>().enabled = false;
		}
		public override void OnTriggerEnter2D(Collider2D inCollider) {
			if (cnt > 0) {
				cnt--;
			} else {
				m_context.gameObject.GetComponent<Collider2D>().enabled = true;
				base.OnTriggerEnter2D(inCollider);
			}
            m_context.animator.SetTrigger("next");
        }
        public override void Reset()
        {
            base.Reset();
            cnt = 2;
            m_context.gameObject.GetComponent<Collider2D>().enabled = false;
        }
    }
	#endregion
	#endregion
	public Ball ball;
	public WallType wallType;
	WallTypeState m_state;
	// Use this for initialization
	#region States init
	Dictionary<WallType, Type> m_states = new Dictionary<WallType, Type> {
		{WallType.Standart, typeof(StandartState)},
		{WallType.OnlyTwoEntry, typeof(OnlyTwoEntryState)}
	};
	#endregion
	void Start () {
		if (ball == null) {
			ball = GameObject.Find("Ball").GetComponent<Ball>();
		}
		Type type = null;
		if (m_states.ContainsKey(wallType)) {
			type = m_states[wallType];
		} else {
			type = typeof(StandartState);
		}
		m_state = System.Activator.CreateInstance(type) as WallTypeState;
		m_state.SetParam(this, ball);
	}
    public virtual void Reset()
    {
        m_state.Reset();
    }

    // Update is called once per frame
    void Update () {
	
	}

	void OnTriggerEnter2D(Collider2D collaider) {
		m_state.OnTriggerEnter2D(collaider);
	}

    Animator animator
    {
        get
        {
            return this.ToString() == "null" ? null : GetComponent<Animator>();
        }
    }
}
