using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Debugger : MonoBehaviour
{
#if UNITY_EDITOR
    new public bool enabled;
    public GameObject[] toggle;

    void Awake()
    {
        if (enabled)
            foreach (GameObject obj in toggle)
            {
                obj.SetActive(!obj.activeSelf);
            }
    }
#endif
}
