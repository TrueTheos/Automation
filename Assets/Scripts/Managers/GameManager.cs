using Assets.Scripts.MapObjects;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Managers
{
    public class GameManager : MonoBehaviour
    {
        public Transform ObjectHighlight;

        public Player CurrentPlayer { get; private set; }

        public static GameManager Instance;
        private void Awake()
        {
            Instance = this;
            CurrentPlayer = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
        }

        void Start()
        {
            Init();         
        }

        private void Init()
        {
            MapGenerator.Instance.Generate();
        }
    }
}