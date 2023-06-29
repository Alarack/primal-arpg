using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class AttachGameObjectsToParticles : MonoBehaviour
{
    public LightTools lightPrefab;

    private ParticleSystem m_ParticleSystem;
    private List<LightTools> activeLights = new List<LightTools>();
    private ParticleSystem.Particle[] m_Particles;

    // Start is called before the first frame update
    void Start()
    {
        m_ParticleSystem = GetComponent<ParticleSystem>();
        m_Particles = new ParticleSystem.Particle[m_ParticleSystem.main.maxParticles];
    }

    // Update is called once per frame
    void LateUpdate()
    {
        int count = m_ParticleSystem.GetParticles(m_Particles);

        while (activeLights.Count < count)
            activeLights.Add(Instantiate(lightPrefab, m_ParticleSystem.transform));

        bool worldSpace = (m_ParticleSystem.main.simulationSpace == ParticleSystemSimulationSpace.World);
        for (int i = 0; i < activeLights.Count; i++)
        {
            if (i < count)
            {
                if (worldSpace)
                    activeLights[i].transform.position = m_Particles[i].position;
                else
                    activeLights[i].transform.localPosition = m_Particles[i].position;

                float lifeRatio = m_Particles[i].remainingLifetime / m_Particles[i].startLifetime;

                activeLights[i].gameObject.SetActive(true);
                activeLights[i].SetAlpha(lifeRatio);
                //activeLights[i].SetFadeTime(0.5f, 0f);
                //activeLights[i].SetActive(true);
            }
            else
            {
                //activeLights[i].SetFadeTime(0.5f, 1f);
                activeLights[i].gameObject.SetActive(false);
            }
        }
    }
}
