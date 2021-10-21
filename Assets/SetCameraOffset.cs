using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetCameraOffset : MonoBehaviour
{
    public CinemachineCameraOffset camOffset;

    void Update()
    {
        camOffset.m_Offset = transform.localPosition;
    }
}
