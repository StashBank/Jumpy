using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

#region States

public enum BallStateType {SHOW,HIDE,IN_AIR,TO_DOWN,TO_LEFT,TO_RIGHT,JUMP} //Типы состояния.
public struct MoveInfo // Структура для хранения информации о передвижении игрока
{
	public MoveInfo(Vector2 v,float f){dist = v;speed = f;}
	public Vector2 dist; // Направление движения
	public float speed; // Растояние которое пройдет игрок за один фрейм (кадр)
}

interface BallState // Общий интерфейс для всех состояний.
{
    void UpdateParams();
	void Left(int columns = 1); // Двигатся влево
    void Right(int columns = 1); // Двигатся вправо
	void Up(); // Двигатся вверх
	void Down(); // Двигатся вниз
	void Jump(); // Прижки
    void OnGround(ShelfType shelfType); // Игрок коснулся твердой поверхности (полка, земля)
	void OnWall(); // Игрок ударился об стену
    void OnCeiling(ShelfType shelfType); // Игрок ударился об потолок
	void Update(); // Метод вызываемый при перерисовке сцены (фрейма)
	string ToString (); // перегрузка общ. метода для всех объектов
    BallStateType type {get;}
}

class BallStateHide : BallState
{
    Ball m_context;
    public BallStateHide(Ball context)
	{
		m_context = context;
	}
    public void UpdateParams() 
    {
        m_context.gameObject.SetActive(false);
    }
    public void Left(int columns) 
    {
        return;
    }
    public void Right(int columns) 
    { 
        return; 
    }
    public void Up() 
    {
        return; 
    }
    public void Down() 
    {
        return;
    }
    public void Jump() 
    {
        return; 
    }
    public void OnGround(ShelfType shelfType) 
    {
        return; 
    }
    public void OnWall() 
    {
        return; 
    }
    public void OnCeiling(ShelfType shelfType) 
    {
        return; 
    }
    public void Update() 
    {
        return;
    }
    override public string ToString()
    {
        return "HIDE";
    }
    public BallStateType type 
    {
        get
        {
            return BallStateType.HIDE;
        }
    }
}

class BallStateShow : BallState
{
    Ball m_context;
    public BallStateShow(Ball context)
    {
        m_context = context;
    }
    public void UpdateParams() 
    {
        m_context.gameObject.SetActive(true);
    }
    public void Left(int columns) 
    {
        m_context.SetState(BallStateType.TO_LEFT);
        m_context.MoveLeftRight(true, BallStateType.SHOW,columns);
    }
    public void Right(int columns) 
    {
        m_context.SetState(BallStateType.TO_RIGHT);
        m_context.MoveLeftRight(false, BallStateType.SHOW,columns);
    }
    public void Up() 
    {
        m_context.SetState(BallStateType.IN_AIR);
    }
    public void Down() 
    {
        m_context.SetState(BallStateType.TO_DOWN);
    }
    public void Jump() 
    {
        m_context.SetState(BallStateType.TO_DOWN);
    }
    public void OnGround(ShelfType shelfType) 
    {
        m_context.SetState(BallStateType.TO_DOWN);
    }
    public void OnWall() 
    {
        return;
    }
    public void OnCeiling(ShelfType shelfType) 
    {
        m_context.SetState(BallStateType.TO_DOWN);
    }
    public void Update() 
    {
        return;
    }
    override public string ToString()
    {
        return "SHOW";
    }
    public BallStateType type
    {
        get
        {
            return BallStateType.SHOW;
        }
    }
}

