using Managers;
using MapObjects.ElectricGrids;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static Assets.Scripts.WattsUtils;

namespace Assets.Scripts.MapObjects
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class AccumulatorObject : MapObject
    {
        [SerializeField] private float _range;
        [SerializeField] private Watt _capacity;
        [SerializeField] private GameObject _rangeHighlight;

        private HashSet<PowerGrid> _electricPoles = new();

        private void Start()
        {
            _rangeHighlight.transform.localScale = new Vector3(_range, _range, 1);
        }

        private void Update()
        {
            if(_electricPoles != null)
            {
                // todo
            }
        }

        private IEnumerator UpdateElectricPoles()
        {
            while(true)
            {
                yield return new WaitForSeconds(1f);
                // todo to do optymalizacji
                _electricPoles = new();

                var colliders = Physics2D.OverlapBoxAll(transform.position, new(_range, _range), 0);

                foreach (var collider in colliders)
                {
                    ElectricPoleObject electricPole = collider.GetComponent<ElectricPoleObject>();
                    if (electricPole != null && electricPole.PowerGrid != null) _electricPoles.Add(electricPole.PowerGrid);
                }
            }
        }

        private void OnMouseEnter()
        {
            _rangeHighlight.SetActive(true);
        }

        private void OnMouseExit()
        {
            _rangeHighlight.SetActive(false);
        }
    }
}
