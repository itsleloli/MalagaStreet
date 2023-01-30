using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
public class Movement : MonoBehaviour
{
    [Header("Movement")]
    private float moveSpeed;
    public float walkSpeed;
    public float sprintSpeed;
    public float slideSpeed;
    public float wallrunSpeed;
    public float climbSpeed;

    private float desiredMoveSpeed;
    private float lastDesiredMoveSpeed;

    public float speedIncreaseMultiplier;
    public float slopeIncreaseMultiplier;

    public float groundDrag;
    private bool canSprint;

    [Header("JumpValues")]
    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplier;
    private bool readyToJump = true;
    private bool canJump;

    [Header("Crouching")]
    public float crouchSpeed;
    public float crouchYScale;
    private float startYScale;
    private bool canCrouch;

    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask whatIsGround;
    public bool grounded;
    public bool isStunned;

    [Header("Slope Handling")]
    public float maxSlopeAngle;
    private RaycastHit slopeHit;
    private bool exitingSlope;

    [Header("References")]
    public Climbing climbingScript;
    [SerializeField] private Animator anim;
    private WallRunning wallRunning;

    public Transform orientation;

    private float horizontalInput;
    private float verticalInput;

    private Vector3 moveDirection;

    private Rigidbody rb;

    public MovementState state;

    public enum MovementState
    {
        walking,
        sprinting,
        wallrunning,
        climbing,
        crouching,
        sliding,
        air,
    }

    public bool sliding;
    public bool wallrunning;
    public bool climbing;

    #region Input
    private void OnMove(InputValue moveDir)
    {
        Vector2 moveInput = moveDir.Get<Vector2>();
        horizontalInput = moveInput.x;
        verticalInput = moveInput.y;
    }

    private void OnJumpDown()
    {
        canJump = true;
    }

    private void OnJumpUp()
    {
        canJump = false;
    }

    private void OnCrouchDown()
    {
        //if (MainMenu.Instance.m_gameStarted)
        //{
        //    if (!this.enabled) return;

        //    transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
        //    rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);

        //    canCrouch = true;
        //}
    }

    private void OnCrouchUp()
    {
        //if (MainMenu.Instance.m_gameStarted)
        //{
        //    transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);

        //    canCrouch = false;
        //}
    }

    private void OnSprintDown()
    {
        canSprint = true;
    }
    private void OnSprintUp()
    {
        canSprint = false;
    }

    #endregion
    private void Start()
    {
        wallRunning = GetComponent<WallRunning>();
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        startYScale = transform.localScale.y;
    }

    private void Update()
    {
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, whatIsGround);

        SpeedControl();
        StateHandler();
        AnimationHandler();

        if (canJump && readyToJump && grounded)
        {
            readyToJump = false;

            Jump();

            Invoke(nameof(ResetJump), jumpCooldown);
        }

