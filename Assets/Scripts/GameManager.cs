using Assets.Scripts.MapObjects;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private MapManager _mapManager;
    private MapGenerator _mapGenerator;

    public static GameManager Instance;

    private List<ConveyorBeltObject> _conveyorBelts = new();

    private float _moveInterval = 1f;
    private float _timeSinceLastMove = 0f;

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

    private void Update()
    {
        _timeSinceLastMove += Time.deltaTime;
        if (_timeSinceLastMove >= _moveInterval)
        {
            UpdateBelts();
            _timeSinceLastMove = 0f;
        }
    }

    private void UpdateBelts()
    {
        foreach (ConveyorBeltObject belt in _conveyorBelts)
        {
            belt.MoveItems();
        }
    }

    public void AddBelt(ConveyorBeltObject belt)
    {
        _conveyorBelts.Add(belt);
    }

    public void RemoveBelt(ConveyorBeltObject belt)
    {
        _conveyorBelts.Remove(belt);
    }

    private void Init()
    {
        _mapGenerator.Generate();
    }
}
