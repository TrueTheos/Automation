using Assets.Scripts.MapObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Managers
{
    public class ConveyorBeltManager : MonoBehaviour
    {
        public static ConveyorBeltManager Instance;

        private List<ConveyorBeltObject> _conveyorBelts = new();

        private float _moveInterval = 1f;
        private float _timeSinceLastMove = 0f;

        private void Awake()
        {
            Instance = this;
        }

        private void Update()
        {
            _timeSinceLastMove += Time.deltaTime;
            if (_timeSinceLastMove >= _moveInterval)
            {
                UpdateBelts();
                _timeSinceLastMove = 0f;
            }
        }

        private void UpdateBelts()
        {
            foreach (ConveyorBeltObject belt in _conveyorBelts)
            {
                belt.MoveItems();
            }
        }

        public void AddBelt(ConveyorBeltObject belt)
        {
            _conveyorBelts.Add(belt);
        }

        public void RemoveBelt(ConveyorBeltObject belt)
        {
            _conveyorBelts.Remove(belt);
        }
    }
}