class BallStateInAir : BallState // Игрок пригнул вверх
{
	Ball m_context; // переменная для хранения ссылки на игрока (шарик =) )
	public BallStateInAir(Ball context)
	{
		m_context = context;
	}
    public void UpdateParams()
    {
        return;
    }
    public void Left(int columns)
	{
        m_context.SetState(BallStateType.TO_LEFT); // переходим в состояние движения влево
        m_context.MoveLeftRight(true, BallStateType.IN_AIR,columns); // этот метод создает очередь векторов для движения влево
	}
    public void Right(int columns)
	{
        m_context.SetState(BallStateType.TO_RIGHT); // переходим в состояние движения вправо
        m_context.MoveLeftRight(false, BallStateType.IN_AIR,columns);  // этот метод создает очередь векторов для движения влево
	}
	public void Up()
	{
		return;  // вверх двигатся не можем. и так уже летим =))
	}
	public void Down()
	{
		m_context.SetState(BallStateType.TO_DOWN); // переходим в состояние движения вниз
	}
	public void Jump()
	{
		return; // Находясь в полете прыгать не можем
	}
    public void OnGround(ShelfType shelfType)
	{
		m_context.moveVectors.Clear(); // скорее всего мертвый код !!!
	}
	public void OnWall()
	{
		return; // летим прямолинейно вверх. встену не должны врезатся.
	}
    public void OnCeiling(ShelfType shelfType)
	{
		m_context.SetState(BallStateType.TO_DOWN); // при попадание в потолок. переходим в состояние движения вниз
	}
	public void Update() // перерисовка кадра
	{
		if(m_context.moveVectors.Count > 0){ // если есть в очереди вектора для движения
			MoveInfo move = m_context.moveVectors.Dequeue(); // берем следующий вектор
            m_context.Move(move); // вызываем метод игрока для движения
		}
		else{
            m_context.Move(new MoveInfo(m_context.upVector, m_context.UpDownSpeed)); // вызываем метод игрока для движения передав параметры движения вверх
		}
	}
	override public string ToString ()
	{
		return "InAir";
	}
    public BallStateType type
    {
        get
        {
            return BallStateType.IN_AIR;
        }
    }
}

class BallStateToDown : BallState // Игрок двигается вниз
{
    Ball m_context; // переменная для хранения ссылки на игрока (шарик =) )
	public BallStateToDown(Ball context)
	{
		m_context = context;
	}
    public void UpdateParams() 
    {
        return;
    }
	public void Left(int columns)
	{
		return; // при движении вниз влево двигатся не можем
	}
    public void Right(int columns)
	{
        return; // при движении вниз вправо двигатся не можем
	}
	public void Up()
	{
        return; // при движении вверх влево двигатся не можем
	}
	public void Down()
	{
        return; // уже и так двигатемся вниз. больне не можем
	}
	public void Jump()
	{
        return; // при движении вниз прыгать не можем
	}
	public void OnGround(ShelfType shelfType) // призимление
	{
		m_context.SetState(BallStateType.JUMP); // переходим в режим прыжков
		m_context.OnGround(shelfType); // сообщаем игроку что приземлились
	}	
	public void OnWall()
	{
		return; // вниз вдигаемся прямолинейно, в стенки попасть не можем
	}
	public void OnCeiling(ShelfType shelfType)
	{
		return; // двигаемся вниз, значит в потолок не попадем
	}
    public void Update() // перерисовка кадра
	{		
        m_context.Move(new MoveInfo(m_context.downVector, m_context.UpDownSpeed));
	}
	override public string ToString ()
	{
		return "ToDown";
	}
    public BallStateType type
    {
        get
        {
            return BallStateType.TO_DOWN;
        }
    }
}

