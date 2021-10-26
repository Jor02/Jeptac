using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Debugger : MonoBehaviour
{
#if UNITY_EDITOR
    [Range(0,1)]
    public float timeScale = 1;
    new public bool enabled;
    public bool continueGame;
    public GameObject[] toggle;
    public float width = 10;
    public float height = 10;

    void Awake()
    {
        if (enabled)
            foreach (GameObject obj in toggle)
            {
                obj.SetActive(!obj.activeSelf);
            }
        if (continueGame)
            MenuScript.Instance.ContinueGame();
    }

    private void Update()
    {
        Time.timeScale = timeScale;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Vector3 left = Vector3.left * width / 2;
        Vector3 right = Vector3.right * width / 2;
        Gizmos.DrawLine(left, left + Vector3.up * height);
        Gizmos.DrawLine(right, right + Vector3.up * height);

        float cameraHeight = SceneView.lastActiveSceneView.camera.transform.position.y;
        Gizmos.color = Color.red;
        Gizmos.DrawLine(left + Vector3.up * cameraHeight, right + Vector3.up * cameraHeight);
    }
#endif
}
