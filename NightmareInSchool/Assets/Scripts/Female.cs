using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Female : MonoBehaviour
{
    // Start is called before the first frame update
    Animator animator;
    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey("w"))
        {
            animator.SetBool("walk", true);
        }

        else if (!Input.GetKey("w"))
        {

            animator.SetBool("walk", false);
        }
    }
}
