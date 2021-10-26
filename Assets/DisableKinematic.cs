using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisableKinematic : MonoBehaviour
{
    public AudioClip shatterSound;
    public Rigidbody[] rigidbodies;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        GetComponent<AudioSource>().PlayOneShot(shatterSound);

        foreach (Rigidbody rigidbody in rigidbodies)
        {
            rigidbody.isKinematic = false;
        }
    }
}
