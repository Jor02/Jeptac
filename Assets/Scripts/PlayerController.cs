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
    public float pathLengthTimerLength = 1f;
    public float pathFollowSpeed = 50;
    public float pathLaunchSpeed = 70;
    public float pathInterval = 10;
    [Space(10)]
    public ObjectShake cameraShake;
    [Space(10)]
    public Collider2D pathDrawerCol;
    public Vector3 pathStartOffset;
    [Space(5)]
    public LayerMask groundMask;

    [Header("Collider")]
    public float colliderFallingHeight = 0.47f;
    public float colliderFallingOffset = 0.47f;
    [Space(5)]
    public float colliderStandingHeight = 0.5623506f;
    public float colliderStandingOffset = 0.5623506f;

    [Header("Particles/Effects")]
    public PathDrawer pathDrawer;
    public GameObject smokePrefab;
    public ParticleSystem smokeTrail;
    private SmokeEffect smokeEffect;

    [Header("Sounds")]
    public AudioClip hitObjectHardSound;
    public float hitMinHardMagnitute;
    [Space(5)]
    public AudioClip hitObjectSoftSound;
    public float hitMinSoftMagnitute;
    [Space(2)]
    public AudioSource hitObjectSource;
    [Space(5)]
    public SoundClip launchingSound;

    /* private fields */
    BoxCollider2D boxCol;
    private Rigidbody2D rb;

    private bool isGrounded = false;
    private bool isStanding = false;

    private bool isLaunching;
    private bool shouldLaunch = false;
    private ContactFilter2D contactFilter = new ContactFilter2D();

    private float standingTimer = 1;
    private float pathLengthTimer = 1;

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

                    boxCol.size = new Vector2(boxCol.size.x, colliderStandingHeight);
                    boxCol.offset = new Vector2(boxCol.offset.x, colliderStandingOffset);
                }
                else standingTimer -= Time.deltaTime * standUpSpeed;
            }
            else standingTimer = 1;
        }
        else isStanding = false;

        if (isStanding)
        {
            transform.rotation = Quaternion.identity;

            if (Input.GetKey(KeyCode.Space) && !shouldLaunch)
            {
                if (!isLaunching) //Using this instead of Input.GetKeyDown to prevent input not registering
                {
                    OnLaunchStart();
                }
                else
                {
                    Launching();
                }
            }
            else if (isLaunching)
            {
                OnLaunchEnd();
            }
        }
        else if (isLaunching) //If falling while holding space
        {
            isLaunching = false;
            shouldLaunch = false;

            DestroySmoke();
            pathDrawer.StopCurve();
            cameraShake.StopShake();

            anim.SetTrigger("pathEnd");

            launchingSound.Stop();
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

                    anim.SetTrigger("pathEnd");
                    smokeTrail.Stop(true, ParticleSystemStopBehavior.StopEmitting);

                    //Sound
                    launchingSound.Stop();
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

    #region Launching
    /// <summary>
    /// Runs on start of launch (Space Pressed)
    /// </summary>
    void OnLaunchStart()
    {
        isLaunching = true;

        pathTimer = 0;
        targetPath.Clear();
        Quaternion rot = Quaternion.AngleAxis(transform.eulerAngles.z, Vector3.up);
        Vector3 offset = rot * pathStartOffset;
        pathDrawer.transform.position = transform.position + offset;
        pathDrawer.transform.up = transform.up;

        pathLengthTimer = pathLengthTimerLength;

        //Effects
        CreateSmoke();
        pathDrawer.StartCurve();

        cameraShake.StartShake(25, 4);

        //Sound
        launchingSound.Play();
    }

    /// <summary>
    /// Runs when launching (Holding space)
    /// </summary>
    void Launching()
    {
        //Make Path
        float h = Input.GetAxis("Horizontal");

        Vector3 rot = pathDrawer.transform.eulerAngles;
        rot.z -= h * rotationSpeed * Time.deltaTime;
        pathDrawer.transform.rotation = Quaternion.Euler(rot);

        pathDrawer.transform.position += pathDrawer.transform.up * speed * Time.deltaTime;

        pathLengthTimer -= Time.deltaTime;

        //Add point to path
        pathTimer -= Time.deltaTime * pathInterval;
        if (pathTimer <= 0)
        {
            pathTimer = 1;
            targetPath.Add(pathDrawer.transform.position);
        }

        if (pathLengthTimer <= 0 || pathDrawerCol.Cast(pathDrawer.transform.up, contactFilter, new RaycastHit2D[1], 0.03f) > 0)
        {
            shouldLaunch = true;
        }
    }
    
    /// <summary>
    /// Runs on end of launch (Space released)
    /// </summary>
    void OnLaunchEnd()
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

        cameraShake.StopShake();

        boxCol.size = new Vector2(boxCol.size.x, colliderFallingHeight);
        boxCol.offset = new Vector2(boxCol.offset.x, colliderFallingOffset);
    }
    #endregion

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

        /*
        if (!shouldFollowPath) {
            if (!isStanding && !isFalling)
            {
                spriteTransform.localRotation = Quaternion.identity;

                //Rotate player towards our current velocity
                //Vector3 curRot = spriteTransform.rotation.eulerAngles;
                //curRot.z = Mathf.Atan2(rb.velocity.x, rb.velocity.y) * Mathf.Rad2Deg;
                //spriteTransform.rotation = Quaternion.Euler(curRot);

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
        */
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

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!hitObjectSource.isPlaying)
        {
            if (collision.relativeVelocity.sqrMagnitude > hitMinHardMagnitute) hitObjectSource.PlayOneShot(hitObjectHardSound);
            else if (collision.relativeVelocity.sqrMagnitude > hitMinSoftMagnitute) hitObjectSource.PlayOneShot(hitObjectSoftSound);
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
