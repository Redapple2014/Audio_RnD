using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[Serializable]
public class ListViewItem : MonoBehaviour
{

    [SerializeField] TextMeshProUGUI itemText;

    public int ItemHeight { get { return Mathf.CeilToInt(GetComponent<RectTransform>().rect.height); }}

    public void Setup(ListViewItemModel model)
    {
        gameObject.name = model.Id.ToString();
        itemText.text = model.Data.ToString();
    }
}
