using Assets.Scripts.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static Assets.Scripts.Utilities;

namespace Assets.Scripts.MapObjects
{
    public abstract class FluidUserObject : MapObject
    {
        public virtual float Capacity { get; set; }
        public virtual float CurrentFill { get; set; }
        public Dictionary<Connection, FluidUserObject> ConnectedObjects = new();
        public virtual Connection InputDirection {get; set;}
        public virtual Connection OutputDirection { get; set; }
        public virtual FluidType InputFluidType { get; set; }
        public virtual FluidType OutputFluidType { get; set; }
        public Connection CurrentConnections = Connection.None;
        public abstract bool IsConnectedTo(FluidUserObject other);
        public abstract void Connect(FluidUserObject other, Connection comingFrom);
        public abstract void UpdateConnections(FluidType fluidType);
        public abstract bool CanConnect(FluidUserObject other, Connection comingFrom);
 
        public void OnDestroy()
        {
            FluidManager.Instance.UnregisterContainer(this);
        }
    }
}
