using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayParticle : MonoBehaviour
{
    [SerializeField] List<ParticleSystem> Particles;
    void OnTriggerEnter2D(Collider2D Player)
    {
        if(Player.gameObject.layer != 12) { return; }
        foreach (ParticleSystem Particle in Particles)
        {
            Particle.Play();
        }
    }
    void OnTriggerExit2D(Collider2D Player)
    {
        if(Player.gameObject.layer != 12) { return; }
        foreach (ParticleSystem Particle in Particles)
        {
            Particle.Stop();
        }
    }
}
