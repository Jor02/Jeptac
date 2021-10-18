using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Player Sprite")]
    public Animator anim;
    public Transform spriteTransform;

    [Header("Gameplay")]
    public float speed = 1;
    public float rotationSpeed = 10;
    public float pathFollowSpeed = 10;
    public float pathInterval = 10;
    public Vector3 pathStartOffset;
    public LayerMask groundMask;

    [Header("Particles/Effects")]
    public PathDrawer pathDrawer;
    public GameObject smokePrefab;
    private SmokeEffect smokeEffect;

    /* private fields */
    BoxCollider2D boxCol;
    private Rigidbody2D rb;
    private bool isGrounded = false;
    private bool isStanding = false;
    private bool isLaunching;
    private ContactFilter2D contactFilter = new ContactFilter2D();

    private float pathTimer = 0;
    private List<Vector3> targetPath = new List<Vector3>();

    private bool shouldFollowPath = false;
    private Vector3 prevPoint = Vector3.negativeInfinity;
    private Vector3 nextPoint = Vector3.negativeInfinity;
    private float pointTimer = 0;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        boxCol = GetComponent<BoxCollider2D>();

        contactFilter.SetLayerMask(groundMask);
    }

    void Update()
    {
        CheckGrounded();

        isStanding = isGrounded; // Needs to change

        if (isStanding)
        {
            if (Input.GetKey(KeyCode.Space))
            {
                if (!isLaunching) //Using this instead of Input.GetKeyDown to prevent input not registering
                {
                    isLaunching = true;

                    pathTimer = 0;
                    targetPath.Clear();
                    Quaternion rot = Quaternion.AngleAxis(transform.eulerAngles.z, Vector3.up);
                    Vector3 offset = rot * pathStartOffset;
                    pathDrawer.transform.position = transform.position + offset;
                    pathDrawer.transform.up = transform.up;

                    //Effects
                    CreateSmoke();
                    pathDrawer.StartCurve();
                }
                else
                {
                    //Make Path
                    float h = Input.GetAxis("Horizontal");

                    Vector3 rot = pathDrawer.transform.eulerAngles;
                    rot.z -= h * rotationSpeed * Time.deltaTime;
                    pathDrawer.transform.rotation = Quaternion.Euler(rot);

                    pathDrawer.transform.position += pathDrawer.transform.up * speed * Time.deltaTime;

                    //Add point to path
                    pathTimer -= Time.deltaTime * pathInterval;
                    if (pathTimer <= 0)
                    {
                        pathTimer = 1;
                        targetPath.Add(pathDrawer.transform.position);
                    }
                }
            }
            else if (isLaunching)
            {
                isLaunching = false;

                shouldFollowPath = true;
                pointTimer = 0;

                rb.bodyType = RigidbodyType2D.Static;
                targetPath.Add(pathDrawer.transform.position);

                //Effects
                DestroySmoke();
                pathDrawer.StopCurve();
            }
        }

        if (shouldFollowPath)
        {
            if (pointTimer <= 0)
            {
                if (targetPath.Count > 0)
                {
                    pointTimer = 1f;
                    nextPoint = targetPath[0];
                    prevPoint = transform.position;
                    targetPath.RemoveAt(0);
                }
                else
                {
                    shouldFollowPath = false;
                    rb.bodyType = RigidbodyType2D.Dynamic;
                    Vector3 angle = (nextPoint - prevPoint).normalized;
                    rb.AddForce(angle * pathFollowSpeed * 4);
                    anim.SetTrigger("isFalling");
                }
            }

            pointTimer -= Time.deltaTime * pathFollowSpeed;
            transform.position = Vector3.Lerp(prevPoint, nextPoint, 1 - pointTimer);
        }

        SetAnims();
    }

    void CheckGrounded()
    {
        isGrounded = boxCol.Cast(-transform.up, contactFilter, new RaycastHit2D[1], 0.03f) > 0;
    }

    void SetAnims()
    {
        //Set Animator states
        anim.SetBool("isLaunching", isLaunching);
        anim.SetBool("isStanding", isStanding);
        anim.SetBool("isGrounded", isGrounded);

        //Rotate player towards our current velocity
        Vector3 curRot = spriteTransform.rotation.eulerAngles;
        Vector2 normalizedVel = rb.velocity.normalized;
        curRot.z = Mathf.Atan2(normalizedVel.x, normalizedVel.y) * Mathf.Rad2Deg;
        spriteTransform.rotation = Quaternion.Euler(curRot);
    }

    void CreateSmoke()
    {
        DestroySmoke();
        smokeEffect = Instantiate(smokePrefab).GetComponent<SmokeEffect>();
        smokeEffect.target = spriteTransform;
    }

    void DestroySmoke()
    {
        if (smokeEffect != null)
        {
            smokeEffect.Destroy();
            smokeEffect = null;
        }
    }

    private void OnDrawGizmos()
    {
        foreach(Vector3 point in targetPath)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(point, 0.01f);
        }
    }
}
