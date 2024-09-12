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
        [Range(0,24), SerializeField] private int _startHour;
        private float _minuteLength => _dayLength / MinutesInDay;
        public const int MinutesInDay = 1440;

        private Light2D _light;

        [SerializeField] private Gradient _gradient;

        [Header("Debug Info")]
        [SerializeField] private int _currentHour;
        [SerializeField] private int _currentMinute;

        private void Awake()
        {
            Instance = this;
            _light = GetComponent<Light2D>();
        }

        private void Start()
        {
            _currentTime = TimeSpan.FromHours(_startHour);
            StartCoroutine(AddMinute());
        }

        private IEnumerator AddMinute()
        {
            while (true)
            {
                _currentTime += TimeSpan.FromMinutes(1);
                _light.color = _gradient.Evaluate(PercentOfDay(_currentTime));

                _currentHour = _currentTime.Hours;
                _currentMinute = _currentTime.Minutes;
                yield return new WaitForSeconds(_minuteLength);
            }
        }

        private float PercentOfDay(TimeSpan timeSpan)
        {
            return (float)timeSpan.TotalMinutes % MinutesInDay / MinutesInDay;
        }
    }
}
