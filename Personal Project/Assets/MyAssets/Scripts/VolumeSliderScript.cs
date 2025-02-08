using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class VolumeSliderScript : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    [SerializeField] private Slider volumeSlider;
    [SerializeField] private TextMeshProUGUI volumeText;

    private void Start()
    {
        volumeSlider.onValueChanged.AddListener((v) =>
        {
            volumeText.text = v.ToString("0.00");
        });
    }
}
