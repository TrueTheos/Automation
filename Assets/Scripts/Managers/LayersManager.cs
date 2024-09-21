using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Managers
{
    public class LayersManager : MonoBehaviour
    {
        public static LayersManager Instance;

        public LayerMask IgnoreRaycastLayer;
        public LayerMask InteractColliderLayer;

        private void Awake()
        {
            Instance = this;
        }
    }
}
