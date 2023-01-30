using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Climbing : MonoBehaviour
{
    [Header("References")]
    public Transform orientation;
    public Rigidbody rb;
    public Movement pm;
    public LayerMask whatIsWall;

    [Header("Climbing")]
    public float climbSpeed;
    public float maxCilmbTime;
    private float climbTimer;

    private bool climbing;

    [Header("ClimbJumping")]
    public float climbJumpUpForce;
    public float climbJumpBackForce;

    public int climbJumps;
    private int climbJumpsLeft;

    [Header("ClimbBoost")]
    public float climbBoostForce;
    public float climbBoostFrontForce;

    [Header("Detection")]
    public float detectionLength;
    public float sphereCastRadius;
    public float maxWallLookAngle;
    private float wallLookAngle;

    private RaycastHit frontWallHit;
    private bool wallFront;

    private Transform lastWall;
    private Vector3 lastWallNormal;
    public float minWallNormalAngleChange;

    [Header("Exiting")]
    public bool exitingWall;
    public float exitWallTime;
    private float exitWallTimer;

    private Vector2 moveInput;
    private bool canClimb;

    #region Input

    private void OnMove(InputValue moveDir)
    {
        moveInput = moveDir.Get<Vector2>();
    }

    private void OnJump()
    {
        if (wallFront && climbJumps > 0)
            ClimbJump();
    }
    #endregion

    private void Update()
    {
        WallCheck();
        StateMachine();
        if (climbing && !exitingWall)
            ClimbingMovement();
    }

    private void StateMachine()
    {
        if (wallFront && moveInput.y > 0.1 && wallLookAngle < maxWallLookAngle && !exitingWall)
        {
            if (!climbing && climbTimer > 0)
                StartClimbing();

            if (climbTimer > 0)
                climbTimer -= Time.deltaTime;
            if (climbTimer < 0)
                StopClimbing();
        }

        else if (exitingWall)
        {
            if (climbing)
                StopClimbing();
            if (exitWallTimer > 0)
                exitWallTimer -= Time.deltaTime;
            if (exitWallTimer < 0)
                exitingWall = false;
        }

        else
        {
            if (climbing)
            {
                Vector3 forceToApply = transform.up * climbBoostForce + frontWallHit.normal * climbBoostFrontForce;
                rb.AddForce(forceToApply, ForceMode.Impulse);

                StopClimbing();
            }
        }
    }

    private void WallCheck()
    {
        wallFront = Physics.SphereCast(transform.position, sphereCastRadius, orientation.forward, out frontWallHit, detectionLength, whatIsWall);
        wallLookAngle = Vector3.Angle(orientation.forward, -frontWallHit.normal);

        bool newWall = frontWallHit.transform != lastWall || Mathf.Abs(Vector3.Angle(lastWallNormal, frontWallHit.normal)) > minWallNormalAngleChange;

        if ((wallFront && newWall) || pm.grounded)
        {
            climbTimer = maxCilmbTime;
            climbJumpsLeft = climbJumps;
        }
    }

    private void StartClimbing()
    {
        climbing = true;
        pm.climbing = true;

        lastWall = frontWallHit.transform;
        lastWallNormal = frontWallHit.normal;
    }

    private void ClimbingMovement()
    {
        rb.velocity = new Vector3(rb.velocity.x, climbSpeed, rb.velocity.z);
    }

    private void StopClimbing()
    {
        climbing = false;
        pm.climbing = false;
    }

    private void ClimbJump()
    {
        exitingWall = true;
        exitWallTimer = exitWallTime;

        Vector3 forceToApply = transform.up * climbJumpUpForce + frontWallHit.normal * climbJumpBackForce;

        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        rb.AddForce(forceToApply, ForceMode.Impulse);

        climbJumpsLeft--;
    }
}
