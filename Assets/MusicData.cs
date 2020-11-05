using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicData : MonoBehaviour
{
    [CreateAssetMenu(fileName = "MusicData", menuName = "ScriptableObjects/MusicScriptableObject", order = 1)]
    public class MusicScriptableObject : ScriptableObject
    {
        public List<string> frequencies = new List<string>();
        public List<string> notes = new List<string>();
    }
}
