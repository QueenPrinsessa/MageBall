using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MageBall
{
    public class MenuButtonController : MonoBehaviour
    {

        private bool hasKeyBeenPressed;
        private bool isMenuLocked;
        private int maxIndex;
        private readonly SortedDictionary<int, MenuButton> menuButtons = new SortedDictionary<int, MenuButton>();
        private readonly float menuLockDuration = 0.1f;
        private float menuLockTimer;

        public int Index { get; set; } = 0;
        public bool Interactable { get; private set; } = true;

        private void Start()
        {
            MenuButton[] menuButtons = GetComponentsInChildren<MenuButton>();

            int currentMaxIndex = 0;
            foreach (MenuButton menuButton in menuButtons)
            {
                if (menuButton.Index > currentMaxIndex)
                    currentMaxIndex = menuButton.Index;
                this.menuButtons.Add(menuButton.Index, menuButton);
                menuButton.ButtonSelected += OnButtonSelected;
            }

            SetTopButtonAsSelected();

            maxIndex = currentMaxIndex;
        }

        private void OnButtonSelected()
        {
            if (!Interactable)
                return;

            menuLockTimer = menuLockDuration;
            DisableInteraction();
            isMenuLocked = true;
        }

        private void SetTopButtonAsSelected()
        {
            foreach (MenuButton menuButton in menuButtons.Values)
            {
                    if (menuButton.Selectable)
                    {
                        Index = menuButton.Index;
                        break;
                    }
            }
        }

        private void Update()
        {
            if (menuLockTimer > 0)
                menuLockTimer -= Time.deltaTime;
            else if (!Interactable && isMenuLocked)
            {
                isMenuLocked = false;
                EnableInteraction();
            }

            if (!Interactable)
                return;

            float verticalAxis = Input.GetAxisRaw("Vertical");

            if(!IsOnAvailableMenuButton(verticalAxis))
                return;

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

                if (menuButtons.ContainsKey(Index) && menuButtons[Index].Selectable)
                    hasKeyBeenPressed = true;
            }
            else
                hasKeyBeenPressed = false;
        }

        private bool IsOnAvailableMenuButton(float verticalAxis)
        {
            if (menuButtons.ContainsKey(Index))
            {
                if (!menuButtons[Index].Selectable && verticalAxis == 0)
                    SetTopButtonAsSelected();

                return true;
            }
            else
            {
                if (Index < maxIndex && verticalAxis < 0)
                    Index++;
                else if (Index < maxIndex && verticalAxis > 0)
                    Index--;
                else
                    Index = 0;

                if (menuButtons.ContainsKey(Index) && menuButtons[Index].Selectable)
                    hasKeyBeenPressed = true;
                return hasKeyBeenPressed;
            }
        }

        public void DisableInteraction()
        {
            foreach (MenuButton menuButton in menuButtons.Values)
                menuButton.Interactable = false;

            Interactable = false;
        }

        public void EnableInteraction()
        {
            foreach (MenuButton menuButton in menuButtons.Values)
                menuButton.Interactable = true;

            Interactable = true;
        }

        public void DeactivateButtons()
        {
            isMenuLocked = false;
            foreach (MenuButton menuButton in menuButtons.Values)
            {
                menuButton.Selectable = false;
                menuButton.Interactable = false;
            }

            Interactable = false;
        }

        public void ActivateButtons()
        {
            isMenuLocked = false;
            foreach (MenuButton menuButton in menuButtons.Values)
            {
                menuButton.Selectable = true;
                menuButton.Interactable = true;
            }

            Interactable = true;
        }
    }
}