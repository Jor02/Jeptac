using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spinning : MonoBehaviour
{
    public float speed = 10f;

    void Update()
    {
        transform.eulerAngles -= transform.up * speed * Time.deltaTime;
    }
}