        if (isStunned)
        {
            rb.drag = 0;
            Invoke("SetStun", 2);
        }
        else if (grounded)
            rb.drag = groundDrag;
        else
            rb.drag = 0;
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }

    private void SetStun()
    {
        isStunned = false;
    }

    private void StateHandler()
    {
        if (climbing)
        {
            state = MovementState.climbing;
            desiredMoveSpeed = climbSpeed;
        }

        else if (wallrunning)
        {
            state = MovementState.wallrunning;
            desiredMoveSpeed = wallrunSpeed;
        }

        if (sliding)
        {
            state = MovementState.sliding;

            if (OnSlope() && rb.velocity.y < 0.1f)
                desiredMoveSpeed = slideSpeed;
            else
                desiredMoveSpeed = sprintSpeed;
        }

        else if (canCrouch)
        {
            state = MovementState.crouching;
            desiredMoveSpeed = crouchSpeed;
        }

        else if (grounded && canSprint)
        {
            state = MovementState.sprinting;
            desiredMoveSpeed = sprintSpeed;
        }

        else if (grounded)
        {
            state = MovementState.walking;
            desiredMoveSpeed = walkSpeed;
        }

        else
        {
            state = MovementState.air;
        }

        if (Mathf.Abs(desiredMoveSpeed - lastDesiredMoveSpeed) > 4 && moveSpeed != 0)
        {
            StopAllCoroutines();
            StartCoroutine(SmoothlyLerpMoveSpeed());
        }
        else
        {
            moveSpeed = desiredMoveSpeed;
        }

        lastDesiredMoveSpeed = desiredMoveSpeed;
    }

    private void AnimationHandler()
    {
        if (climbing)
        {
            anim.SetBool("inAir", false);
            anim.SetBool("inClimb", true);
        }
        else
            anim.SetBool("inClimb", false);

        if (state == MovementState.air && !climbing)
            anim.SetBool("inAir", true);
        else
            anim.SetBool("inAir", false);

        if (wallRunning.wallLeft)
        {
            anim.SetBool("WallRunLeft", true);
        }
        else
            anim.SetBool("WallRunLeft", false);


        if (wallRunning.wallRight)
        {
            anim.SetBool("WallRunRight", true);
        }
        else
            anim.SetBool("WallRunRight", false);

        switch (state)
        {
            case MovementState.walking:
                if (verticalInput == 0 && horizontalInput == 0)
                    anim.SetFloat("walkSpeed", 0);
                else
                    anim.SetFloat("walkSpeed", 1);
                break;
            case MovementState.sprinting:
                anim.SetFloat("walkSpeed", 3);
                break;
            case MovementState.wallrunning:
                break;
            case MovementState.climbing:
                break;
            case MovementState.crouching:
                if (verticalInput == 0 && horizontalInput == 0)
                    anim.SetFloat("walkSpeed", 0);
                else
                    anim.SetFloat("walkSpeed", 1);
                break;
            case MovementState.sliding:
                break;
            case MovementState.air:
                break;
            default:
                anim.SetBool("inAir", false);
                anim.SetFloat("walkSpeed", 0);
                break;
        }
    }

    private IEnumerator SmoothlyLerpMoveSpeed()
    {
        float time = 0;
        float difference = Mathf.Abs(desiredMoveSpeed - moveSpeed);
        float startValue = moveSpeed;

        while (time < difference)
        {
            moveSpeed = Mathf.Lerp(startValue, desiredMoveSpeed, time / difference);

            if (OnSlope())
            {
                float slopeAngle = Vector3.Angle(Vector3.up, slopeHit.normal);
                float slopeAngleIncrease = 1 + (slopeAngle / 90f);

                time += Time.deltaTime * speedIncreaseMultiplier * slopeIncreaseMultiplier * slopeAngleIncrease;
            }
            else
                time += Time.deltaTime * speedIncreaseMultiplier;

            yield return null;
        }

        moveSpeed = desiredMoveSpeed;
    }

    private void MovePlayer()
    {
        if (climbingScript.exitingWall)
            return;

        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        if (OnSlope() && !exitingSlope)
        {
            rb.AddForce(GetSlopeMoveDirection(moveDirection) * moveSpeed * 20f, ForceMode.Force);

            if (rb.velocity.y > 0)
                rb.AddForce(Vector3.down * 80f, ForceMode.Force);
        }

        if (grounded)
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);

        else if (!grounded)
            rb.AddForce(moveDirection.normalized * airMultiplier * 10, ForceMode.Force);

        if (!wallrunning)
            rb.useGravity = !OnSlope();
    }

    private void SpeedControl()
    {
        if (OnSlope() && !exitingSlope)
        {
            if (rb.velocity.magnitude > moveSpeed)
                rb.velocity = rb.velocity.normalized * moveSpeed;
        }

        else
        {
            Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

            if (flatVel.magnitude > moveSpeed)
            {
                Vector3 limitedVel = flatVel.normalized * moveSpeed;
                rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
            }
        }
    }

    private void Jump()
    {
        exitingSlope = true;

        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }

    private void ResetJump()
    {
        readyToJump = true;

        exitingSlope = false;
    }

    public bool OnSlope()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight * 0.5f + 0.3f))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle < maxSlopeAngle && angle != 0;
        }

        return false;
    }

    public Vector3 GetSlopeMoveDirection(Vector3 direction)
    {
        return Vector3.ProjectOnPlane(direction, slopeHit.normal).normalized;
    }
}
