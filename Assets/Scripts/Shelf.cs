using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public enum ShelfType { 
    Standart, // +Обычная полочка
    Ice,  // +Сколзящая (перемещает шарик далее по направлению, если не задать другое движение)
    DisappearByMultJumps, // +Исчезает при достижении определенного кол-ва попаданий на полку
    DisappearByOneJumps, // +Исчезает если на полке перейти в состояние прыгов
    Disappear, // Исчезает сразу. пропускает через себя (пустышка)
    Sticky, // Липкая полка (Вертикальная). Нужно продумать поведение шарика
    TowardsOn1CellsLeft, //+Бросает шарик влево на 1 колонку
    TowardsOn1CellsRight, //+Бросает шарик вправо на 1 колонку
    TowardsOn2CellsLeft, //+Бросает шарик влево на 2 колонку
    TowardsOn2CellsRight, //+Бросает шарик вправо на 2 колонку
    ChangeableToTowardsOn1Cells, //+При вертикальных прижках обычная полка. при приге всторону изменяется на полку с типом TowardsOn1CellsLeft|TowardsOn1CellsRight
    DoubleSpike, //Шипы по обеем сторонам полки. Горизонтальная|Вертикальная
    BotomSpike, //Шипы снизу полки (Горизонтальная)
    PullUp,
    Cloud, //?????
    Teleport
}
public class Shelf : MonoBehaviour {

    abstract class ShelfTypeState
    {
        protected Shelf m_context;
        protected ShelfType m_type = ShelfType.Standart;
        protected Ball m_ball;
        public virtual void SetParam(Shelf context, Ball ball)
        {
            this.m_context = context;
            this.m_ball = ball;
        }
        public virtual void OnTriggerEnter2D(Collider2D inCollider)
        {
            
            if (!GetIsOnGround(inCollider))
                m_ball.OnCeiling(m_type);
            else
                m_ball.OnGround(m_type);
        }

        protected bool GetIsOnGround(Collider2D inCollider)
        {
            Vector2 inPos = inCollider.transform.position;
            Vector2 pos = m_context.transform.position;
            return !(pos.y > inPos.y);
        }
    }

    class ShelfTypeStandartState : ShelfTypeState
    {
        
    }

    class ShelfTypeIceState : ShelfTypeState
    {
        public ShelfTypeIceState()
        {
            m_type = ShelfType.Ice;
        }
    }

    class ShelfTypeDisappearByOneJumpState : ShelfTypeState
    {
        protected int count = 1;
        protected  float step = 0.0f;
        private Collider2D inCollider;
        public ShelfTypeDisappearByOneJumpState()
        {
            m_type = ShelfType.DisappearByOneJumps;
        }
        public override void SetParam(Shelf context, Ball ball)
        {
            base.SetParam(context, ball);
            step = m_context.transform.localScale.x / count;
        }
        public override void OnTriggerEnter2D(Collider2D inCollider)
        {
            this.inCollider = inCollider;
            if (GetIsOnGround(inCollider))
            {
                m_ball.BallStateChanged += OnBallStateChanged;
            }
            base.OnTriggerEnter2D(inCollider);
        }

        void OnBallStateChanged(Ball.BallStateType newState)
        {
            if (newState == Ball.BallStateType.SHOW)
                return;
            m_ball.BallStateChanged -= OnBallStateChanged;
            if (newState == Ball.BallStateType.JUMP || m_context.shelfType == ShelfType.DisappearByMultJumps)
            {
                Vector3 scale = m_context.transform.localScale;
                m_context.transform.localScale -= new Vector3(step, 0.0f, 0.0f);
                count--;
                base.OnTriggerEnter2D(inCollider);
                if (count == 0)
                {
                    m_context.GetComponent<Renderer>().enabled = false;
                    m_context.GetComponent<Collider2D>().isTrigger = false;
                    Destroy(m_context);
                }
            }           
        }
    }

    class ShelfTypeDisappearByMultJumpsState : ShelfTypeDisappearByOneJumpState
    {
        public ShelfTypeDisappearByMultJumpsState()
        {
            m_type = ShelfType.DisappearByMultJumps;
            count = 5;
        }
    }

    class TowardsOn1CellsRightState : ShelfTypeState
    {
        public TowardsOn1CellsRightState()
        {
            m_type = ShelfType.TowardsOn1CellsRight;
        }
    }

