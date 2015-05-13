using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.IO;



public class Ball : MonoBehaviour
{
    #region States

    public enum BallStateType { SHOW, HIDE, IN_AIR, TO_DOWN, TO_LEFT, TO_RIGHT, JUMP } //Типы состояния.
    public struct MoveInfo // Структура для хранения информации о передвижении игрока
    {
        public MoveInfo(Vector2 v, float f) { dist = v; speed = f; }
        public Vector2 dist; // Направление движения
        public float speed; // Растояние которое пройдет игрок за один фрейм (кадр)
    }

    abstract class BallState // Общий интерфейс для всех состояний.
    {
        protected Ball m_context;
        public BallState(Ball context)
        {
            m_context = context;
        }
        virtual public void UpdateParams() { }
        virtual public void Left(int columns = 1) { } // Двигатся влево
        virtual public void Right(int columns = 1) { } // Двигатся вправо
        virtual public void Up() { } // Двигатся вверх
        virtual public void Down() { } // Двигатся вниз
        virtual public void Jump() { } // Прижки
        virtual public void OnGround(ShelfType shelfType) { } // Игрок коснулся твердой поверхности (полка, земля)
        virtual public void OnWall() { } // Игрок ударился об стену
        virtual public void OnCeiling(ShelfType shelfType) { } // Игрок ударился об потолок
        virtual public void Update() { } // Метод вызываемый при перерисовке сцены (фрейма)
        override public string ToString()
        {
            return "Ball State";
        } // перегрузка общ. метода для всех объектов
        abstract public BallStateType type { get; }
    }

    class BallStateHide : BallState
    {
        //Ball m_context;
        public BallStateHide(Ball context) : base(context) { }

        override public void UpdateParams()
        {
            m_context.gameObject.SetActive(false);
        }
        override public string ToString()
        {
            return "HIDE";
        }
        override public BallStateType type
        {
            get
            {
                return BallStateType.HIDE;
            }
        }
    }

    class BallStateShow : BallState
    {
        public BallStateShow(Ball context) : base(context) { }
        override public void UpdateParams()
        {
            m_context.gameObject.SetActive(true);
        }
        override public void Left(int columns)
        {
            m_context.SetState(BallStateType.TO_LEFT);
            m_context.MoveLeftRight(true, BallStateType.SHOW, columns);
        }
        override public void Right(int columns)
        {
            m_context.SetState(BallStateType.TO_RIGHT);
            m_context.MoveLeftRight(false, BallStateType.SHOW, columns);
        }
        override public void Up()
        {
            m_context.SetState(BallStateType.IN_AIR);
        }
        override public void Down()
        {
            m_context.SetState(BallStateType.TO_DOWN);
        }
        override public void Jump()
        {
            m_context.SetState(BallStateType.TO_DOWN);
        }
        override public void OnGround(ShelfType shelfType)
        {
            m_context.SetState(BallStateType.TO_DOWN);
        }
        override public void OnCeiling(ShelfType shelfType)
        {
            m_context.SetState(BallStateType.TO_DOWN);
        }
        override public string ToString()
        {
            return "SHOW";
        }
        override public BallStateType type
        {
            get
            {
                return BallStateType.SHOW;
            }
        }
    }

    class BallStateInAir : BallState // Игрок пригнул вверх
    {
        public BallStateInAir(Ball context) : base(context) { }
        override public void UpdateParams()
        {
            return;
        }
        override public void Left(int columns)
        {
            m_context.SetState(BallStateType.TO_LEFT); // переходим в состояние движения влево
            m_context.MoveLeftRight(true, BallStateType.IN_AIR, columns); // этот метод создает очередь векторов для движения влево
        }
        override public void Right(int columns)
        {
            m_context.SetState(BallStateType.TO_RIGHT); // переходим в состояние движения вправо
            m_context.MoveLeftRight(false, BallStateType.IN_AIR, columns);  // этот метод создает очередь векторов для движения влево
        }
        override public void Up()
        {
            return;  // вверх двигатся не можем. и так уже летим =))
        }
        override public void Down()
        {
            m_context.SetState(BallStateType.TO_DOWN); // переходим в состояние движения вниз
        }
        override public void OnGround(ShelfType shelfType)
        {
            m_context.moveVectors.Clear(); // скорее всего мертвый код !!!
        }
        override public void OnWall()
        {
            return; // летим прямолинейно вверх. встену не должны врезатся.
        }
        override public void OnCeiling(ShelfType shelfType)
        {
            m_context.SetState(BallStateType.TO_DOWN); // при попадание в потолок. переходим в состояние движения вниз
        }
        override public void Update() // перерисовка кадра
        {
            if (m_context.moveVectors.Count > 0)
            { // если есть в очереди вектора для движения
                MoveInfo move = m_context.moveVectors.Dequeue(); // берем следующий вектор
                m_context.Move(move); // вызываем метод игрока для движения
            }
            else
            {
                m_context.Move(new MoveInfo(m_context.upVector, m_context.UpDownSpeed)); // вызываем метод игрока для движения передав параметры движения вверх
            }
        }
        override public string ToString()
        {
            return "InAir";
        }
        override public BallStateType type
        {
            get
            {
                return BallStateType.IN_AIR;
            }
        }
    }

