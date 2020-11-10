using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ListViewItemPool : MonoBehaviour
{
    private int poolSize;
    [SerializeField] GameObject PoolObjectPrefab;

    int head = 0;

    public void SetUpPool(int poolSize)
    {
        head = 0;
        this.poolSize = poolSize;
        for (int i = 0; i < poolSize; i++)
        {
            GameObject poolObj = Instantiate(PoolObjectPrefab) as GameObject;
            poolObj.transform.SetParent(this.transform);
            poolObj.SetActive(false);
        }
    }

    public GameObject ItemBorrow()
    {
        if (head >= poolSize)
        {
            return null;
        }

        head++;
        return this.transform.GetChild(0).gameObject;
    }

    public void ItemReturn(GameObject go)
    {
        if (head <= 0)
        {
            return;
        }

        head--;
        go.SetActive(false);
        go.transform.SetParent(this.transform);
    }
}
