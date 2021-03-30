using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuButtonController : MonoBehaviour
{

    private bool hasKeyBeenPressed;
    private int maxIndex;

    public int Index { get; set; } = 0;

    private void Start()
    {
        MenuButton[] menuButtons = GetComponentsInChildren<MenuButton>();

        maxIndex = menuButtons.Length-1;
    }

    private void Update()
    {
        float verticalAxis = Input.GetAxisRaw("Vertical");

        if (verticalAxis != 0)
        {
            if (hasKeyBeenPressed)
                return;

            if (verticalAxis < 0)
            {
                if (Index < maxIndex)
                    Index++;
                else
                    Index = 0;
            }
            else
            {
                if (Index > 0)
                    Index--;
                else
                    Index = maxIndex;
            }

            hasKeyBeenPressed = true;
        }
        else
            hasKeyBeenPressed = false;
    }
}
