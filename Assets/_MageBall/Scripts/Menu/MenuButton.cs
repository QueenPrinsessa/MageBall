using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button), typeof(Animator))]
public class MenuButton : MonoBehaviour
{

    [SerializeField, Min(0),Tooltip("Button index. Starts from 0.")] private int index;
    private Button button;
    private MenuButtonController menuButtonController;
    private Animator animator;

    private void Start()
    {
        button = gameObject.GetComponent<Button>();
        animator = gameObject.GetComponent<Animator>();
        menuButtonController = gameObject.GetComponentInParent<MenuButtonController>();

        if (menuButtonController == null)
            Debug.LogError("There is no MenuButtonController in the parent object. MenuButton won't work!");
    }

    private void Update()
    {
        if (menuButtonController == null)
            return;

        if (menuButtonController.Index != index)
        {
            animator.SetBool("Selected", false);
            return;
        }

        animator.SetBool("Selected", true);

        if (Input.GetButtonDown("Submit") || Input.GetKeyDown(KeyCode.Mouse0))
        {
            animator.SetBool("Pressed", true);
        }
        else if (animator.GetBool("Pressed"))
        {
            animator.SetBool("Pressed", false);
        }

    }

    public void InvokeButtonClick()
    {
        button.onClick?.Invoke();
    }

    public void SelectButtonOnHover()
    {
        menuButtonController.Index = index;
    }
}