    class TowardsOn1CellsLeftState : ShelfTypeState
    {
        public TowardsOn1CellsLeftState()
        {
            m_type = ShelfType.TowardsOn1CellsLeft;
        }
    }

    class TowardsOn2CellsLeftState : ShelfTypeState
    {
        public TowardsOn2CellsLeftState()
        {
            m_type = ShelfType.TowardsOn2CellsLeft;
        }
    }

    class TowardsOn2CellsRighttState : ShelfTypeState
    {
        public TowardsOn2CellsRighttState()
        {
            m_type = ShelfType.TowardsOn2CellsRight;
        }
    }

    class ChangeableToTowardsOn1CellsState : ShelfTypeState
    {
        public ChangeableToTowardsOn1CellsState()
        {
            m_type = ShelfType.ChangeableToTowardsOn1Cells;
        }

        public override void OnTriggerEnter2D(Collider2D inCollider)
        {
            m_ball.BallStateChanged += OnBallStateChanged;            
            base.OnTriggerEnter2D(inCollider);
        }

        private void OnBallStateChanged(Ball.BallStateType newState)
        {
            if (newState == Ball.BallStateType.TO_LEFT)
            {
                m_context.shelfType = ShelfType.TowardsOn1CellsLeft;
                m_context.UpdateState();
                m_ball.BallStateChanged -= OnBallStateChanged;
            }
            if (newState == Ball.BallStateType.TO_RIGHT)
            {
                m_context.shelfType = ShelfType.TowardsOn1CellsRight;
                m_context.UpdateState();
                m_ball.BallStateChanged -= OnBallStateChanged;
            }
        }
    }

    class StickyState : ShelfTypeState
    {
        public StickyState()
        {
            m_type = ShelfType.Sticky;
        }
    }

    class DoubleSpikeState : ShelfTypeState
    {
        public DoubleSpikeState()
        {
            m_type = ShelfType.DoubleSpike;
        }
    }

    class BotomSpikeState : ShelfTypeState
    {
        public BotomSpikeState()
        {
            m_type = ShelfType.BotomSpike;
        }
    }

	Ball ball;
    public ShelfType shelfType;
    ShelfTypeState m_shelfTypeState;
    Dictionary<ShelfType, Type> m_states = new Dictionary<ShelfType, Type>
        {
            {ShelfType.Standart,typeof(ShelfTypeStandartState)},
            {ShelfType.Ice,typeof(ShelfTypeIceState)},
            {ShelfType.DisappearByOneJumps,typeof(ShelfTypeDisappearByOneJumpState)},
            {ShelfType.DisappearByMultJumps,typeof(ShelfTypeDisappearByMultJumpsState)},
            {ShelfType.TowardsOn1CellsRight,typeof(TowardsOn1CellsRightState)},
            {ShelfType.TowardsOn1CellsLeft,typeof(TowardsOn1CellsLeftState)},
            {ShelfType.TowardsOn2CellsLeft,typeof(TowardsOn2CellsLeftState)},
            {ShelfType.TowardsOn2CellsRight,typeof(TowardsOn2CellsRighttState)},
            {ShelfType.ChangeableToTowardsOn1Cells,typeof(ChangeableToTowardsOn1CellsState)},
            {ShelfType.Sticky,typeof(StickyState)},
            {ShelfType.BotomSpike,typeof(BotomSpikeState)},
            {ShelfType.DoubleSpike,typeof(DoubleSpikeState)},
        };
	// Use this for initialization
	void Start () 
	{
        if (ball == null)
            ball = GameObject.Find("Ball").GetComponent<Ball>();
        UpdateState();
	}
	
	// Update is called once per frame
	void Update () 
	{
	
	}

	void OnTriggerEnter2D(Collider2D inCollider)
	{
        m_shelfTypeState.OnTriggerEnter2D(inCollider);
	}

    ShelfTypeState GetShelfType()
    {
        Type type = null;
        if(m_states.ContainsKey(shelfType)){
            type = m_states[shelfType];
        }else{
            type = typeof(ShelfTypeStandartState);
        }
        ShelfTypeState state = System.Activator.CreateInstance(type) as ShelfTypeState;
        
        return state;
    }

    void UpdateState()
    {
        m_shelfTypeState = GetShelfType();
        m_shelfTypeState.SetParam(this, ball);
    }
}
