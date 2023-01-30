using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Slide : MonoBehaviour
{
    [Header("References")]
    public Transform orientation;
    private Rigidbody rb;
    private Movement pm;

    [Header("Sliding")]
    public float maxSlideTime;
    public float slideForce;
    private float slideTimer;

    public float slideYScale;
    private float startYScale;

    [Header("Input")]
    private float horizontalInput;
    private float verticalInput;

    #region Input
    private void OnMove(InputValue moveDir)
    {
        Vector2 moveInput = moveDir.Get<Vector2>();
        horizontalInput = moveInput.x;
        verticalInput = moveInput.y;

    }

    private void OnSlideDown()
    {
        if (!PlayerManager.instance.m_menuOn)
        {
            if ((horizontalInput != 0 || verticalInput != 0))
                StartSlide();
        }
    }

    private void OnSlideUp()
    {
        if (!PlayerManager.instance.m_menuOn)
        {
            if (pm.sliding)
                StopSlide();
        }
    }
    #endregion

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        pm = GetComponent<Movement>();

        startYScale = transform.localScale.y;
    }

    private void FixedUpdate()
    {
        if (pm.sliding)
            SlidingMovement();
    }

    private void StartSlide()
    {
        pm.sliding = true;

        transform.localScale = new Vector3(transform.localScale.x, slideYScale, transform.localScale.z);
        rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);

        GetComponent<Movement>().jumpForce = 9;

        slideTimer = maxSlideTime;
    }

    private void SlidingMovement()
    {
        Vector3 inputDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        if (!pm.OnSlope() || rb.velocity.y > -0.1f)
        {
            rb.AddForce(inputDirection.normalized * slideForce, ForceMode.Force);

            slideTimer -= Time.deltaTime;
        }

        else
        {
            rb.AddForce(pm.GetSlopeMoveDirection(inputDirection) * slideForce, ForceMode.Force);
        }

        if (slideTimer <= 0)
            StopSlide();
    }

    private void StopSlide()
    {
        pm.sliding = false;
        GetComponent<Movement>().jumpForce = 7;
        transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
    }
}
