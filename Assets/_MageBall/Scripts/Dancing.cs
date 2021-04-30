using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dancing : MonoBehaviour
{
    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (Input.GetKeyDown("j"))
        {
            animator.SetBool("Dance1", !animator.GetBool("Dance1"));
            animator.SetBool("Dance2", false);
            animator.SetBool("Dance3", false);
        }
        else if (Input.GetKeyDown("k"))
        {
            animator.SetBool("Dance1", false);
            animator.SetBool("Dance2", !animator.GetBool("Dance2"));
            animator.SetBool("Dance3", false);
        }
        else if (Input.GetKeyDown("l"))
        {
            animator.SetBool("Dance1", false);
            animator.SetBool("Dance2", false);
            animator.SetBool("Dance3", !animator.GetBool("Dance3"));
        }
    }

}
