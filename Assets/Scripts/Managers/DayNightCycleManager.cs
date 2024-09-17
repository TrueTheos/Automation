using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering.Universal;
using static Assets.Scripts.Managers.DayNightCycleManager;

namespace Assets.Scripts.Managers
{
    [RequireComponent(typeof(Light2D))]
    public class DayNightCycleManager : MonoBehaviour
    {
        public static DayNightCycleManager Instance;

        [SerializeField] private float _dayLength;
        private TimeSpan _currentTime;
        [Range(0,23), SerializeField] private int _startHour;
        private float _minuteLength => _dayLength / MinutesInDay;
        public const int MinutesInDay = 1440;

        private Light2D _light;

        [SerializeField] private Gradient _gradient;

        [Header("Debug Info")]
        [SerializeField] private bool _stopTime;
        [SerializeField] private int _currentHour;
        [SerializeField] private int _currentMinute;

        private List<ScheduledEvent> _scheduledEvents = new();

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
                if (!_stopTime)
                {
                    _currentTime += TimeSpan.FromMinutes(1);
                    _light.color = _gradient.Evaluate(PercentOfDay());

                    _currentHour = _currentTime.Hours;
                    _currentMinute = _currentTime.Minutes;
                    CheckScheduledEvents();
                    yield return new WaitForSeconds(_minuteLength);
                }
                else
                {
                    yield return new WaitForEndOfFrame();
                }
            }
        }

        private void CheckScheduledEvents()
        {
            for (int i = _scheduledEvents.Count - 1; i >= 0; i--)
            {
                var schedule = _scheduledEvents[i];
                if (schedule.Hour == _currentHour && schedule.Minute == _currentMinute)
                {
                    schedule.Event.Invoke();
                    if (!schedule.Repeat)
                    {
                        _scheduledEvents.RemoveAt(i);
                    }
                }
            }
        }

        public float PercentOfDay()
        {
            return (float)_currentTime.TotalMinutes % MinutesInDay / MinutesInDay;
        }

        public void AddScheduledEvent(ScheduledEvent schedule)
        {
            _scheduledEvents.Add(schedule);
        }
    }

    [Serializable]
    public class ScheduledEvent
    {
        [Range(0, 23)] public int Hour;
        [Range(0, 60)] public int Minute;
        public bool Repeat;
        public UnityEvent Event;
    }
}
