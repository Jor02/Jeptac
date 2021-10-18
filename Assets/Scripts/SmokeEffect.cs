using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmokeEffect : MonoBehaviour
{
    [HideInInspector] public Transform target;
    [SerializeField] private Animator anim;

    private bool shouldDestroy;
    public void Destroy()
    {
        anim.SetTrigger("stop");
        shouldDestroy = true;
    }

    private void Update()
    {
        if (!shouldDestroy)
        {
            if (target != null)
            {
                transform.position = target.position;
                transform.rotation = target.rotation;
            }
        }
        else if (anim.GetCurrentAnimatorStateInfo(0).IsName("SmokeEnd") && anim.GetCurrentAnimatorStateInfo(0).normalizedTime > 1)
        {
            Destroy(gameObject);
        }
    }
}
