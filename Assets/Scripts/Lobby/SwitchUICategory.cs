using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchUICategory : MonoBehaviour
{
    [SerializeField] private GameObject SoundUI;
    [SerializeField] private GameObject LanguageUI;
    [SerializeField] private GameObject VersionUI;
    public void TurnOnSoundUI()
    {
        SoundUI.SetActive(true);
        LanguageUI.SetActive(false);
        VersionUI.SetActive(false);
    }
    public void TurnOnLanguageUI()
    {
        SoundUI.SetActive(false);
        LanguageUI.SetActive(true);
        VersionUI.SetActive(false);
    }
    public void TurnOnVersionUI()
    {
        SoundUI.SetActive(false);
        LanguageUI.SetActive(false);
        VersionUI.SetActive(true);
    }
}
