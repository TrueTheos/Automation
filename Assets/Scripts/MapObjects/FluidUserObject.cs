using Assets.Scripts.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

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
    
        public Connection GetOppositeConnection(Connection con)
        {
            if (con == Connection.Down) return Connection.Up;
            if (con == Connection.Up) return Connection.Down;
            if (con == Connection.Left) return Connection.Right;
            if (con == Connection.Right) return Connection.Left;
            return Connection.None;
        }

        public Vector2 ConnectionToVector(Connection con)
        {
            if (con == Connection.Down) return Vector2.down;
            if (con == Connection.Up) return Vector2.up;
            if (con == Connection.Left) return Vector2.left;
            if (con == Connection.Right) return Vector2.right;
            return Vector2.zero;
        }

        public void OnDestroy()
        {
            FluidManager.Instance.UnregisterContainer(this);
        }
    }

    [System.Flags]
    public enum Connection
    {
        None = 0,
        Up = 1,
        Right = 2,
        Down = 4,
        Left = 8
    }
}
