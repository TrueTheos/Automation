using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Assets.Scripts.Managers;

namespace Assets.Scripts
{
    public interface IFluidReceiver
    {
        float Capacity { get; set; }
        float CurrentFluid { get; set; }
        bool IsConnectedTo(IFluidReceiver other);
        List<IFluidReceiver> Connections { get; }
        PipeConnection CurrentConnections { get; set; }
        Vector3 Position { get; }

        void Connect(IFluidReceiver other);
        void UpdateConnections(FluidType fluidType);
        void UpdateSprite();
    }

    [System.Flags]
    public enum PipeConnection
    {
        None = 0,
        Up = 1,
        Right = 2,
        Down = 4,
        Left = 8
    }
}
