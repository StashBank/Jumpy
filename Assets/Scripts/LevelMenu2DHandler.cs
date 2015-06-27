using UnityEngine;
using System.Collections;

public class LevelMenu2DHandler : MonoBehaviour {

	// Use this for initialization
	void Start () {
        
	}

    void Awake()
    {
        InitLevelMenu2dHandlers();
    }
	
	// Update is called once per frame
	void Update () {
	
	}

    void InitLevelMenu2dHandlers()
    {
        LevelMenu2D.I.OnItemClicked += HandleOnItemClicked;
        SwipeDetector.OnSwipeLeft += HandleOnSwipeLeft;
        SwipeDetector.OnSwipeRight += HandleOnSwipeRight;
    }

    void HandleOnItemClicked(int itemIndex, GameObject itemObject)
    {
        int levelIndex = (itemIndex + 1);
        Application.LoadLevel("Mission" + levelIndex);
    }
    void HandleOnSwipeRight()
    {
        LevelMenu2D.I.gotoBackItem();
    }
    void HandleOnSwipeLeft()
    {
        LevelMenu2D.I.gotoNextItem();
    }
}
