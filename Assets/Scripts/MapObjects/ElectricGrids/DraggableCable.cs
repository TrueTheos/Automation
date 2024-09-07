using UnityEngine;

namespace MapObjects.ElectricGrids
{
    public class DraggableCable : MonoBehaviour
    {
        private Vector3 _startPoint;
        private Camera _camera;

        [SerializeField]
        public LineRenderer lineRenderer;
        
        public bool Initialised { get; set; }

        private void Start()
        {
            _camera = Camera.main;
            
            _startPoint = transform.position;
            
            if (lineRenderer == null)
            {
                lineRenderer = GetComponent<LineRenderer>();
            }
        }

        public void Initialise(Vector3 startPos)
        {
            lineRenderer.positionCount = 2;
            lineRenderer.SetPosition(0, startPos);
            lineRenderer.SetPosition(1, startPos);
            
            Initialised = true;
        }

        private void Update()
        {
            if (!Initialised)
            {
                return;
            }
            
            var mousePos = _camera.ScreenToWorldPoint(Input.mousePosition);
            lineRenderer.SetPosition(1, new Vector3(mousePos.x, mousePos.y, _startPoint.z));
        }

        public void Disable()
        {
            Initialised = false;
            lineRenderer.positionCount = 0;
        }
    }
}