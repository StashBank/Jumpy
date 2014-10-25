using UnityEngine;
using System.Collections;

public class Wall : MonoBehaviour {

	public Ball ball;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnTriggerEnter2D(Collider2D collaider)
	{
		ball.OnWall();
	}
}
