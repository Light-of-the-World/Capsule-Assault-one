using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SensitivitySliderScript : MonoBehaviour
{
    [SerializeField] private Slider sensSlider;
    [SerializeField] private TextMeshProUGUI sensText;

    private void Start()
    {
        sensSlider.onValueChanged.AddListener((v) =>
        {
            sensText.text = v.ToString("0.00");
        });
    }
}
