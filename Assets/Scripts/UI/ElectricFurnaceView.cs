using Assets.Scripts.MapObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.UI;

namespace Assets.Scripts.UI
{
    public class ElectricFurnaceView : BuildingView
    {
        public Image ProgressBar;
        public ItemSlot InputSlot;
        public ItemSlot OutputSlot;

        private ElectricFuranceObject _currentFurnace;

        private void Start()
        {
            InputSlot.OnItemChangeEvent += UpdateFurnace;
            OutputSlot.OnItemChangeEvent += UpdateFurnace;
        }

        public void UpdateSlots(ItemAmount input, ItemAmount output)
        {
            if (input.GetItem() != null) InputSlot.Init(input);
            if (output.GetItem() != null) OutputSlot.Init(output);
        }

        private void UpdateFurnace()
        {
            if (_currentFurnace != null)
            {
                _currentFurnace.UpdateItems(InputSlot, OutputSlot);
            }
        }

        public void OpenFurnace(ElectricFuranceObject furnace)
        {
            _currentFurnace = furnace;
            ResetUI();
            Open();
        }

        public override void ResetUI()
        {
            InputSlot.ResetSlot();
            OutputSlot.ResetSlot();
        }
    }
}
