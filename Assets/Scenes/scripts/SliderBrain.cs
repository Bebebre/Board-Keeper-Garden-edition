using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class SliderBrain : MonoBehaviour
{
    [SerializeField]
    private string Name = "Default";
    [SerializeField]
    private AudioMixer mixer;

    private Slider slider;
    private void Awake()
    {
        slider = this.GetComponent<Slider>();
        mixer.GetFloat(Name, out float a);
        slider.value = a;
        slider.onValueChanged.AddListener(Change);
    }

    private void Change(float Vol)
    {
        mixer.SetFloat(Name, Vol);
        if (Vol < -38) mixer.SetFloat(Name, -80);
    }
}