    class BallStateToDown : BallState // Игрок двигается вниз
    {
        public BallStateToDown(Ball context) : base(context) { }
        override public void OnGround(ShelfType shelfType) // призимление
        {
            m_context.SetState(BallStateType.JUMP); // переходим в режим прыжков
            m_context.OnGround(shelfType); // сообщаем игроку что приземлились
        }
        override public void Update() // перерисовка кадра
        {
            m_context.Move(new MoveInfo(m_context.downVector, m_context.UpDownSpeed));
        }
        override public string ToString()
        {
            return "ToDown";
        }
        override public BallStateType type
        {
            get
            {
                return BallStateType.TO_DOWN;
            }
        }
    }

    class BallStateToLeft : BallState // Игрок двигается влево
    {
        public BallStateToLeft(Ball context) : base(context) { }
        override public void OnGround(ShelfType shelfType) // приземлились
        {
            m_context.SetState(BallStateType.JUMP); // переходим в состояние прыжков
            m_context.OnGround(shelfType); // сообщаем игроку что приземлились
        }
        override public void OnWall() //удар об стенку
        {
            m_context.FromWallToShelf(); // возвращаем  игрока назад
        }
        override public void Update() // перерисовка кадра
        {
            if (m_context.moveVectors.Count > 0)
            { // если есть ещо векторы для движения влево
                MoveInfo move = m_context.moveVectors.Dequeue(); // берем следующий
                m_context.Move(move); // двигаем игрока
            }
            else
            {
                m_context.SetState(BallStateType.TO_DOWN); // иначе двигаемся вниз
            }
        }
        override public string ToString()
        {
            return "ToLeft";
        }
        override public BallStateType type
        {
            get
            {
                return BallStateType.TO_LEFT;
            }
        }
    }

    class BallStateToRight : BallState // Игрок двигается вправо
    {
        public BallStateToRight(Ball context) : base(context) { }
        override public void OnGround(ShelfType shelfType) // приземлились
        {
            m_context.SetState(BallStateType.JUMP); // переходим в состояние прыжков
            m_context.OnGround(shelfType); // сообщаем игроку про приземление
        }
        override public void OnWall()
        {
            m_context.FromWallToShelf(); // возвращаем назад игрока
        }
        override public void Update() // перерисовка кадра
        {
            if (m_context.moveVectors.Count > 0)
            { // если есть куда двигатся
                MoveInfo move = m_context.moveVectors.Dequeue(); // берем сл. шаг
                m_context.Move(move); // двигаем
            }
            else
            {
                m_context.SetState(BallStateType.TO_DOWN); // падаем вниз
            }
        }
        override public string ToString()
        {
            return "ToRight";
        }
        override public BallStateType type
        {
            get
            {
                return BallStateType.TO_RIGHT;
            }
        }
    }

    class BallStateJump : BallState // Игрок прыгает на месте
    {
        public BallStateJump(Ball context) : base(context) { }
        override public void Left(int columns) // прыгаем влево
        {
            m_context.SetState(BallStateType.TO_LEFT); // переходим в сотояние движение влево
            m_context.MoveLeftRight(true, BallStateType.JUMP, columns); // двигаем влево
        }
        override public void Right(int columns) // прыжок вправо
        {
            m_context.SetState(BallStateType.TO_RIGHT); // переходим в сотояние движения вправо
            m_context.MoveLeftRight(false, BallStateType.JUMP, columns);	// двигаем вправо
        }
        override public void Up()
        {
            m_context.moveVectors.Clear(); // чистим очередь векторов для прыжков
            m_context.SetState(BallStateType.IN_AIR); // переходим в состояние движения вверх
        }
        override public void OnGround(ShelfType shelfType)
        {
            m_context.Jump(); // при приземлении прыгаем ещо раз
        }
        public override void Jump()
        {
            m_context.Jump();
        }
        override public void Update() // перерисовка кадра
        {
            if (m_context.moveVectors.Count > 0)
            { // если есть ещо что двигать
                MoveInfo move = m_context.moveVectors.Dequeue(); // двигаем
                m_context.Move(move);
            }
            else
            {
                m_context.SetState(BallStateType.TO_DOWN);
            }
        }
        override public string ToString()
        {
            return "Jump";
        }
        override public BallStateType type
        {
            get
            {
                return BallStateType.JUMP;
            }
        }
    }
    #endregion
    List<MonoBehaviour> m_Shelfs = new List<MonoBehaviour>(); // список всех доступных полок на сцене	
    public List<MonoBehaviour> Shelfs
    {
        get
        {
            return m_Shelfs;
        }
        set
        {
            m_Shelfs = value;
        }
    }

