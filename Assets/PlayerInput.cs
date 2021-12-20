using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    InputSystem inputSystem;
    Animator animator;
    CharacterController controller;
    Vector3 moveDirection;
    
    [Header("User Input")]
    public Vector2 movementInput;
    public bool jumpInput;
    public bool attackInput;

    [Header("Variables")]
    public float moveSpeed = 5f;
    public float jumpForce = 20f;
    public float gravityScale;

    bool canAttack;
    private void OnEnable() 
    {
        if(inputSystem == null)
        {
            inputSystem = new InputSystem();
            inputSystem.PlayerActions.Movement.performed += i => movementInput = i.ReadValue<Vector2>();
            inputSystem.PlayerActions.Jump.performed += i => jumpInput = true;
            inputSystem.PlayerActions.Jump.canceled += i => jumpInput = false;
            inputSystem.PlayerActions.Attack.performed += i => attackInput = true;
            inputSystem.PlayerActions.Attack.canceled += i => attackInput = false;
        }
        inputSystem.Enable();
    }
    private void OnDisable() 
    {
        inputSystem.Disable();
    }

    private void Awake() 
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        canAttack = animator.GetBool("CanAttack");
        HandleMovement();
        HandleAttack();
        HandleJump();
    }

    Vector3 PoolInput()
    {   
        Vector3 r = default(Vector3);
        r.x = movementInput.x;
        r.y = movementInput.y;
        return r.normalized;
    }

    public void HandleMovement()
    {
        Vector3 input = PoolInput();
        moveDirection = new Vector3(input.x * moveSpeed, moveDirection.y, input.y * moveSpeed);

        moveDirection.y += (Physics.gravity.y * gravityScale);
        controller.Move(moveDirection * Time.deltaTime);

        animator.SetFloat("V", Mathf.Abs(input.y), .1f, Time.deltaTime);
        animator.SetFloat("H", Mathf.Abs(input.x), .1f, Time.deltaTime);
    }

    public void HandleJump()
    {
        if(controller.isGrounded)
        {
            if(jumpInput)
            {
                jumpInput = false;
                moveDirection.y = jumpForce;
            }
        }
    }

    public void HandleAttack()
    {
        if(attackInput && canAttack)
        {
            attackInput = false;
            disableAttack();
            int _count = animator.GetInteger("Count");
            string attackStr = "Attack" + _count.ToString();
            Debug.Log(attackStr);
            animator.CrossFade(attackStr, 0f);
            SetCount(_count + 1);
        }
        else if(animator.GetInteger("Count") == 0)
            animator.SetBool("CanAttack", true);
            
    }

    public void SetCount(int num)
    {
        animator.SetInteger("Count", num);
    }

    public void enableAttack()
    {
        animator.SetBool("CanAttack", true);
    }
    public void disableAttack()
    {
        animator.SetBool("CanAttack", false);
    }
}
