using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
namespace Assets.Scripts
{
    class PlayerController: MonoBehaviour
    {
        CharacterController controller;
        Animator animator;

        Vector3 velocity;
        bool isGrounded;

        public Transform ground;
        public float distance = 0.3f;

        public float speed;
        public float jumpHeight;
        public float gravity;

        public LayerMask mask;

        private void Start()
        {
            controller = GetComponent<CharacterController>();
            animator = GetComponent<Animator>();
        }

        private void Update()
        {
            #region Movement

            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");

            Vector3 move = transform.right * horizontal + transform.forward * vertical;
            controller.Move(move * speed * Time.deltaTime);

            #endregion

            #region Jumpp

            Debug.Log("Space basıldı");

            if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
            {

                Debug.Log("İf içinde");
                velocity.y += Mathf.Sqrt(jumpHeight * -3.0f * gravity);
            }
            #endregion

            #region Gravity
            isGrounded = Physics.CheckSphere(ground.position, distance, mask);

            if (isGrounded && velocity.y < 0)
            {
                velocity.y = 0f;
            }

            velocity.y += gravity * Time.deltaTime;
            controller.Move(velocity * Time.deltaTime);

            #endregion
            //if (Input.GetKey("w"))
            //{
            //    animator.SetBool("walk", true);
            //}

            //else 
            //{

            //    animator.SetBool("walk", false);
            //}
        }
    }
}
