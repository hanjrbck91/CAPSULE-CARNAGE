using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Audio;

public class UISoundController : MonoBehaviour
{
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private GameObject toggleImage;
    public OnbuttonClickSound sound;

    private void Start()
    {
        sound = GetComponent<OnbuttonClickSound>();
    }
    public void SetMasterVolume(float sliderValue)
    {
        audioMixer.SetFloat("MasterVol", Mathf.Log10(sliderValue) * 20);
    }

    public void SetBackgroundVolume(float sliderValue)
    {
        audioMixer.SetFloat("BackgroundVol",Mathf.Log10(sliderValue) * 20);
    }
    public void ToggleSFXSvolume()
    {
        sound.SfxSound.mute = !sound.SfxSound.mute;
        toggleImage.SetActive(!sound.SfxSound.mute);
    }
}
