using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using System;

public class ButtonSelectionSound : Button
{
    [SerializeField] private AudioMixerGroup audioMixer;
    [SerializeField] private MusicType musicType;
    public static Action<AudioMixerGroup, MusicType> onAudioSelected;
    public void ChangeAudioEffect()
    {
        onAudioSelected.Invoke(audioMixer, musicType);
    }
}
public enum MusicType
{
    Normal, Surround
}