class BallStateToLeft : BallState // Игрок двигается влево
{
    Ball m_context; // переменная для хранения ссылки на игрока (шарик =) )
	public BallStateToLeft(Ball context)
	{
		m_context = context;
	}
    public void UpdateParams() 
    {
        return;
    }
    public void Left(int columns)
	{
		return; // уже двигаемся в лево
	}
    public void Right(int columns)
	{
		return; // вправо двигатся при движении влево не можем
	}
	public void Up()
	{
		return; // пока движемся влево вверх не подымаемся
	}
	public void Down()
	{
		return; // пока движемся влево падать не можем
	}
	public void Jump()
	{
		return; // прыгать в этом состоянии не можем
	}
	public void OnGround(ShelfType shelfType) // приземлились
	{
		m_context.SetState(BallStateType.JUMP); // переходим в состояние прыжков
		m_context.OnGround(shelfType); // сообщаем игроку что приземлились
	}	
	public void OnWall() //удар об стенку
	{
		m_context.FromWallToShelf(); // возвращаем  игрока назад
	}	
	public void OnCeiling(ShelfType shelfType) // поидее в потолок попасть при движении влево не можем
	{
		return;
	}
    public void Update() // перерисовка кадра
	{
		if(m_context.moveVectors.Count > 0){ // если есть ещо векторы для движения влево
			MoveInfo move = m_context.moveVectors.Dequeue(); // берем следующий
            m_context.Move(move); // двигаем игрока
		}
		else{
            m_context.SetState(BallStateType.TO_DOWN); // иначе двигаемся вниз
		}
	}
	override public string ToString ()
	{
		return "ToLeft";
	}
    public BallStateType type
    {
        get
        {
            return BallStateType.TO_LEFT;
        }
    }
}

class BallStateToRight : BallState // Игрок двигается вправо
{
    Ball m_context; // переменная для хранения ссылки на игрока (шарик =) )
	public BallStateToRight(Ball context)
	{
		m_context = context;
	}
    public void UpdateParams()
    {
        return;
    }
    public void Left(int columns)
	{
		return; // при движении вправо влево уже дивигатся не можем
	}
    public void Right(int columns)
	{
		return; // уже и так вправо двигаемся
	}
	public void Up()
	{
		return; // вверх двигатся не можем
	}
	public void Down()
	{
		return; // вниз не можем двигатся
	}
	public void Jump()
	{
		return; //прыгать отменяется =))
	}	
	public void OnGround(ShelfType shelfType) // приземлились
	{
		m_context.SetState(BallStateType.JUMP); // переходим в состояние прыжков
		m_context.OnGround(shelfType); // сообщаем игроку про приземление
	}	
	public void OnWall()
	{
		m_context.FromWallToShelf(); // возвращаем назад игрока
	}	
	public void OnCeiling(ShelfType shelfType) // если расчеты правильные то достич потолка не получится
	{
	}
    public void Update() // перерисовка кадра
	{
		if(m_context.moveVectors.Count > 0){ // если есть куда двигатся
			MoveInfo move = m_context.moveVectors.Dequeue(); // берем сл. шаг
            m_context.Move(move); // двигаем
		}
		else{
			m_context.SetState(BallStateType.TO_DOWN); // падаем вниз
		}
	}
	override public string ToString ()
	{
		return "ToRight";
	}
    public BallStateType type
    {
        get
        {
            return BallStateType.TO_RIGHT;
        }
    }
}

class BallStateJump : BallState // Игрок прыгает на месте
{
    Ball m_context; // переменная для хранения ссылки на игрока (шарик =) )
    public BallStateJump(Ball context)
	{
		m_context = context;
	}
    public void UpdateParams() 
    {
        return;
    }
    public void Left(int columns) // прыгаем влево
	{
        m_context.SetState(BallStateType.TO_LEFT); // переходим в сотояние движение влево
        m_context.MoveLeftRight(true, BallStateType.JUMP,columns); // двигаем влево
	}
    public void Right(int columns) // прыжок вправо
	{
        m_context.SetState(BallStateType.TO_RIGHT); // переходим в сотояние движения вправо
        m_context.MoveLeftRight(false, BallStateType.JUMP,columns);	// двигаем вправо
	}
	public void Up()
	{
        m_context.moveVectors.Clear(); // чистим очередь векторов для прыжков
        m_context.SetState(BallStateType.IN_AIR); // переходим в состояние движения вверх
	}
	public void Down()
	{
		return; // не получится
	}	
	public void Jump()
	{
		return; // уже в состоянии прыжков
	}	
	public void OnGround(ShelfType shelfType)
	{
		m_context.Jump(); // при приземлении прыгаем ещо раз
	}	
	public void OnWall()
	{
		return; // попасть в стенку не можем
	}	
	public void OnCeiling(ShelfType shelfType)
	{
		return; // до потолка не должны допрыгнуть
	}
    public void Update() // перерисовка кадра
	{
		if(m_context.moveVectors.Count > 0){ // если есть ещо что двигать
			MoveInfo move = m_context.moveVectors.Dequeue(); // двигаем
            m_context.Move(move);
		}
	}
	override public string ToString ()
	{
		return "Jump";
	}
    public BallStateType type
    {
        get
        {
            return BallStateType.JUMP;
        }
    }
}

