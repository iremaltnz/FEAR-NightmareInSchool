using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Male : MonoBehaviour
{

    Animator animator;
    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey("w") || Input.GetKey("a") || Input.GetKey("d") || Input.GetKey("s"))
        {
            animator.SetBool("walk", true);
        }

        else if (!Input.GetKey("w"))
        {

            animator.SetBool("walk", false);
        }
    }
}
