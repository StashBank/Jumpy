using UnityEngine;
using System.Collections;


public struct GameInfo
{
    static float m_CellSide = 10.0f;
    public static float cellSide { get { return m_CellSide; } }
}
public class Controls : MonoBehaviour {

    public GameObject levels;

	// Use this for initialization
	void Start () 
    {
        //Arrows:
        Arrows = GameObject.Find("Arrows");
        Transform[] arrows = new Transform[4]; // масив кнопок
        arrows[0] = Arrows.transform.Find("ArrowLeft"); //ищем кнопку по названию
        arrows[1] = Arrows.transform.Find("ArrowRight");
        arrows[2] = Arrows.transform.Find("ArrowUp");
        arrows[3] = Arrows.transform.Find("ArrowDown");
        
        foreach (Transform item in arrows)
        {
            Vector2 pos = item.position; //позиция кнопки
            Vector2 rect = item.renderer.bounds.size; //размер кнопки
            pos.x -= rect.x/2; //т.к. позиция в центре объекта то сдвигаем значения влево и вверх до угла
            pos.y -= rect.y/2;
            switch (item.name) //инициализация пространства кнопки (место где будет проверятся клик мыши)
            {                
                case "ArrowLeft":
                    ButtonLeftRect = new Rect(pos.x, pos.y, rect.x, rect.y);
                    break;
                case "ArrowRight":
                    ButtonRightRect = new Rect(pos.x, pos.y, rect.x, rect.y);
                    break;
                case "ArrowDown":
                    ButtonDownRect = new Rect(pos.x, pos.y, rect.x, rect.y);
                    break;
                case "ArrowUp":
                    ButtonUpRect = new Rect(pos.x, pos.y, rect.x, rect.y);
                    break;
            }            
        }
        //Joystick:
        //joystickPoint = Joystick.transform.Find("circle"); // большой круг
        //Transform JoystickMainCircle = Joystick.transform.Find("Circle_Grey");  // мал. круг (указатель мыши)
        //Vector3 JoystickMainCirclePosition = camera.WorldToScreenPoint(JoystickMainCircle.position);
        //float X = JoystickMainCirclePosition.x;
        //float Y = JoystickMainCirclePosition.y;        
        //joystickPosition = new Vector2(X, Y);        
       // R = 50.0f;
        //SpriteRenderer sr = JoystickMainCircle.GetComponent<SpriteRenderer>();
       // R = sr.bounds.size.x / 2;
	}

    public GameObject Arrows;
    public GameObject Joystick;
    Transform joystickPoint;
    //Vector2 joystickPosition;
    //float R = 0.0f;
	// Update is called once per frame
	void Update () 
    {
        if (Input.GetMouseButton(0))
        {
            Vector3 mousePos = Input.mousePosition;
            //Vector3 newJoystickPointPos = camera.ScreenToWorldPoint(mousePos) - joystickPoint.position;
            //newJoystickPointPos.z = 0;
            Vector2 mousePosV2 = new Vector2(mousePos.x,mousePos.y);
            
            //Vector3 v = Joystick.transform.position - camera.ScreenToWorldPoint(mousePos);
            //v.z = 0;
            //float distance = v.magnitude;
            //трансформируем координаты мыши относительно (0,0) джойстика
            /*Vector2 pos = mousePosV2 - joystickPosition;
            
            if (distance <= R)
            {
                if (v.magnitude > R * 0.25)
                {
                    if (pos.y > Mathf.Abs(pos.x)) //верхняя часть джойстика
                    {
                        ball.UpKey();
                    }
                    if (-pos.y > Mathf.Abs(pos.x)) //нижняя часть джойстика
                    {
                        ball.DownKey();
                    }
                    if (pos.x > Mathf.Abs(pos.y)) //правая часть джойстика
                    {
                        ball.RightKey();
                    }
                    if (-pos.x > Mathf.Abs(pos.y)) //левая часть джойстика
                    {
                        ball.LeftKey();
                    }
                }
            }
            else
            {
                //newJoystickPointPos = camera.ScreenToWorldPoint(joystickPosition) - joystickPoint.position;
                //newJoystickPointPos.z = 0;
            }
            //joystickPoint.Translate(newJoystickPointPos);
            */
            mousePosV2 = camera.ScreenToWorldPoint(mousePosV2);

            if (ButtonLeftRect.Contains(mousePosV2))
            {
                ball.LeftKey();
            }
            if (ButtonRightRect.Contains(mousePosV2))
            {
                ball.RightKey();
            }
            if (ButtonUpRect.Contains(mousePosV2))
            {
                ball.UpKey();
            }
            if (ButtonDownRect.Contains(mousePosV2))
            {
                ball.DownKey();
            }
        }
        if (Input.GetMouseButtonUp(0))
        {
            //Vector3 newJoystickPointPos = camera.ScreenToWorldPoint(joystickPosition) - joystickPoint.position;
            //newJoystickPointPos.z = 0;
            //joystickPoint.Translate(newJoystickPointPos);
        }
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            Application.Quit();
        }
	}

    public Ball ball;

    Rect ButtonLeftRect;
    Rect ButtonRightRect;
    Rect ButtonUpRect;
    Rect ButtonDownRect;    
}
