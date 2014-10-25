using UnityEngine;
using System.Collections;

public enum ShelfType { 
    Standart, // Обычная полочка
    Ice,  // Сколзящая (перемещает шарик далее по направлению, если не задать другое движение)
    DisappearByMultJumps, // Исчезает при достижении определенного кол-ва попаданий на полку
    DisappearByOneJumps, // Исчезает если на полке перейти в состояние прыгов
    Disappear, // Исчезает сразу. пропускает через себя (пустышка)
    Sticky, // Липкая полка (Вертикальная). Нужно продумать поведение шарика
    TowardsOn1CellsLeft, //Бросает шарик влево на 1 колонку
    TowardsOn1CellsRight, //Бросает шарик вправо на 1 колонку
    TowardsOn2CellsLeft, //Бросает шарик влево на 2 колонку
    TowardsOn2CellsRight, //Бросает шарик вправо на 2 колонку
    ChangeableToTowardsOn1Cells, //При вертикальных прижках обычная полка. при приге всторону изменяется на полку с типом TowardsOn1CellsLeft|TowardsOn1CellsRight
    DoubleSpike, //Шипы по обеем сторонам полки. Горизонтальная|Вертикальная
    BotomSpike, //Шипы снизу полки (Горизонтальная)
    PullUp,
    Cloud, //?????
    TELEPORT
}
public class Shelf : MonoBehaviour {

	Ball m_Ball;
    public ShelfType shelfType;
	// Use this for initialization
	void Start () 
	{
        if (m_Ball == null)
            m_Ball = GameObject.Find("Ball").GetComponent<Ball>();
	}
	
	// Update is called once per frame
	void Update () 
	{
	
	}

	void OnTriggerEnter2D(Collider2D inCollider)
	{
		//ToLeft();
		Vector2 inPos = inCollider.transform.position;
		Vector2 pos = this.transform.position;
		if(pos.y > inPos.y)
            m_Ball.OnCeiling(shelfType);
		else
            m_Ball.OnGround(shelfType);
	}	
}
