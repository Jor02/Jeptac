using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForestGenerator : MonoBehaviour
{
    public GameObject instance;
    public Vector3 offset;
    public Transform parent;
    public float Radius = 2;
    public float TreeCount = 300;
    [Space(10)]
    public float minSize;
    public float maxSize;
    [Space(10)]
    public float minYSize;
    public float maxYSize;

    private List<Vector3> positions = new List<Vector3>();

    void Start()
    {
        for (int i = 0; i < TreeCount; i++)
        {
            Vector3 pos = transform.position;
            pos.x += Random.Range(-transform.lossyScale.x / 2, transform.lossyScale.x / 2);
            pos.z += Random.Range(-transform.lossyScale.z / 2, transform.lossyScale.z / 2);

            RaycastHit hit;
            if (Physics.Raycast(pos + transform.up * 30f, -transform.up, out hit))
                pos = hit.point;

            positions.Add(pos);
        }

        for (int i = 0; i < positions.Count; i++)
        {
            Transform t = Instantiate(instance, parent).transform;
            t.position = positions[i] + offset;
            Vector3 newRot = Vector3.zero;
            newRot.x = Random.Range(-10, 10);
            newRot.y = Random.Range(0, 360);
            newRot.z = Random.Range(-10, 10);

            t.eulerAngles = newRot;

            float width = Random.Range(minSize, maxSize);
            t.localScale = new Vector3(width, width, Random.Range(minYSize, maxYSize));
            t.name = "Tree" + i;
        }
    }

    private void OnDrawGizmos()
    {
        Vector3 size = transform.lossyScale;
        size.y = 0;

        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position, size);

        Gizmos.color = Color.red;
        foreach (Vector3 point in positions)
        {
            Gizmos.DrawSphere(point, 0.1f);
        }
    }
}