#endregion

public class Ball : MonoBehaviour 
{
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
	Vector2 m_StopVector = new Vector2(0,0); // вектор для остановки
	public Vector2 stopVector{
		get {
			return m_StopVector;
		}
	}
	Vector2 m_DownVector = new Vector2(0,-1); // вектор движения вниз
	public Vector2 downVector{
		get{
			return m_DownVector;
		}
	}
	Vector2 m_UpVector = new Vector2(0,1); // вектор движения вверх
	public Vector2 upVector{
		get{
			return m_UpVector;
		}
	}
	Vector2 m_LeftVector = new Vector2(-1,0); // вектор движения влево
	public Vector2 leftVector{
		get{
			return m_LeftVector;
		}
	}
	Vector2 m_RightVector = new Vector2(1,0); // вектор движения вправо
	public Vector2 rightVector{
		get{
			return m_RightVector;
		}
	}
	public Vector2 JumpVector = new Vector2(0,4); // вектор длины прыжков на месте
	public Vector2 LeftRightVector = new Vector2(10,0); // вектор длины прыжка влево/право (сейчас не используется)
	#endregion
	public float LeftRightSpeed = 0.05f; // скорость передвижения влево/право
	public float UpDownSpeed = 0.015f; // скорость движения вверх/вниз
	private float JumpSpeed = 0.010f; // скорость прыжков
    float m_LeftX = -30.0f; // крайняя левая х координата
    float m_RightX = 30.0f; // крайняя правая х координата
    float m_DownY = -30.0f; // крайняя нижняя у координата    
    float m_UpY = 30.0f; // крайняя верхняя у координата
    float m_CellSide = 10.0f; // длина стороны ячейки

    BallState m_State; // текущее состояние игрока
    Dictionary<BallStateType, BallState> m_States; // список доступных состояний
	Queue<MoveInfo> m_moveVectors = new Queue<MoveInfo>(); // очередь векторов для движения
    Stack<MoveInfo> m_moveBackVectors = new Stack<MoveInfo>(); // стек векторов для обратного движения
	public Queue<MoveInfo> moveVectors{
		get{
			return m_moveVectors;
		}
	}
	MoveInfo m_moveVector; //текущий вектор для движения

	public void SetState(BallStateType type) // метод для изменения состояния
	{
        if (m_States.ContainsKey(type)) // если тип сотояния есть в списке доступных
        {
            m_State = m_States[type]; // то устаналвиваем ссылку на этот тип.
            m_State.UpdateParams();
        }
	}

