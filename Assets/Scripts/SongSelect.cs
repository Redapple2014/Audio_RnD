using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SongSelect : MonoBehaviour {
    [SerializeField] private TextMeshProUGUI songNameText;
	public void SelectME()
	{
	    SoundManager.Instance.SelectSongByName(songNameText.text);
		Debug.Log(songNameText.text);
	}

}
