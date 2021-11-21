using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    CharacterController playerController;

    [Range(0, 40)]
    [SerializeField] float speed;

    [Range(-60, 0)]
    [SerializeField] float gravity = -9.81f;
    [SerializeField] float jumpHeight = 5f;
    Vector3 velocity;

    [SerializeField] Transform groundCheck;
    [SerializeField] float groundCheckRadius;
    [SerializeField] LayerMask groundMask;
    bool isGrounded;

    // Start is called before the first frame update
    void Start()
    {
        playerController = GetComponent<CharacterController>();
    }

    // Update is called once per frame
    void Update()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundMask);

        if (isGrounded && velocity.y < 0f)
        {
            velocity.y = -2f;
        }

        float xInput = Input.GetAxis("Horizontal");
        float zInput = Input.GetAxis("Vertical");

        Vector3 movement = transform.right * xInput + transform.forward * zInput;
        playerController.Move(movement * speed * Time.deltaTime);

        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        velocity.y += gravity * Time.deltaTime;
        playerController.Move(velocity * Time.deltaTime);
    }
}
