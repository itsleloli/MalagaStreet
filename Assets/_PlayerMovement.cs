using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.InputSystem;

public class _PlayerMovement : MonoBehaviour
{
    [SerializeField] float jumpForce;
    [SerializeField] TextMeshProUGUI debugText;
    [SerializeField] GameObject cam;

    bool gyroSupport;
    float horizontalInput;
    float verticalInput;

    public CalibrationTool calibrationTool;

    Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (SystemInfo.supportsGyroscope)
        {
            debugText.text = "Gyro is supported";
            Input.gyro.enabled = true;
            gyroSupport = true;
        }
        else
        {
            debugText.text = "Gyro is not supported";
            gyroSupport = false;
        }
    }

    private void Update()
    {
        if (gyroSupport)
        {
            cam.transform.localRotation = Input.gyro.attitude;

            float xrotation = Input.gyro.attitude.x + calibrationTool.xSlider.value;
            float yrotation = Input.gyro.attitude.y + calibrationTool.ySlider.value;
            float zrotation = Input.gyro.attitude.z + calibrationTool.zSlider.value;

            debugText.text = $"x: {Mathf.Round(xrotation)}, y: {Mathf.Round(yrotation)}, z: {Mathf.Round(zrotation)}";

            cam.transform.Rotate(xrotation - 90, yrotation, zrotation + 90, Space.World);
        }
    }

    public void Jump()
    {
        rb.velocity = transform.up * jumpForce;
    }

    public void Move(InputValue moveDir)
    {
        Vector2 moveInput = moveDir.Get<Vector2>();
        horizontalInput = moveInput.x;
        verticalInput = moveInput.y;

        rb.velocity = transform.position + new Vector3(horizontalInput, 0, verticalInput); 

    }


}


