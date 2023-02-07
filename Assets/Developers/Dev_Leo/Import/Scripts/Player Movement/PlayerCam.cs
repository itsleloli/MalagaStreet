using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCam : MonoBehaviour
{
    [SerializeField] private Camera cam;

    public float _sensX;
    public float _sensY;

    public Transform _orientation;
    public Transform _camHolder;

    private float _yRotation;
    private float _xRotation;

    private Vector2 lookInput;

    #region Input

    private void OnLook(InputValue lookDir)
    {
        lookInput = lookDir.Get<Vector2>();
    }
    #endregion

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        float mouseX = lookInput.x * Time.deltaTime * _sensX;
        float mouseY = lookInput.y * Time.deltaTime * _sensY;

        _yRotation += mouseX;

        _xRotation -= mouseY;
        _xRotation = Mathf.Clamp(_xRotation, -90f, 90f);

        _camHolder.rotation = Quaternion.Euler(_xRotation, _yRotation, 0);
        _orientation.rotation = Quaternion.Euler(0, _yRotation, 0);
    }   
}
