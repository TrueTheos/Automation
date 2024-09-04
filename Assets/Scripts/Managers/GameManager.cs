using Assets.Scripts.MapObjects;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Managers
{
    public class GameManager : MonoBehaviour
    {
        private MapManager _mapManager;
        private MapGenerator _mapGenerator;

        public Transform ObjectHighlight;

        public static GameManager Instance;
        private void Awake()
        {
            Instance = this;
        }

        void Start()
        {
            _mapManager = MapManager.Instance;
            _mapGenerator = MapGenerator.Instance;

            Init();
        }

        private void Init()
        {
            _mapGenerator.Generate();
        }
    }
}