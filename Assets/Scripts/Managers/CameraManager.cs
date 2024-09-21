using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;

namespace Assets.Scripts.Managers
{
    public class CameraManager : MonoBehaviour
    {
        public static CameraManager Instance;

        [HideInInspector] public Camera MainCam;
        [SerializeField] private int _minZoom;
        [SerializeField] private int _maxZoom;
        [SerializeField] private float _zoomSpeed;
        [SerializeField] private float _startZoom;
        private float _currentZoom;

        private void Awake()
        {
            Instance = this;
            MainCam = Camera.main;

            MainCam.orthographicSize = _startZoom;
            _currentZoom = _startZoom;
        }

        void Update()
        {
            float zoomInput = Input.GetAxis("Mouse ScrollWheel");
            if (Input.GetKey(PlayerKeybinds.ZoomKey) && zoomInput != 0)
            {
                Zoom(zoomInput);
            }
        }

        public void Zoom(float zoomDirection)
        {
            _currentZoom += zoomDirection * _zoomSpeed;
            _currentZoom = Mathf.Clamp(_currentZoom, _minZoom, _maxZoom);
            MainCam.orthographicSize = _currentZoom;
        }
    }
}
