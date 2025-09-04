using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndingSound : MonoBehaviour
{
    public List<AudioClip> audioClips;
    [SerializeField] private AudioSource audioSource;
    public void PlaySound(int type)
    {
        audioSource.PlayOneShot(audioClips[type]);
    }
}
