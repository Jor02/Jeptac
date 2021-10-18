using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathDrawer : MonoBehaviour
{
    [Header("Speeds")]
    public float removeSpeed = 5;

    [Header("References")]
    public SpriteRenderer mainDot;
    public ParticleSystem dotParticle;

    bool prev = false;
    ParticleSystem.MainModule main;

    private bool isPlaying = false;

    private void Awake()
    {
        main = dotParticle.main;
        isPlaying = false;
    }

    void Update()
    {
        if (isPlaying)
        {
            main.startRotation = -transform.rotation.eulerAngles.z * Mathf.Deg2Rad;
        }
    }

    public void StartCurve()
    {
        isPlaying = true;

        mainDot.enabled = true;
        dotParticle.Play(true);
    }

    public void StopCurve()
    {
        isPlaying = false;

        mainDot.enabled = false;
        dotParticle.Stop(true, ParticleSystemStopBehavior.StopEmitting);

        ParticleSystem.Particle[] particles = new ParticleSystem.Particle[dotParticle.particleCount];
        dotParticle.GetParticles(particles);

        for (int i = 0; i < particles.Length; i++)
        {
            particles[i].startLifetime = 1 / removeSpeed * i;
        }

        dotParticle.SetParticles(particles);
    }
}
