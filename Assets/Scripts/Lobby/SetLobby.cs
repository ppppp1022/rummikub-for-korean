using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SetLobby : MonoBehaviour
{
    [SerializeField] private GameObject SettingUIObj;
    [SerializeField] private Slider SoundSlider;
    void Start()
    {
        SoundSlider.value = GameObject.Find("LobbyButtonListener").GetComponent<PlayerSetting>().soundVolume;
    }
    public void SettingLobby()
    {
        ButtonListener buttonListener = GameObject.Find("LobbyButtonListener").GetComponent<ButtonListener>();
        buttonListener.SetInitialSettings();
    }
    public void OnSettingUI()
    {
        SettingUIObj.SetActive(true);
    }
    public void UpdateSound()
    {
        GameObject.Find("LobbyButtonListener").GetComponent<PlayerSetting>().UpdateSound(SoundSlider.value);
    }
    public void PlaySound()
    {
        GameObject.Find("LobbyButtonListener").GetComponent<AudioSource>().Play();
    }
}
