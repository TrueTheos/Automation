using Assets.Scripts.MapObjects;
using Assets.Scripts.UI;
using Assets.Scripts;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class CombinerView : BuildingView
{
    private CombinerObject _combiner;
    [SerializeField] private TextMeshProUGUI _text;
    [SerializeField] private Slider _slider;

    private void Start()
    {
        _slider.onValueChanged.AddListener(delegate { OnSliderChange(); });
    }

    private void OnSliderChange()
    {
        if(_combiner != null)
        {
            if (_slider.value == 0)
            {
                _combiner.InputPriority = CombinerObject.BeltPriority.A;
                _text.text = "Priority: <b>A</b>";
            }
            if (_slider.value == 1)
            {
                _combiner.InputPriority = CombinerObject.BeltPriority.None;
                _text.text = "Priority: <b>None</b>";
            }
            if (_slider.value == 2)
            {
                _combiner.InputPriority = CombinerObject.BeltPriority.B;
                _text.text = "Priority: <b>B</b>";
            }
        }
    }

    private void UpdateSplitter()
    {
        if (_combiner != null)
        {
            if (_combiner.InputPriority == CombinerObject.BeltPriority.A) _slider.value = 0;
            if (_combiner.InputPriority == CombinerObject.BeltPriority.None) _slider.value = 1;
            if (_combiner.InputPriority == CombinerObject.BeltPriority.B) _slider.value = 2;
        }
    }

    public void OpenCombiner(CombinerObject combiner)
    {
        _combiner = combiner;
        ResetUI();
        Open();
        UpdateSplitter();
    }

    public override void ResetUI() { }
}