    #region direction_vectors
    Vector2 m_StopVector = new Vector2(0, 0); // вектор для остановки
    public Vector2 stopVector
    {
        get
        {
            return m_StopVector;
        }
    }
    Vector2 m_DownVector = new Vector2(0, -1); // вектор движения вниз
    public Vector2 downVector
    {
        get
        {
            return m_DownVector;
        }
    }
    Vector2 m_UpVector = new Vector2(0, 1); // вектор движения вверх
    public Vector2 upVector
    {
        get
        {
            return m_UpVector;
        }
    }
    Vector2 m_LeftVector = new Vector2(-1, 0); // вектор движения влево
    public Vector2 leftVector
    {
        get
        {
            return m_LeftVector;
        }
    }
    Vector2 m_RightVector = new Vector2(1, 0); // вектор движения вправо
    public Vector2 rightVector
    {
        get
        {
            return m_RightVector;
        }
    }
    public Vector2 JumpVector = new Vector2(0, 4); // вектор длины прыжков на месте
    public Vector2 LeftRightVector = new Vector2(10, 0); // вектор длины прыжка влево/право (сейчас не используется)
    #endregion
    public float LeftRightSpeed = 0.05f; // скорость передвижения влево/право
    public float UpDownSpeed = 0.015f; // скорость движения вверх/вниз
    public float JumpSpeed = 0.010f; // скорость прыжков
    float m_CellSide = 10.0f; // длина стороны ячейки

    BallState m_State; // текущее состояние игрока
    Dictionary<BallStateType, BallState> m_States; // список доступных состояний
    Queue<MoveInfo> m_moveVectors = new Queue<MoveInfo>(); // очередь векторов для движения
    Stack<MoveInfo> m_moveBackVectors = new Stack<MoveInfo>(); // стек векторов для обратного движения
    Queue<MoveInfo> moveVectors
    {
        get
        {
            return m_moveVectors;
        }
    }
    //MoveInfo m_moveVector; //текущий вектор для движения

    public void SetState(BallStateType type) // метод для изменения состояния
    {
        if (m_States.ContainsKey(type)) // если тип сотояния есть в списке доступных
        {
            m_State = m_States[type]; // то устаналвиваем ссылку на этот тип.
            m_State.UpdateParams();
        }
    }

    public BallStateType GetState()
    {
        return m_State.type;
    }

    // Use this for initialization
    void Start()
    {
        m_CellSide = GameInfo.cellSide;

        //m_moveVector.dist = m_StopVector; //началный вектор движения - стоять на месте.
        //m_moveVector.speed = 0.0f; // начальная скорость движения
        m_States = new Dictionary<BallStateType, BallState>(7);
        m_States[BallStateType.SHOW] = new BallStateShow(this);
        m_States[BallStateType.HIDE] = new BallStateHide(this);
        m_States[BallStateType.IN_AIR] = new BallStateInAir(this);
        m_States[BallStateType.TO_DOWN] = new BallStateToDown(this);
        m_States[BallStateType.TO_LEFT] = new BallStateToLeft(this);
        m_States[BallStateType.TO_RIGHT] = new BallStateToRight(this);
        m_States[BallStateType.JUMP] = new BallStateJump(this);
        SetState(BallStateType.SHOW); // начальное состоянние. (можно продумать инициализ. при старте)      
    }

    int MinMultiple(float number, int multiply)
    {
        int ret;
        if (number >= 0)
            ret = (int)(number / multiply) * multiply;
        else
            ret = multiply * ((int)(number / multiply) - 1);
        return ret;
    }

