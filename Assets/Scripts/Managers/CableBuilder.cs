using MapObjects.ElectricGrids;

using UnityEngine;

namespace Managers
{
    public class CableBuilder : MonoBehaviour
    {
        [SerializeField]
        private DraggableCable draggableCablePrefab;
        
        [SerializeField]
        private PowerGridCable powerGridCablePrefab;
        
        [SerializeField]
        public Color cableColor = Color.red;

        [SerializeField]
        public float cableWidth = 0.2f;
        
        public delegate void CableCancelledDelegate();
        public event CableCancelledDelegate CableCancelled;
        public delegate void CableInitializedDelegate(Vector3 connectionPoint);
        public event CableInitializedDelegate CableStarted;
        
        public bool IsConnecting { get; set; }

        private DraggableCable _draggableCable;
        
        
        private IPowerGridUser _currentActiveGridUser;
        private IPowerGridUser CurrentActiveGridUser
        {
            get => _currentActiveGridUser;
            set
            {
                _currentActiveGridUser = value;
                if (value == null)
                {
                    CableCancelled?.Invoke();
                }
                else
                {
                    CableStarted?.Invoke(value.ConnectionPoint);
                }
            }
        }

        private void Awake()
        {
            CreateDraggableCable();
            
            CableStarted += InitialiseDraggableCable;
            CableCancelled += DisableDraggableCable;
        }
        
        private void OnDestroy()
        {
            CableStarted -= InitialiseDraggableCable;
            CableCancelled -= DisableDraggableCable;
        }

        public void HandleCableActionFor(IPowerGridUser activatedGridUser)
        {
            if (CurrentActiveGridUser == null)
            {
                CurrentActiveGridUser = activatedGridUser;
            }
            else
            {                
                var current = CurrentActiveGridUser;

                if (!current.PowerGrid.HaveDirectConnection(current, activatedGridUser))
                {
                    ConnectGridUser(activatedGridUser);
                }
                else
                {
                    DisconnectGridUser(activatedGridUser);
                }
            }
        }

        private void ConnectGridUser(IPowerGridUser activatedGridUser)
        {
            var powerGrid = GetPowerGrid(CurrentActiveGridUser, activatedGridUser);

            powerGrid?.Connect(CurrentActiveGridUser, activatedGridUser);

            CurrentActiveGridUser = activatedGridUser;
        }
 
        private PowerGrid GetPowerGrid(IPowerGridUser previous, IPowerGridUser current)
        {
            PowerGrid targetGrid;

            if (previous.PowerGrid == current.PowerGrid)
            {
                return previous.PowerGrid;
            }

            if (previous.PowerGrid.CurrentPower >= current.PowerGrid.CurrentPower)
            {
                previous.PowerGrid.DevourGrid(current.PowerGrid);
                targetGrid = previous.PowerGrid;
            }
            else
            {
                current.PowerGrid.DevourGrid(previous.PowerGrid);
                targetGrid = current.PowerGrid;
            }

            return targetGrid;
        }

        private void DisconnectGridUser(IPowerGridUser activatedGridUser)
        {
            var previous = CurrentActiveGridUser;

            previous?.PowerGrid.Disconnect(previous, activatedGridUser);

            CurrentActiveGridUser = activatedGridUser;
        }

        public void CancelCurrentAction()
        {
            _draggableCable.Disable();
            CurrentActiveGridUser = null;
        }
        
        private void InitialiseDraggableCable(Vector3 point)
        {
            _draggableCable.Initialise(point);
        }

        private void DisableDraggableCable()
        {
            _draggableCable.Disable();
        }

        private void CreateDraggableCable()
        {
            _draggableCable = Instantiate(draggableCablePrefab);

            if (_draggableCable.lineRenderer == null)
            {
                _draggableCable.lineRenderer = _draggableCable.GetComponent<LineRenderer>();
            }
            
            _draggableCable.lineRenderer.startWidth = cableWidth;
            _draggableCable.lineRenderer.endWidth = cableWidth;
            _draggableCable.lineRenderer.material.color = cableColor;
        }

        public PowerGridCable DrawConnectionCable(IPowerGridUser start, IPowerGridUser end)
        {
            var cable = Instantiate(powerGridCablePrefab);

            if (cable.LineRenderer == null)
            {
                cable.LineRenderer = cable.GetComponent<LineRenderer>();
            }
            
            cable.Start = start;
            cable.End = end;
            
            cable.LineRenderer.startWidth = cableWidth;
            cable.LineRenderer.endWidth = cableWidth;
            
            cable.LineRenderer.material.color = cableColor;
            
            cable.LineRenderer.positionCount = 2;
            cable.LineRenderer.SetPosition(0, start.ConnectionPoint);
            cable.LineRenderer.SetPosition(1, end.ConnectionPoint);

            return cable;
        }
    }
}