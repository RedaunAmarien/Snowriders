using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioMaker : MonoBehaviour
{

    [SerializeField] private float vol;

    [SerializeField] private AudioClip trickStart;
    [SerializeField] private AudioClip trickSuccess;
    [SerializeField] private AudioClip coinPickup;
    [SerializeField] private AudioClip reflectAttack;
    [SerializeField] private AudioClip error;
    [SerializeField] private AudioClip collectItem;

    private AudioSource audioSource;

    void Start()
    {
        audioSource = this.GetComponent<AudioSource>();
    }

    public void Play(string clipToPlay)
    {
        switch (clipToPlay)
        {
            case "trickstart":
                audioSource.PlayOneShot(trickStart, vol);
                break;
            case "trickwin":
                audioSource.PlayOneShot(trickSuccess, vol);
                break;
            case "coinPickup":
                audioSource.PlayOneShot(coinPickup, vol);
                break;
            case "reflectAttack":
                audioSource.PlayOneShot(reflectAttack, vol);
                break;
            case "error":
                audioSource.PlayOneShot(error, vol);
                break;
            case "collectItem":
                audioSource.PlayOneShot(collectItem, vol);
                break;
        }
    }
}
