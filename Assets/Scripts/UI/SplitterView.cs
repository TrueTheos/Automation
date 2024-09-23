using Assets.Scripts;
using Assets.Scripts.MapObjects;
using Assets.Scripts.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SplitterView : BuildingView
{
    public ItemTypeSlot OutputAFilter;
    public ItemTypeSlot OutputBFilter;

    private SplitterObject _splitter;

    private void Start()
    {
        OutputAFilter.OnItemChangeEvent += UpdateSplitter;
        OutputBFilter.OnItemChangeEvent += UpdateSplitter;
    }

    private void UpdateSplitter()
    {
        if (_splitter != null)
        {
            _splitter.FilterA = OutputAFilter.CurrentItem;
            _splitter.FilterB = OutputBFilter.CurrentItem;
        }
    }

    public void OpenSplitter(SplitterObject splitter)
    {
        _splitter = splitter;
        ResetUI();
        Open();
        OutputAFilter.Init(_splitter.FilterA);
        OutputBFilter.Init(_splitter.FilterB);
    }

    public override void ResetUI()
    {
        OutputAFilter.ResetSlot();
        OutputBFilter.ResetSlot();
    }
}
