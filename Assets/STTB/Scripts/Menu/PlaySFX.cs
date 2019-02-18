using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaySFX : MonoBehaviour {

    public AudioClip clip_ButtonClick;
    public AudioClip clip_ButtonAfter;
    public void PlayButtonClick()
    {
        GameManager.active.audio_SFX.PlayOneShot(clip_ButtonClick);
    }
    public void PlayButtonAfter()
    {
        GameManager.active.audio_SFX.PlayOneShot(clip_ButtonAfter);
    }
}
