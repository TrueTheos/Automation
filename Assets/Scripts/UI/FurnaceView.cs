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

        public void UpdateSlots(ItemAmount input, ItemAmount output)
        {
            InputSlot.Init(input);
            OutputSlot.Init(output);
        }
    }
}
