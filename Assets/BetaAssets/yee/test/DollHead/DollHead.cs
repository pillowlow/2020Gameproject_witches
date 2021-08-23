using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DollHead : MonoBehaviour
{
    Animator animator;
    void Start()
    {
        animator = GetComponent<Animator>();
    }
    void Break()
    {
        animator.SetTrigger("break");
    }
}
