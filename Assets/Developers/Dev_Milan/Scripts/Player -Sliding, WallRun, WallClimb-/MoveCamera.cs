using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveCamera : MonoBehaviour
{
    [SerializeField] private Transform _camerPos;
    public bool isDead;

    private void Update()
    {
        if(!isDead)
        transform.position = _camerPos.position;
    }
}
