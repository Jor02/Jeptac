using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmokeEffect : MonoBehaviour
{
    [SerializeField] private Animator anim;
    private bool shouldDestroy;
    public void Destroy()
    {
        anim.SetTrigger("stop");
        shouldDestroy = true;
    }

    private void Update()
    {
        if (shouldDestroy && anim.GetCurrentAnimatorStateInfo(0).IsName("SmokeEnd") && anim.GetCurrentAnimatorStateInfo(0).normalizedTime > 1)
        {
            Destroy(gameObject);
        }
    }
}
