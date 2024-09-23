using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.UI
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance;

        public FurnaceView FurnaceView;
        public ElectricFurnaceView ElectricFurnaceView;
        public SplitterView SplitterView;
        public IronCrateView IronCrateView;
        public MultimetrView MultimetrView;

        private void Awake()
        {
            Instance = this;
        }
    }
}
