using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MageBall
{
    public class MenuButtonController : MonoBehaviour
    {
        [SerializeField, Tooltip("Default is a vertical menu")] private bool isHorizontalMenu = false;
        private bool hasKeyBeenPressed;
        private int maxIndex;
        private readonly SortedDictionary<int, MenuButton> menuButtons = new SortedDictionary<int, MenuButton>();
        private readonly float menuLockDuration = 0.1f;
        private Coroutine lockMenuRoutine;
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

            if (lockMenuRoutine != null)
                StopCoroutine(lockMenuRoutine);

            lockMenuRoutine = StartCoroutine(LockMenu());
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

        private IEnumerator LockMenu()
        {
            DisableInteraction();

            yield return new WaitForSeconds(menuLockDuration);

            EnableInteraction();
        }

        private void Update()
        {
            if (!Interactable)
                return;

            float axis = isHorizontalMenu ? -Input.GetAxisRaw("Horizontal") + -Input.GetAxisRaw("MenuHorizontal") : Input.GetAxisRaw("Vertical") + Input.GetAxisRaw("MenuVertical");

            if(!MoveToNextAvailableMenuButton(axis))
                return;

            if (axis != 0)
            {
                if (hasKeyBeenPressed)
                    return;

                if (axis < 0)
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

        /// <summary>
        /// Makes sure that the current index exists and otherwise selects the next available button
        /// </summary>
        /// <param name="axis"></param>
        /// <returns>Success</returns>
        private bool MoveToNextAvailableMenuButton(float axis)
        {
            if (menuButtons.ContainsKey(Index))
            {
                if (!menuButtons[Index].Selectable && axis == 0)
                    SetTopButtonAsSelected();

                return true;
            }
            else
            {
                if (Index < maxIndex && axis < 0)
                    Index++;
                else if (Index < maxIndex && axis > 0)
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
            foreach (MenuButton menuButton in menuButtons.Values)
            {
                menuButton.Selectable = false;
                menuButton.Interactable = false;
            }

            Interactable = false;
        }

        public void ActivateButtons()
        {
            foreach (MenuButton menuButton in menuButtons.Values)
            {
                menuButton.Selectable = true;
                menuButton.Interactable = true;
            }

            Interactable = true;
        }
    }
}