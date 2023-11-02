using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnbuttonClickSound : MonoBehaviour
{
    public AudioSource SfxSound;
    // Start is called before the first frame update
    public void OnButtonClicked()
    {
        SfxSound.Play();
    }
}
