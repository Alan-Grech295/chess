using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserInput : MonoBehaviour
{
    private bool isDragging;
    private BoardUI boardUI;

    private Vector2 mousePos;
    // Start is called before the first frame update
    void Start()
    {
        boardUI = GetComponent<BoardUI>(); 
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            isDragging = true;
            mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            boardUI.StartDrag(mousePos);
        }

        if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
            mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            boardUI.DropPiece(mousePos);
        }

        if (isDragging)
        {
            mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            boardUI.DragPiece(mousePos);
        }
    }
}