    int MaxMultiple(float number, int multiply)
    {
        int ret;
        if (number >= 0)
            ret = multiply * ((int)(number / multiply) + 1);
        else
            ret = multiply * (int)(number / multiply);
        return ret;
    }

    int Sign(float number)
    {
        return (number > 0) ? 1 : -1;
    }

    bool ObstacleCheck() // функция проверки преграды сверху
    {
        float x = transform.position.x; //текущая позиция по х
        float y = transform.position.y; // текущая позиция по у
        float upDek = MaxMultiple(y, (int)m_CellSide);
        float leftDek = MinMultiple(x, (int)m_CellSide);
        float rightDek = MaxMultiple(x, (int)m_CellSide);
        float halfSide = m_CellSide / 2;

        List<MonoBehaviour> shelfs = new List<MonoBehaviour>();
        shelfs.AddRange(m_Shelfs);
        MonoBehaviour ceil = GameObject.Find("Ceiling").GetComponent<MonoBehaviour>();
        if (ceil != null)
        {
            float pY = ceil.transform.position.y; // значение У потолка
            float pX = ceil.transform.position.x;
            if (pY >= (upDek - halfSide) && pY <= (upDek + halfSide))
                return true;
        }
        foreach (MonoBehaviour s in shelfs) // перебираем все полочки на сцене
        {
            float pY = s.transform.position.y; // значение У полки
            float pX = s.transform.position.x; // значение Х полки
            if (pY >= (upDek - halfSide) && pY <= (upDek + halfSide) && pX >= leftDek && pX <= rightDek)
                return true;
        }
        return false;
    }

    bool PreLeftRight(BallStateType stateFrom) // функция предварительных расчетов перед полетом влево или право
    {

        float halfCell = m_CellSide / 2;
        float y = transform.position.y;
        float x = transform.position.x;
        float shiftY = 0, shiftX = 0;
        int directY = 0, directX = 0;
        float absY = Mathf.Abs(y), absX = Mathf.Abs(x);
        float downDek = 0, leftDek = 0;

        downDek = MinMultiple(y, (int)m_CellSide);
        leftDek = MinMultiple(x, (int)m_CellSide);
        shiftY = (downDek + halfCell) - y;
        shiftX = (leftDek + halfCell) - x;
        directY = Sign(shiftY);
        directX = Sign(shiftX);

        shiftY = Mathf.Abs(shiftY);
        shiftX = Mathf.Abs(shiftX);

        if (stateFrom == BallStateType.IN_AIR)
        {
            if (y > (downDek + 1.0f * halfCell))
            {
                if (ObstacleCheck())
                {
                    return false;
                }
                shiftY = downDek + 1.5f * m_CellSide - y;
                directY = 1;
            }
        }

        float difShift = shiftY % LeftRightSpeed;
        int cntFrames = (int)(shiftY / LeftRightSpeed);
        while (cntFrames > 0)
        {
            m_moveVectors.Enqueue(new MoveInfo(new Vector2(0, directY), LeftRightSpeed));
            cntFrames--;
        }
        m_moveVectors.Enqueue(new MoveInfo(new Vector2(0, directY), difShift));

        difShift = shiftX % LeftRightSpeed;
        cntFrames = (int)(shiftX / LeftRightSpeed);
        cntFrames = 1;
        while (cntFrames > 0)
        {
            m_moveVectors.Enqueue(new MoveInfo(new Vector2(0, directX), LeftRightSpeed));
            m_moveVectors.Enqueue(new MoveInfo(new Vector2(directX, 0), shiftX));
            cntFrames--;
        }
        m_moveVectors.Enqueue(new MoveInfo(new Vector2(0, directX), difShift));

        return true;
    }

    public void MoveLeftRight(bool left, BallStateType stateFrom, int columns)
    {
        m_moveVectors.Clear(); // очищаем очередь движений
        m_moveBackVectors.Clear(); // очищаем очередь обратных движений

        if (!PreLeftRight(stateFrom))
        {
            return;
        }

        Vector2 direction = left ? leftVector : rightVector;

        float shift = m_CellSide * columns;
        int cntFrames = (int)(shift / LeftRightSpeed);
        float difShift = shift % LeftRightSpeed;

        float y = 0;
        float x = 0;
        const float maxX = 1.0f;
        float d = maxX / cntFrames;

        while (cntFrames > 0)  // обработка движения влево/вправо по кадрам
        {
            x += d;
            y = x * (1 - x) * 1.75f;
            if (x > maxX / 2)
                y = -y;
            if (x == maxX / 2)
                y = 0;
            Vector2 mov = new Vector2(direction.x, y);

            m_moveVectors.Enqueue(new MoveInfo(mov, LeftRightSpeed));
            cntFrames--;
        }
        m_moveVectors.Enqueue(new MoveInfo(direction, difShift));  // движение корректировки

        cntFrames = (int)(m_CellSide / LeftRightSpeed);

        while (cntFrames > 0) // для возвращения стейта даун для мячика (чтобы работали полки)
        {
            m_moveVectors.Enqueue(new MoveInfo(downVector, LeftRightSpeed));
            cntFrames--;
        }
    }

