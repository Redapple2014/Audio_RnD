using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ListViewItemModel
{
    private string data;
    private int id;

    public string Data { get { return data; } }
    public int Id { get { return id; } }

    public ListViewItemModel(string data,int id)
    {
        this.data = data;
        this.id = id;
    }
}
