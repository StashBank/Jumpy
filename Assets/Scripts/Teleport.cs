using UnityEngine;
using System.Collections;

public class Teleport : Shelf {
    public Teleport TargetTeleport;
    // Use this for initialization
    void Start () {
        if (ball == null)
        {
            ball = GameObject.Find("Ball").GetComponent<Ball>();
        }
    }
	
	// Update is called once per frame
	void Update () {
	
	}
    void OnTriggerEnter2D(Collider2D inCollider)
    {
        ball.SetState(Ball.BallStateType.HIDE);
        Vector3 target = TargetTeleport.transform.position;
        target.y += ball.GetComponent<SpriteRenderer>().bounds.extents.y + 1f;
        ball.transform.position = target;
        ball.SetState(Ball.BallStateType.SHOW);
        ball.SetState(Ball.BallStateType.IN_AIR);
    }
}
