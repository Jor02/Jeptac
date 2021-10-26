using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpaceSkybox : MonoBehaviour
{
    public GameObject asteroids;
    public Transform cameraTransform;
    public Color normalGroundCol = new Color(163, 176, 189);
    public Color normalFogCol = new Color(163, 176, 189);
    public float maxHeight = 1f;
    public float normalAtmosphere = 0.37f;
    public float spaceAtmosphere = 0f;

    // Update is called once per frame
    void Update()
    {
        float height = Mathf.Max(cameraTransform.position.y - transform.position.y, 0);
        float thickness = Mathf.Min(height, maxHeight) / maxHeight;
        RenderSettings.skybox.SetFloat("_AtmosphereThickness", Mathf.Lerp(normalAtmosphere, spaceAtmosphere, thickness));
        RenderSettings.skybox.SetColor("_GroundColor", Color.Lerp(normalGroundCol, Color.black, thickness));
        RenderSettings.fogDensity = Mathf.Lerp(0.03f, 0.01f, thickness);
        RenderSettings.fogColor = Color.Lerp(normalFogCol, Color.black, thickness);
        asteroids.SetActive(thickness >= 0.2f);
    }
}
