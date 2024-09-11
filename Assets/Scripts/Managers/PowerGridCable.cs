using MapObjects.ElectricGrids;

using UnityEngine;

namespace Managers
{
    public class PowerGridCable : MonoBehaviour
    {
        public IPowerGridUser Start;
        public IPowerGridUser End;
        public LineRenderer LineRenderer;

        public PowerGridCable(IPowerGridUser start, IPowerGridUser end, LineRenderer lineRenderer)
        {
            Start = start;
            End = end;
            LineRenderer = lineRenderer;
        }

        public void SetState(PowerState powerState)
        {
            //if (powerState.HasFlag(PowerState.IsConnected))
            //{
            //    LineRenderer.material.color = Color.gray;
            //}
            
            //if (powerState.HasFlag(PowerState.HasPower))
            //{
            //    LineRenderer.material.color = Color.green;
            //}
        }
    }
}