    public void Move(MoveInfo moveInfo) // движение игрока
    {
        transform.Translate(moveInfo.dist * moveInfo.speed); // двигаем игрока        
        moveInfo.dist.x *= -1; //меняем направление по х на противоположное
        moveInfo.dist.y *= -1; //меняем направление по у на противоположное
        m_moveBackVectors.Push(moveInfo); // ложим в стек инфо про посл. движение игрока        
    }
    
    public void FromWallToShelf()
    {
        m_moveVectors.Clear(); // очищаем данные о движении
        while (m_moveBackVectors.Count > 0)
        {
            MoveInfo move = m_moveBackVectors.Pop(); // посл. движение            
            m_moveVectors.Enqueue(move); // перекладываем данные о движении в обратном порядке
        }
    }

    public void Jump()
    {
        m_moveVectors.Clear(); // очищаем данные про движения
        m_moveBackVectors.Clear(); // очищаем стек обратных движений
        Vector2 dist = JumpVector; // вектор движения прыжков
        float len = dist.magnitude + 1; // высота прыжка
        int N = (int)(len / JumpSpeed); // кол-во кадров для прыжка вверх
        int cnt = N * 2; // кол-во кадров для всего прыжка
        while (cnt > 0)
        {
            if (cnt > N) // если движение вверх (первая фаза)
                m_moveVectors.Enqueue(new MoveInfo(m_UpVector, JumpSpeed));
            else // движение вниз
                m_moveVectors.Enqueue(new MoveInfo(m_DownVector, JumpSpeed));
            cnt--;
        }
    }
    // Update is called once per frame
    void FixedUpdate()
    {
        m_State.Update(); // Делегируем обработку текущему состоянию        

        PressedKey();
    }

    void PressedKey()
    {
        if (Input.GetKey(KeyCode.UpArrow))
        {
            m_State.Up(); // Делегируем обработку текущему состоянию
        }
        if (Input.GetKey(KeyCode.DownArrow))
        {
            m_State.Down(); // Делегируем обработку текущему состоянию
        }
        if (Input.GetKey(KeyCode.RightArrow))
        {
            m_State.Right(); // Делегируем обработку текущему состоянию
        }
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            m_State.Left(); // Делегируем обработку текущему состоянию
        }
    }

    public void LeftKey(int columns = 1)
    {
        m_State.Left(columns);
    }

    public void RightKey(int columns = 1)
    {
        m_State.Right(columns);
    }

    public void UpKey()
    {
        m_State.Up();
    }

    public void DownKey()
    {
        m_State.Down();
    }

    public void OnGround(ShelfType shelfType)
    {
        BallStateType stateFrom = m_State.type;
        //SetState(BallStateType.SHOW);
        SetState(BallStateType.JUMP);
        bool anyKey = Input.anyKey; // если нажата любая кнопка
        if (Input.GetKey(KeyCode.DownArrow))
            anyKey = false;
        switch (shelfType)
        {
            case ShelfType.Ice:
                if (anyKey)
                {
                    PressedKey();
                    return;
                }
                switch (stateFrom)
                {
                    case BallStateType.TO_LEFT:
                        LeftKey();
                        return;
                    case BallStateType.TO_RIGHT:
                        RightKey();
                        return;
                }
                break;
            case ShelfType.TowardsOn1CellsRight:
                RightKey();
                break;
            case ShelfType.TowardsOn1CellsLeft:
                LeftKey();
                break;
            case ShelfType.TowardsOn2CellsLeft:
                LeftKey(2);
                break;
            case ShelfType.TowardsOn2CellsRight:
                RightKey(2);
                break;
        }
        //SetState(BallStateType.JUMP);
        m_State.Jump();
    }

    public void OnCeiling(ShelfType shelfType)
    {
        m_State.OnCeiling(shelfType); // Делегируем обработку текущему состоянию
    }

    public void OnWall()
    {
        m_State.OnWall(); // Делегируем обработку текущему состоянию
    }
}
