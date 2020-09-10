using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioMaker : MonoBehaviour {

	public AudioClip trickStart, trickSuccess, coin, reflect, error, item;
	AudioSource sourcer;
	public float vol;
	void Start() {
		sourcer = this.GetComponent<AudioSource>();
	}

	public void Play(string clipToPlay) {
		switch(clipToPlay) {
			case "trickstart":
			sourcer.PlayOneShot(trickStart, vol);
			break;
			case "trickwin":
			sourcer.PlayOneShot(trickSuccess, vol);
			break;
			case "coin":
			sourcer.PlayOneShot(coin, vol);
			break;
			case "reflect":
			sourcer.PlayOneShot(reflect, vol);
			break;
			case "error":
			sourcer.PlayOneShot(error, vol);
			break;
			case "item":
			sourcer.PlayOneShot(item, vol);
			break;
		}
	}
}
