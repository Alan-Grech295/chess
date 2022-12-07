using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UserInput;

public class UserInput : MonoBehaviour
{
    [HideInInspector]
    public bool isDragging;
    [HideInInspector]
    public Vector2 mousePos;
    [HideInInspector]
    public Vector2 clickPos;

    public enum ClickState { IDLE, CLICK, DRAG, RELEASE}
    [HideInInspector]
    public ClickState clickState = ClickState.IDLE;
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        if (Input.GetMouseButtonDown(0))
        {
            clickState = ClickState.CLICK;
            isDragging = true;
            clickPos = mousePos;
        }

        if (Input.GetMouseButtonUp(0))
        {
            clickState = ClickState.RELEASE;
            isDragging = false;
        }

        if(isDragging)
            clickState = ClickState.DRAG;
    }
}
