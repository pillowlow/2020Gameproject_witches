using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleEffect : MonoBehaviour
{
    [SerializeField] private ParticleSystem m_particleSystem;
    [SerializeField] PlayerMovement.EventType EventToAssign;
    [SerializeField] float OffsetTime = 1;
    [SerializeField] float EndTime = 1;
    [SerializeField] float GapTime = 2;
    [SerializeField] bool Mirror;
    [SerializeField] bool DirectionDependsOnCharacter;
    [SerializeField] Transform Follow;
    private Vector3 scale;

    private void Start()
    {
        PlayerMovement.instance.AssignEvent(EventToAssign, ParticleAction);
        if(Mirror)
        {
            scale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
        }
        else
        {
            scale = transform.localScale;
        }
        m_particleSystem.Stop();
    }
    float lastStartTime = 0;
    bool IsEnd = true;
    void ParticleAction()
    {
        if(Time.time - lastStartTime + EndTime > GapTime && IsEnd)
        {
            if (OffsetTime != 0)
            {
                StartCoroutine(nameof(WaitToStart));
            }
            else
            {
                StartParticle();
            }
            IsEnd = false;
        }
    }

    IEnumerator WaitToStart()
    {
        yield return new WaitForSeconds(OffsetTime);
        StartParticle();
    }

    void StartParticle()
    {
        lastStartTime = Time.time;
        if (DirectionDependsOnCharacter)
        {
            transform.localScale = new Vector3(PlayerMovement.instance.orient ? scale.x : -scale.x, scale.y, scale.z);
        }
        transform.position = Follow.position;
        m_particleSystem.Play();
        StartCoroutine(nameof(EndEffect));
    }
    IEnumerator EndEffect()
    {
        yield return new WaitForSeconds(EndTime);
        m_particleSystem.Stop();
        IsEnd = true;
    }
}
