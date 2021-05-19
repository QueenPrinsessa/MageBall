using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MageBall
{

    [RequireComponent(typeof(Button), typeof(Animator))]
    public class MenuButton : MonoBehaviour
    {

        [SerializeField, Min(0), Tooltip("Button index. Starts from 0.")] private int index;
        [SerializeField] private UnityEvent buttonDeselected;
        [SerializeField] private UnityEvent buttonSelected;
        private Button button;
        private MenuButtonController menuButtonController;
        private Animator animator;
        public event Action ButtonSelected;

        public int Index => index;
        public bool Interactable { get; set; } = true;
        public bool Selectable { get; set; } = true;

        private void Start()
        {
            button = gameObject.GetComponent<Button>();
            animator = gameObject.GetComponent<Animator>();
            menuButtonController = gameObject.GetComponentInParent<MenuButtonController>();
            button.interactable = false;
            if (menuButtonController == null)
                Debug.LogError("There is no MenuButtonController in the parent object. MenuButton won't work!");
        }

        private void OnDisable()
        {
            buttonDeselected.Invoke();
        }

        private void Update()
        {
            if (menuButtonController == null)
                return;

            if (menuButtonController.Index != index)
                buttonDeselected.Invoke();

            if (menuButtonController.Index != index || !Selectable)
            {
                animator.SetBool("Selected", false);
                return;
            }

            animator.SetBool("Selected", true);
            buttonSelected.Invoke();

            if (!Interactable)
                return;

            if (Input.GetButtonDown("Submit") || Input.GetKeyDown(KeyCode.Mouse0) && EventSystem.current.currentSelectedGameObject == gameObject)
                animator.SetBool("Pressed", true);
            else if (animator.GetBool("Pressed"))
                animator.SetBool("Pressed", false);

        }

        //  Används i Unity för att triggera animation event
        public void InvokeButtonClick()
        {
            if (!Interactable)
                return;

            button.onClick?.Invoke();
        }

        // Den används i Unity inspectorn
        public void SelectMenuButton()
        {
            if (!Selectable)
                return;

            ButtonSelected?.Invoke();
            buttonSelected.Invoke();
            button.Select();
            menuButtonController.Index = index;
        }
    }
}