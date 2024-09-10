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

        protected Vector2[] GetCheckPositions(Connection con)
        {
            Vector2 basePos = transform.position;
            Vector2 direction = ConnectionToVector(con);
            List<Vector2> positions = new List<Vector2>();

            switch (Size)
            {
                case ObjectSize._1x1:
                    positions.Add(basePos + direction);
                    break;
                case ObjectSize._2x1:
                    if (con == Connection.Up || con == Connection.Down)
                    {
                        positions.Add(basePos + direction);
                        positions.Add(basePos + Vector2.right + direction);
                    }
                    else // Left or Right
                    {
                        positions.Add(basePos + direction * 2);
                    }
                    break;
                case ObjectSize._1x2:
                    if (con == Connection.Up)
                    {
                        positions.Add(basePos + Vector2.up * 2);
                    }
                    else if (con == Connection.Down)
                    {
                        positions.Add(basePos + Vector2.down);
                    }
                    else // Left or Right
                    {
                        positions.Add(basePos + direction);
                        positions.Add(basePos + Vector2.up + direction);
                    }
                    break;
                case ObjectSize._2x2:
                    if (con == Connection.Up || con == Connection.Down)
                    {
                        positions.Add(basePos + direction * 2);
                        positions.Add(basePos + Vector2.right + direction * 2);
                    }
                    else if (con == Connection.Left)
                    {
                        positions.Add(basePos + direction);
                        positions.Add(basePos + Vector2.up + direction);
                    }
                    else if (con == Connection.Right)
                    {
                        positions.Add(basePos + Vector2.right * 2 + direction);
                        positions.Add(basePos + Vector2.right * 2 + Vector2.up + direction);
                    }
                    break;
            }

            return positions.ToArray();
        }

        public void OnDestroy()
        {
            FluidManager.Instance.UnregisterContainer(this);
        }
    }
}
