using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Player Sprite")]
    public Animator anim;
    public Transform spriteTransform;

    [Header("Gameplay")]
    public float standUpSpeed = 10;
    [Space(10)]
    public float speed = 1;
    public float rotationSpeed = 10;
    [Space(10)]
    public float pathFollowSpeed = 50;
    public float pathLaunchSpeed = 70;
    public float pathInterval = 10;
    [Space(10)]
    public Collider2D pathDrawerCol;
    public Vector3 pathStartOffset;
    [Space(5)]
    public LayerMask groundMask;

    [Header("Particles/Effects")]
    public PathDrawer pathDrawer;
    public GameObject smokePrefab;
    public ParticleSystem smokeTrail;
    private SmokeEffect smokeEffect;

    /* private fields */
    BoxCollider2D boxCol;
    private Rigidbody2D rb;
    private bool isGrounded = false;
    private bool isStanding = false;
    private bool isFalling = false;
    private bool isLaunching;
    private bool shouldLaunch = false;
    private ContactFilter2D contactFilter = new ContactFilter2D();

    private float standingTimer = 1;

    private float pathTimer = 0;
    private List<Vector3> targetPath = new List<Vector3>();

    private bool shouldFollowPath = false;
    private float nextAngle = 0;
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

        //Make player stand up
        if (isGrounded)
        {
            if (rb.velocity.magnitude <= 0.1f)
            {
                if (standingTimer <= 0) {
                    standingTimer = 1;
                    isStanding = true; 
                }
                else standingTimer -= Time.deltaTime * standUpSpeed;
            }
            else standingTimer = 1;
        }
        else isStanding = false;

        if (isStanding) isFalling = false;

        if (isStanding)
        {
            transform.rotation = Quaternion.identity;

            if (Input.GetKey(KeyCode.Space) && !shouldLaunch)
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

                    if (pathDrawerCol.Cast(pathDrawer.transform.up, contactFilter, new RaycastHit2D[1], 0.03f) > 0)
                    {
                        shouldLaunch = true;
                    }
                }
            }
            else if (isLaunching)
            {
                isLaunching = false;
                shouldLaunch = false;

                smokeTrail.Play(true);

                shouldFollowPath = true;
                pointTimer = 0;

                rb.bodyType = RigidbodyType2D.Static;

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

                    Vector2 pointOffset = nextPoint - prevPoint;
                    nextAngle = Mathf.Atan2(pointOffset.y, pointOffset.x) * Mathf.Rad2Deg;

                    targetPath.RemoveAt(0);
                }
                else
                {
                    shouldFollowPath = false;
                    rb.bodyType = RigidbodyType2D.Dynamic;

                    Vector3 angle = (nextPoint - prevPoint).normalized;
                    rb.AddForce(angle * pathLaunchSpeed * 4);

                    anim.SetTrigger("isFalling");
                    smokeTrail.Stop(true, ParticleSystemStopBehavior.StopEmitting);
                }
            }

            pointTimer -= Time.deltaTime * pathFollowSpeed;

            //Lerp to next point
            Vector3 newPos = Vector3.Lerp(prevPoint, nextPoint, 1 - pointTimer);
            newPos.z = transform.position.z;
            transform.position = newPos;

            //Rotate to next point
            transform.rotation = Quaternion.Euler(0, 0, nextAngle - 90);
        }

        SetAnims();
    }

    void CheckGrounded()
    {
        isGrounded = shouldFollowPath ? false : boxCol.Cast(Vector3.down, contactFilter, new RaycastHit2D[1], 0.03f) > 0;
    }

    void SetAnims()
    {
        //Set Animator states
        anim.SetBool("isLaunching", isLaunching);
        anim.SetBool("isStanding", isStanding);
        anim.SetBool("isGrounded", isGrounded);

        if (!shouldFollowPath) {
            if (!isStanding && !isFalling)
            {
                spriteTransform.localRotation = Quaternion.identity;
                /*
                //Rotate player towards our current velocity
                Vector3 curRot = spriteTransform.rotation.eulerAngles;
                curRot.z = Mathf.Atan2(rb.velocity.x, rb.velocity.y) * Mathf.Rad2Deg;
                spriteTransform.rotation = Quaternion.Euler(curRot);
                */
            } else if (!isFalling)
            {
                spriteTransform.rotation = Quaternion.identity;
            } else
            {
                spriteTransform.localRotation = Quaternion.identity;
            }
        } else
        {
            spriteTransform.localRotation = Quaternion.identity;
        }
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
        Gizmos.color = Color.green;
        Gizmos.DrawLine(prevPoint, nextPoint);

        foreach (Vector3 point in targetPath)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(point, 0.01f);
        }
    }
}
