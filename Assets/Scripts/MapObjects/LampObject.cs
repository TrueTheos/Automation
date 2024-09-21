using Assets.Scripts.Managers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.MapObjects
{
    public class LampObject : MapObject
    {
        [SerializeField] private ParticleSystem _particle;
        private ParticleSystem.EmissionModule _emissionModule;
        [SerializeField] private float _hideParticlesDuration;
        [SerializeField] private ScheduledEvent _turnOnLights, _turnOffLights;
        [SerializeField] private ScheduledEvent _enableFireflies, _disableFireflies;
        private ParticleSystem.MinMaxGradient _originalStartColor;
        private Coroutine _hidingFirefliesCoroutine;

        private void Awake()
        {
            _emissionModule = _particle.emission;
            _emissionModule.enabled = false;
        }

        private void Start()
        {
            DayNightCycleManager dayNightCycleManager = DayNightCycleManager.Instance;
            dayNightCycleManager.AddScheduledEvent(_turnOnLights);
            dayNightCycleManager.AddScheduledEvent(_turnOffLights);
            dayNightCycleManager.AddScheduledEvent(_enableFireflies);
            dayNightCycleManager.AddScheduledEvent(_disableFireflies);
            _originalStartColor = _particle.main.startColor;
        }

        public void ToggleFireflies(bool toggle)
        {
            _emissionModule.enabled = toggle;

            if(!toggle)
            {
                if(_hidingFirefliesCoroutine != null)
                {
                    StopCoroutine(_hidingFirefliesCoroutine);
                }
                _hidingFirefliesCoroutine = StartCoroutine(HideFireflies());
            }
        }

        private IEnumerator HideFireflies()
        {
            var emission = _particle.emission;
            emission.enabled = false;

            float elapsedTime = 0f;
            ParticleSystem.Particle[] particles = new ParticleSystem.Particle[_particle.main.maxParticles];
            int liveParticles;

            while (elapsedTime < _hideParticlesDuration)
            {
                liveParticles = _particle.GetParticles(particles);
                if (liveParticles == 0) break;

                float t = elapsedTime / _hideParticlesDuration;

                for (int i = 0; i < liveParticles; i++)
                {
                    // Reduce remaining lifetime
                    particles[i].remainingLifetime = Mathf.Lerp(particles[i].remainingLifetime, 0f, t);

                    // Optionally, fade out particles
                    Color color = particles[i].startColor;
                    color.a = Mathf.Lerp(color.a, 0f, t);
                    particles[i].startColor = color;
                }

                _particle.SetParticles(particles, liveParticles);

                elapsedTime += Time.deltaTime;
                yield return null;
            }

            // Clear any remaining particles
            _particle.Clear();

            _hidingFirefliesCoroutine = null;
        }
    }
}
