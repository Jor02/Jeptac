using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Debug : MonoBehaviour
{
#if UNITY_EDITOR
    public GameObject[] toggle;

    void Awake()
    {
        foreach (GameObject obj in toggle)
        {
            obj.SetActive(!obj.activeSelf);
        }
    }
#endif
}
