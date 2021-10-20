using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectShake : MonoBehaviour
{
    private Vector3 origin;
    private Vector3 prev;
    private Vector3 next;

    private float speed;
    private float amplitude;

    private float timer;

    private bool isShaking = false;

    public void StartShake(float speed, float amplitude)
    {
        isShaking = true;

        this.speed = speed;
        this.amplitude = amplitude / 100;

        timer = 0;

        origin = transform.localPosition;
        next = Vector2.zero;
    }

    public void StopShake()
    {
        isShaking = false;
        transform.localPosition = origin;
    }

    void Update()
    {
        if (isShaking)
        {
            if (timer >= 1)
            {
                prev = next;
                next = Random.insideUnitCircle * amplitude;
                timer = 0;
            }

            transform.localPosition = origin + (Vector3)Vector2.Lerp(prev, next, timer);

            timer += Time.deltaTime * speed;
        }
    }
}
