using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProceduralLegs : MonoBehaviour
{
    [SerializeField] private LayerMask layerMask;
    [SerializeField] private Material legMaterial;
    [SerializeField] private AnimationCurve stepAnim = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(0, 0));
    [SerializeField] private float stepSpeed;
    [SerializeField] private float stepHeight;
    [SerializeField] private Leg[] legs;

    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        for (int i = 0; i < legs.Length; i++)
        {
            Leg leg = legs[i];

            #region Create Foot Object
            GameObject footObject = new GameObject("Foot");
            leg.footRB = footObject.AddComponent<Rigidbody2D>();
            leg.footRB.bodyType = RigidbodyType2D.Static;
            leg.footTrans = footObject.transform;
            #endregion

            #region Update Leg Info
            //Update current leg information
            UpdateLeg(leg);

            //Set initial leg resting positions
            if (i % 2 == 0)
                leg.curPosition = leg.restingPosition + transform.right * (leg.maxDistance / 2);
            else
                leg.curPosition = leg.restingPosition - transform.right * (leg.maxDistance / 2);

            leg.prevPosition = leg.curPosition;
            #endregion

            #region Create Leg Object
            //Add a GameObject to assign a LineRenderer to
            GameObject legObject = new GameObject("Leg");
            legObject.transform.position = transform.position + leg.position;
            footObject.transform.parent = legObject.transform;
            leg.legRB = legObject.AddComponent<Rigidbody2D>();
            legObject.AddComponent<HingeJoint2D>().connectedBody = leg.footRB;
            #endregion

            #region Create LineRenderer
            //Add LineRender to leg GameObject
            leg.lineRenderer = legObject.AddComponent<LineRenderer>();
            leg.lineRenderer.startWidth = 0.1f;
            leg.lineRenderer.endWidth = 0.1f;
            leg.lineRenderer.material = legMaterial;
            leg.lineRenderer.numCapVertices = 7;
            #endregion
        }
    }

    void Update()
    {
        UpdateLegs();
    }

    void UpdateLegs()
    {
        foreach (Leg leg in legs)
        {
            UpdateLeg(leg);

            if (leg.footRB != null) leg.footRB.bodyType = leg.isGrounded ? RigidbodyType2D.Static : RigidbodyType2D.Dynamic;

            if (leg.legRB != null)
            {
                Vector3 targetPos = transform.position + leg.position;
                if (leg.isGrounded) leg.legRB.transform.position = targetPos;
                else leg.legRB.MovePosition(targetPos);
            }

            if (leg.lineRenderer != null) {
                if (leg.isGrounded)
                {
                    leg.lastStepTime = Mathf.Clamp01(leg.lastStepTime - Time.smoothDeltaTime * Mathf.Max(1, rb.velocity.x) * stepSpeed);
                    Vector3 newPos = Vector3.Lerp(leg.prevPosition, leg.curPosition, 1 - leg.lastStepTime);
                    newPos.y += stepAnim.Evaluate(1 - leg.lastStepTime) * stepHeight;

                    leg.footTrans.position = newPos;
                }

                leg.lineRenderer.SetPositions(new Vector3[] {
                    leg.legRB.position,
                    leg.footTrans.position
                });
            }
        }
    }

    void UpdateLeg(Leg leg)
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position + leg.position, -Vector2.up, leg.legLength, layerMask);
        leg.isGrounded = hit.collider != null;
        if (hit.collider != null)
        {
            leg.restingPosition = hit.point;

            if (Vector3.Distance(leg.curPosition, leg.restingPosition) > leg.maxDistance)
            {
                leg.prevPosition = leg.curPosition;

                Vector3 nextOffset = transform.right * (leg.maxDistance * 0.90f);
                if (leg.curPosition.x > leg.restingPosition.x)
                    leg.curPosition = leg.restingPosition - nextOffset;
                else leg.curPosition = leg.restingPosition + nextOffset;

                if (leg.footTrans != null) leg.footTrans.position = leg.curPosition;
                leg.lastStepTime = 1;
            }
        }
        else
        {
            leg.curPosition = leg.footTrans.position;
        }
    }
    
    private void OnDrawGizmos()
    {
        for (int i = 0; i < legs.Length; i++)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(transform.position + legs[i].position, 0.1f);
            Gizmos.DrawSphere(legs[i].restingPosition, 0.1f);

            Gizmos.color = Color.red;
            Gizmos.DrawSphere(legs[i].curPosition, 0.1f);
        }
    }

    [Serializable]
    class Leg
    {
        public Vector3 position;
        public float legLength = 1;
        public float maxDistance = 0.5f;

        [HideInInspector] public Vector3 restingPosition;
        [HideInInspector] public Vector3 curPosition;
        [HideInInspector] public Vector3 prevPosition;
        [HideInInspector] public float lastStepTime;
        [HideInInspector] public bool isGrounded;
        [HideInInspector] public LineRenderer lineRenderer;
        [HideInInspector] public Transform footTrans;
        [HideInInspector] public Rigidbody2D footRB;
        [HideInInspector] public Rigidbody2D legRB;
    }
}