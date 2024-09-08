using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Assets.Scripts.Managers
{
    [RequireComponent(typeof(Light2D))]
    public class DayNightCycleManager : MonoBehaviour
    {
        public static DayNightCycleManager Instance;

        [SerializeField] private float _dayLength;
        private TimeSpan _currentTime;
        private float _minuteLength => _dayLength / MinutesInDay;
        public const int MinutesInDay = 1440;

        private Light2D _light;

        [SerializeField] private Gradient _gradient;

        private void Awake()
        {
            Instance = this;
            _light = GetComponent<Light2D>();
        }

        private void Start()
        {
            StartCoroutine(AddMinute());
        }

        private IEnumerator AddMinute()
        {
            while (true)
            {
                _currentTime += TimeSpan.FromMinutes(1);
                _light.color = _gradient.Evaluate(PercentOfDay(_currentTime));
                yield return new WaitForSeconds(_minuteLength);
            }
        }

        private float PercentOfDay(TimeSpan timeSpan)
        {
            return (float)timeSpan.TotalMinutes % MinutesInDay / MinutesInDay;
        }
    }
}
