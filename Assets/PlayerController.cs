using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public Animator anim;
    public Transform spriteTransform;

    public GameObject smokePrefab;
    private SmokeEffect smokeEffect;

    private bool prevInput;
    private Rigidbody2D rb;

    private bool isGrounded = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.Space) && !prevInput)
        {
            prevInput = true;
            CreateSmoke();
            anim.SetBool("isLaunching", true);
        }
        else if (!Input.GetKey(KeyCode.Space) && prevInput)
        {
            prevInput = false;
            rb.AddForce(transform.up * 600f);
            anim.SetBool("isStanding", false);
            anim.SetBool("isGrounded", false);
            anim.SetBool("isLaunching", false);

            DestroySmoke();
        }

        Vector3 curRot = spriteTransform.rotation.eulerAngles;
        Vector2 normalizedVel = rb.velocity.normalized;
        curRot.z = Mathf.Atan2(normalizedVel.x, normalizedVel.y) * Mathf.Rad2Deg;
        spriteTransform.rotation = Quaternion.Euler(curRot);
    }

    void CreateSmoke()
    {
        DestroySmoke();
        smokeEffect = Instantiate(smokePrefab).GetComponent<SmokeEffect>();
        smokePrefab.transform.position = transform.position;
        smokePrefab.transform.rotation = spriteTransform.rotation;
    }

    void DestroySmoke()
    {
        if (smokeEffect != null)
            smokeEffect.Destroy();
    }
}
