using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSetting : MonoBehaviour
{
    public float soundVolume = 75;
    public void UpdateSound(float volume)
    {
        soundVolume = volume;
        GetComponent<AudioSource>().volume = volume;
    }
}
