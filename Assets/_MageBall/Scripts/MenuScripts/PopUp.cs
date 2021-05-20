using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PopUp : MonoBehaviour
{
    [SerializeField] private GameObject popUpCanvas;

    public bool IsOpen => popUpCanvas.activeInHierarchy;

    public event Action PopUpOpened;
    public event Action PopUpClosed;

    private void Update()
    {
        if (!IsOpen)
            return;

        if (Input.GetButtonDown("Cancel"))
            Close();
    }

    public void Open()
    {
        popUpCanvas.SetActive(true);
        PopUpOpened?.Invoke();
    }

    public void Close()
    {
        popUpCanvas.SetActive(false);
        PopUpClosed?.Invoke();
    }
}