	// Use this for initialization
	void Start () 
	{
        //Camera cam = GameObject.Find("MainCamera").GetComponent<Camera>();
        m_LeftX = -100.0f;//-cam.ScreenToWorldPoint(new Vector2(Screen.width,0)).x;
        m_RightX = -m_LeftX;
        m_CellSide = GameInfo.cellSide;

		m_moveVector.dist = m_StopVector; //началный вектор движения - стоять на месте.
		m_moveVector.speed = 0.0f; // начальная скорость движения
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

    bool ObstacleCheck() // функция проверки преграды сверху
    {
        float x = transform.position.x; //текущая позиция по х
        float y = transform.position.y; // текущая позиция по у
        int level = (int)((y + m_RightX) / m_CellSide) + 1; // расчет текущего уровня по высоте (строка). 
        /* смещаем у на значение нижнего у поля. таким образом переместив координатную сетку к позиции 0,0 камеры
         * делим на высоту (сторону) нашей ячейки и узнаем текущий уровень. в данном случаее 0 это первый.
         * тоесть если у менее высоты то в р-те деления и приведения к целому будет 0. 
         * прибавив к рез-ту 1 узнаем номер этажа на котором находится игрок.
         */
        int column = (int)((x + 30.0f) / m_CellSide); // расчет текущей кололонки в которой находится игрок
        /* смещаем х на значение левого х поля. таким образом переместив координатную сетку к позиции 0,0 камеры
         * делим на ширина (сторону) нашей ячейки и узнаем текущую колонку. в данном случаее 0 это первая.         
         */
        float Y = m_DownY + (m_CellSide * level); // значение У координаты следующего уровня.
        //(к нижнему значению у добавляем сумму высот всех ячеек до текущего уровня)
        float X = m_LeftX + (10.0f * column); // значение х координаты текущей колонки
        // к крайнему левому значению х добавляем сумму всех колонок до текущей

        foreach (MonoBehaviour s in m_Shelfs) // перебираем все полочки на сцене
        {
            float pY = s.transform.position.y; // значение У полки
            float pX = s.transform.position.x; // значение Х полки
            if (pY >= Y && pX >= X && pX <= (X+10)) // если полка находится уровнем выше чем игрок и находится в той же колонке что игрок
                return true; // возвращаем истину
        }
        return false;
    }

	bool PreLeftRight(BallStateType stateFrom) // функция предварительных расчетов перед полетом влево или право
	{
		Vector2 pos = new Vector2(transform.position.x,transform.position.y); // теущая позиция игрока
		float H = pos.y + m_UpY; // сдвиг координатной сетки на позиции (0,0).
		int level = (int)(H / m_CellSide); // текущий уровень игрока (0 - первый).
		float h = m_CellSide * level; // У координата полки текущего уровня
		float hH = H - h; // позиция по У Игрока относительно полки
		float moveLen = 0; // растояние сдвига игрока небоходимое для переноса в центр ячейки (иницализировано в 0)
		Vector2 movVector = upVector; // вектор для направления сдвига (инициализировано в вектор- вверх)
        float halfCell = m_CellSide / 2;
		if(hH > halfCell){ // если позиция Игрока относительно полки больше половины ячейки
            if (stateFrom != BallStateType.JUMP) // если Игрок не в состоянии прыжка
			{
				if (!ObstacleCheck()) // если нет приград сверху (т.к. при движении Игрок окажется на сл уровне
				{
					moveLen = m_CellSide * 1.5f - hH; // растояние на которое нужно сдвинуть игрока - выстоа ячейки + пол следующей ячейки и минус его позиция отосительно текущей полки
				}
				else
				{
                    SetState(BallStateType.IN_AIR); // иначе продолжаем движение вверх
					return false;
				}
			}
			else
			{
				movVector = downVector; // если игрок в состоянии прижков. не логично двигать его до сл. уровня
				moveLen = halfCell - hH; // поетому сдвигамем его вниз к центру ячейки.
			}
		}
		
		if(hH < halfCell){ // если игрок ниже чем середина ячейки
			moveLen = halfCell - hH; // сдвигаем игрока вверх до средины текущей ячейки
		}
		
		if (moveLen > 0) // если Игрока нужно сдвинуть
		{
			int N = (int)(moveLen / LeftRightSpeed); // кол-во необходимых кадров для передвижения
			while (N > 0) // пока есть кадры
			{
				m_moveVectors.Enqueue(new MoveInfo(movVector, LeftRightSpeed)); // ложим в очередь инфу о движении
				N--; //уменьшаем кол-во необходимых кадров
			}
		}

		return true;
	}

    public void MoveLeftRight(bool left, BallStateType stateFrom, int columns)
	{
		Vector2 direction = left?m_LeftVector:m_RightVector; // куда двигаемся? влево или вправо
		m_moveVectors.Clear(); // очищаем очередь движений
        m_moveBackVectors.Clear(); // очищаем очередь обратных движений
		Vector2 pos = new Vector2(transform.position.x,transform.position.y); //текущая позиция Игрока
		int column = (int)((pos.x + m_RightX ) / m_CellSide); // текущая колонка

		float len = 0.0f; // длина на которую нужно передвинуть Игрока
        float newX = 0.0f; // новая позиция по Х
        float addCol = (columns - 1) * m_CellSide;
		if(left){
            newX = m_LeftX + (column - 1) * m_CellSide - addCol + m_CellSide / 2;
		}
		else{
            newX = m_LeftX + (column + 1) * m_CellSide + addCol + m_CellSide / 2;
		}
        len = (pos - new Vector2(newX,pos.y)).magnitude;

        float speed = columns == 1 ? LeftRightSpeed : LeftRightSpeed * 1.5f;
		int cnt = (int)(len / speed)+1; // кол-во кадров для передвижения
        const float maxX = 1.0f; // макс. знач. Х для расчета траектории по формуле (в масштабе)
		float d = maxX / cnt; // шаг изм. Х за один кадр в масштабе
		float x = 0;
        float y = 0.0f;

		if(!PreLeftRight(stateFrom)){
			return;
		}

        float posX = pos.x;
		while(cnt > 0)
		{
            x += d;
            if (columns == 1)
            {
                y = 0.5f * x;
                //y = x * (1 - x);
                //y = Mathf.Sin(x);
            }
            else
            {
                y = 0;
            }

            if (x > maxX / 2)
                y = -y;
            if (x == maxX / 2)
                y = 0;           
			Vector2 mov = direction+new Vector2(0,y);
            posX += LeftRightSpeed;
            if (posX > (pos.x + len))
            {
                speed = LeftRightSpeed - (posX - (pos.x + len));
            }
            m_moveVectors.Enqueue(new MoveInfo(mov, speed));
			cnt--;			
		}
        cnt = (int)(m_CellSide/LeftRightSpeed);
        while(cnt > 0)
        {
            m_moveVectors.Enqueue(new MoveInfo(m_DownVector, speed));
            cnt--;
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
		float len = dist.magnitude+1; // высота прыжка
		int N = (int)(len / JumpSpeed); // кол-во кадров для прыжка вверх
		int cnt = N * 2; // кол-во кадров для всего прыжка
		while(cnt > 0)
		{
			if(cnt > N) // если движение вверх (первая фаза)
				m_moveVectors.Enqueue(new MoveInfo(m_UpVector,JumpSpeed));
			else // движение вниз
				m_moveVectors.Enqueue(new MoveInfo(m_DownVector,JumpSpeed));
			cnt--;
		}
	}
	// Update is called once per frame
	void FixedUpdate () 
	{
        m_State.Update(); // Делегируем обработку текущему состоянию        

		if (Input.GetKey(KeyCode.UpArrow)){
            m_State.Up(); // Делегируем обработку текущему состоянию
		}
		if(Input.GetKey(KeyCode.DownArrow)){
            m_State.Down(); // Делегируем обработку текущему состоянию
		}
		if (Input.GetKey (KeyCode.RightArrow)){
            m_State.Right(); // Делегируем обработку текущему состоянию
		}
        if (Input.GetKey(KeyCode.LeftArrow)){
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
        switch (shelfType)
        {
            case ShelfType.Ice:
                if (m_State.type == BallStateType.TO_LEFT)
                {
                    SetState(BallStateType.JUMP);
                    LeftKey(2);
                    return;
                }
                if (m_State.type == BallStateType.TO_RIGHT) 
                {
                    SetState(BallStateType.JUMP);
                    RightKey(2);
                    return;
                }                
            break;
        }
		m_State.OnGround(shelfType); // Делегируем обработку текущему состоянию
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
