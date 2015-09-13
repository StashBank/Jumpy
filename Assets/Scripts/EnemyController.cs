using UnityEngine;
using System.Collections;

public class EnemyController : MonoBehaviour {

    public GameObject enemy1;
    public Ball ball;
    public float speed = 1.0f;
    public Vector2 MinMax = Vector2.zero;
    public delegate void GameOverDelegate();
    static public event GameOverDelegate GameOver = delegate () { };
    bool isStart = false;
    public void ChangeMoving(bool isMove)
    {
        //gameObject.SetActive(isMove);
        isStart = isMove;
    }
    // Use this for initialization
    void Start () {
        ChangeMoving(false);
	}
	
	// Update is called once per frame
	void Update () {
        if (isStart)
        {
            enemy1.transform.position = new Vector3(
                MinMax.x + Mathf.PingPong(Time.time * speed, 1.0f) * (MinMax.y - MinMax.x),
                transform.position.y,
                transform.position.z
               );
        }
	}
    void OnTriggerEnter2D(Collider2D inCollider) {
        GameOver();
    }
}
