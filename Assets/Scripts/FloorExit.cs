using UnityEngine;
using System.Collections;

public class FloorExit : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
    void OnTriggerEnter2D(Collider2D inCollider)
    {
        Application.LoadLevel("Start");
    }
}
