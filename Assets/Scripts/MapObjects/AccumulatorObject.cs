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
using static Assets.Scripts.Utilities;

namespace Assets.Scripts.MapObjects
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class AccumulatorObject : MapObject
    {
        [SerializeField] private float _range;
        [SerializeField] private Watt _capacity;
        private Watt _storedPower;
        [SerializeField] private Watt _maxChargeRate;
        [SerializeField] private Watt _maxDischargeRate;
        [SerializeField] private GameObject _rangeHighlight;
        [SerializeField] private List<Sprite> _chargeSprites = new();

        private HashSet<PowerGrid> _powerGrids = new();

        private void Start()
        {
            _rangeHighlight.transform.localScale = new Vector3(_range, _range, 1);
            _storedPower = new Watt(_capacity.WattType, 0);
            StartCoroutine(UpdateElectricPoles());
        }

        private void Update()
        {
            if (_powerGrids != null)
            {
                Watt maxChargeThisFrame = new Watt(_maxChargeRate.WattType, _maxChargeRate.Value * Time.deltaTime);
                Watt totalChargeThisFrame = new Watt(_maxChargeRate.WattType, 0);

                foreach (var grid in _powerGrids)
                {
                    Watt excessPower = SumWatts(new List<Watt> { grid.ProducedPower, new Watt(grid.ConsumedPower.WattType, -grid.ConsumedPower.Value) });

                    if (CompareWatts(excessPower, new Watt(excessPower.WattType, 0)) > 0)
                    {
                        ChargeAccumulator(grid, excessPower, maxChargeThisFrame, totalChargeThisFrame);
                    }
                    else
                    {
                        DischargeAccumulator(grid);
                    }
                }

                double chargePercentage = ConvertToUnit(_storedPower, _capacity.WattType) / _capacity.Value;
                UpdateSprite((float)chargePercentage);
            }
        }

        private void ChargeAccumulator(PowerGrid grid, Watt excessPower, Watt maxChargeThisFrame, Watt totalChargeThisFrame)
        {
            double remainingCapacity = _capacity.Value - _storedPower.Value;

            if (remainingCapacity <= 0)
            {
                return;
            }

            Watt availableCharge = new Watt(_maxChargeRate.WattType,
                Math.Min(
                    ConvertToUnit(excessPower, _maxChargeRate.WattType),
                    maxChargeThisFrame.Value - totalChargeThisFrame.Value
                )
            );

            availableCharge = new Watt(availableCharge.WattType,
                Math.Min(
                    availableCharge.Value,
                    ConvertToUnit(new Watt(_capacity.WattType, remainingCapacity), availableCharge.WattType)
                )
            );

            Watt newStoredPower = SumWatts(new List<Watt> { _storedPower, ConvertWatt(availableCharge, _storedPower.WattType) });

            _storedPower = new Watt(_capacity.WattType,
                Math.Min(ConvertToUnit(newStoredPower, _capacity.WattType), _capacity.Value));
            totalChargeThisFrame = SumWatts(new List<Watt> { totalChargeThisFrame, availableCharge });
           // if (CompareWatts(totalChargeThisFrame, maxChargeThisFrame) >= 0 || CompareWatts(_storedPower, _capacity) >= 0)
             //   break;
        }

        private void DischargeAccumulator(PowerGrid grid)
        {
            if (CompareWatts(_storedPower, new Watt(_storedPower.WattType, 0)) > 0)
            {
                Watt maxDischargeThisFrame = new Watt(_maxDischargeRate.WattType, _maxDischargeRate.Value * Time.deltaTime);
                Watt powerDeficit = SumWatts(new List<Watt> { grid.ConsumedPower, new Watt(grid.ProducedPower.WattType, -grid.ProducedPower.Value) });

                Watt dischargeAmount = new Watt(_maxDischargeRate.WattType,
                    Math.Min(
                        ConvertToUnit(_storedPower, _maxDischargeRate.WattType),
                        Math.Min(
                            ConvertToUnit(powerDeficit, _maxDischargeRate.WattType),
                            maxDischargeThisFrame.Value
                        )
                    )
                );

                _storedPower = SumWatts(new List<Watt> { _storedPower, new Watt(dischargeAmount.WattType, -dischargeAmount.Value) });
                grid.AdditionalPower = SumWatts(new List<Watt> { grid.AdditionalPower, dischargeAmount });
            }
        }

        private IEnumerator UpdateElectricPoles()
        {
            while(true)
            {
                yield return new WaitForSeconds(1f);
                // todo to do optymalizacji
                _powerGrids = new();

                var colliders = Physics2D.OverlapBoxAll(transform.position, new(_range, _range), 0);

                foreach (var collider in colliders)
                {
                    ElectricPoleObject electricPole = collider.GetComponent<ElectricPoleObject>();
                    if (electricPole != null && electricPole.PowerGrid != null) _powerGrids.Add(electricPole.PowerGrid);
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

        private void UpdateSprite(float percentage)
        {
            SpriteRend.sprite = _chargeSprites.IndexByPercentage(percentage);
        }
    }
}
