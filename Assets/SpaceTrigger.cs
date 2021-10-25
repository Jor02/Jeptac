using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpaceTrigger : MonoBehaviour
{
    public Vector3 gravity;
    public float mass = 15f;
    public float multiplier = 0.2f;
    public Transform[] asteroids;

    private Rigidbody2D player;

    public static bool isSpace;
    public static Vector2 gravityDir;

    private void Awake()
    {
        gravity = Physics2D.gravity;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isSpace = true;
            Physics2D.gravity = Vector3.zero;
            player = collision.attachedRigidbody;
        }
    }

    private void Update()
    {
        float strength = 0;

        if (player != null)
            foreach (Transform asteroid in asteroids)
            {
                Vector2 direction = (Vector2)asteroid.position - player.position;
                Vector2 force = (mass / direction.sqrMagnitude * direction.normalized) * multiplier;
                player.AddForce(force);

                if (force.magnitude > strength)
                {
                    strength = force.magnitude;
                    gravityDir = force.normalized;
                }
            }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isSpace = false;
            Physics2D.gravity = gravity;
            player = null;
        }
    }
}
