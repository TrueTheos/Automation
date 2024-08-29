using Assets.Scripts.MapObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI
{
    public class FurnaceView : BuildingView
    {
        public Image ProgressBar;
        public ItemSlot InputSlot;
        public ItemSlot OutputSlot;

        private FurnaceObject _currentFurnace;

        private void Start()
        {
            InputSlot.OnItemChangeEvent += UpdateFurnace;
        }

        public void UpdateSlots(ItemAmount input, ItemAmount output)
        {
            if(input.Item != null) InputSlot.Init(input);
            if (output.Item != null) OutputSlot.Init(output);
        }

        private void UpdateFurnace()
        {
            if(_currentFurnace != null)
            {
                _currentFurnace.UpdateItems(InputSlot, OutputSlot);
            }
        }

        public void OpenFurnace(FurnaceObject furnace)
        {
            _currentFurnace = furnace;
            Open();
        }
    }
}
