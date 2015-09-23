using UnityEngine;
using System.Collections;

public class SlidingDoors : MonoBehaviour{
    // Use this for initialization
    Shelf m_shelf;
    Ball m_ball;
    public enum SlideDoorType
    {
        Top,
        Down
    }
    void Start() {

        if (m_ball == null)
        {
            m_ball = GameObject.Find("Ball").GetComponent<Ball>();
        }
    }

    // Update is called once per frame
    void Update() {
    }
    void OnTriggerEnter2D(Collider2D inCollider)
    {
       
    }
}
