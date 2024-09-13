using Assets.Scripts.Items;
using Assets.Scripts.Managers;
using Managers;
using MapObjects.ElectricGrids;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Assets.Scripts.WattsUtils;

namespace Assets.Scripts.UI
{
    public class MultimetrView : MonoBehaviour
    {
        [SerializeField] private Item _multimetr;

        [Header("Cable View")]
        [SerializeField] private GameObject CableParent;
        [SerializeField] private Image CableFillBar;
        [SerializeField] private TextMeshProUGUI AvailableCablePower;
        [SerializeField] private TextMeshProUGUI ConsumedCablePower;

        private Player _player;

        private bool _enabled = false;

        private void Start()
        {
            _player = GameManager.Instance.CurrentPlayer;

            if(_multimetr == null)
            {
                Debug.LogError("MULTIMETR NIE JEST PRZYPISANY");
                return;
            }

            _player.PlayeMovement.OnSelectedItemChange += OnItemChange;
        }

        public void OnItemChange()
        {
            if(_player.PlayeMovement.SelectedItem.Item != _multimetr)
            {
                CableParent.SetActive(false);
                _enabled = false;
            }
            else
            {
                _enabled = true;
            }
        }

        public void OnElectricPoleHover(ElectricPoleObject pole)
        {
            if(pole == null)
            {
                CableParent.SetActive(false);
                return;
            }
            if (!_enabled) return;

            PowerGrid currentGrid = pole.PowerGrid;
            if (currentGrid == null)
            {
                return;
            }

            Watt availablePower = currentGrid.ProducedPower;
            Watt consumedPower = currentGrid.ConsumedPower;
            AvailableCablePower.text = WattsToString(availablePower);
            ConsumedCablePower.text = WattsToString(consumedPower);

            WattType highestUnit = GetHighestUnit(availablePower.WattType, consumedPower.WattType);
            double availableInHighestUnit = ConvertToUnit(availablePower, highestUnit);
            double requiredInHighestUnit = ConvertToUnit(consumedPower, highestUnit);
            float ratio;
            if (requiredInHighestUnit == 0 || availableInHighestUnit == 0) ratio = 0;
            else { ratio = (float)(requiredInHighestUnit / availableInHighestUnit); }

            CableFillBar.fillAmount = Mathf.Max(0, 1 - ratio);

            CableParent.SetActive(true);
        }

        public void OnElectricBuildingHower()
        {
            CableParent.SetActive(false);
        }
    }
}
