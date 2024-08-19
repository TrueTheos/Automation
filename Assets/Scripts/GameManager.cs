using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private MapManager _mapManager;
    private MapGenerator _mapGenerator;

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
