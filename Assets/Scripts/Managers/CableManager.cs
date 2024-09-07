using System.Collections.Generic;

using MapObjects.ElectricGrids;

using UnityEngine;

namespace Managers
{
    public class CableManager : MonoBehaviour
    {
        [SerializeField]
        private DraggableCable draggableCablePrefab;

        private DraggableCable _draggableCable;

        [SerializeField]
        public Color cableColor = Color.red;

        [SerializeField]
        public float cableWidth = 0.2f;
        
        [SerializeField]
        private LineRenderer staticCablePrefab;
        
        public delegate void CableCancelledDelegate();
        public event CableCancelledDelegate CableCancelled;

        public delegate void CableInitializedDelegate(Vector3 connectionPoint);
        public event CableInitializedDelegate CableStarted;
        
        private IPowerGridUser _currentObjectBeingConnected;
        public IPowerGridUser CurrentObjectBeingConnected
        {
            get => _currentObjectBeingConnected;
            set
            {
                _currentObjectBeingConnected = value;
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
        public bool IsConnecting { get; set; }

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

        public void HandleConnectingGridUsers(IPowerGridUser newCurrentGridUser)
        {
            if (CurrentObjectBeingConnected == null)
            {
                CurrentObjectBeingConnected = newCurrentGridUser;
            }
            else
            {
                var previous = CurrentObjectBeingConnected;

                if (previous != null)
                {
                    var connectionCable = DrawConnectionCable(
                        newCurrentGridUser.ConnectionPoint,
                        previous.ConnectionPoint);
                    
                    previous.ConnectUsers(newCurrentGridUser, connectionCable);
                }

                CurrentObjectBeingConnected = newCurrentGridUser;
            }
        }

        public void CancelCurrentAction()
        {
            _draggableCable.Disable();
            CurrentObjectBeingConnected = null;
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
            _draggableCable.lineRenderer.startWidth = cableWidth;
            _draggableCable.lineRenderer.endWidth = cableWidth;
            _draggableCable.lineRenderer.material.color = cableColor;
        }

        private LineRenderer DrawConnectionCable(Vector3 startPoint, Vector3 endPoint)
        {
            var lineRenderer = Instantiate(staticCablePrefab);
            
            lineRenderer.startWidth = cableWidth;
            lineRenderer.endWidth = cableWidth;
            lineRenderer.material.color = cableColor;
            
            lineRenderer.positionCount = 2;
            lineRenderer.SetPosition(0, startPoint);
            lineRenderer.SetPosition(1, endPoint);

            return lineRenderer;
        }
    }
}