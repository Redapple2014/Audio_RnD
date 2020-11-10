using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PooledListView : MonoBehaviour, IBeginDragHandler
{
    #region Child Components
    [SerializeField] ScrollRect ScrollRect;
    [SerializeField] RectTransform viewPortT;
    [SerializeField] RectTransform DragDetectionT;
    [SerializeField] RectTransform ContentT;
    [SerializeField] ListViewItemPool ItemPool;
    [SerializeField] RectTransform ItemTransform;
    #endregion

    #region Layout Parameters
    [SerializeField] int BufferSize;
    #endregion

    #region Layout Variables
    int ItemHeight { get { return Mathf.CeilToInt(ItemTransform.rect.height); } }
    int VisibleItemCount { get { return Mathf.CeilToInt(viewPortT.rect.height / ItemHeight); } }
    int TopItemOutOfView { get { return Mathf.CeilToInt(ContentT.anchoredPosition.y / ItemHeight); } }
    float dragDetectionAnchorPreviousY = 0;
    #endregion

    #region Data
    ListViewItemModel[] data;
    int dataHead = 0;
    int dataTail = 0;
    #endregion   

    public void Setup(ListViewItemModel[] data)
    {
        ScrollRect.onValueChanged.AddListener(OnDragDetectionPositionChange);

        this.data = data;

        DragDetectionT.sizeDelta = new Vector2(DragDetectionT.sizeDelta.x, this.data.Length * ItemHeight);
        Debug.Log(VisibleItemCount);
        int lenght = 0;
        if(data.Length < VisibleItemCount+BufferSize)
        {
            lenght = data.Length;
        }
        else
        {
            lenght = VisibleItemCount+BufferSize;
        }
        for(int i = 0; i < lenght; i++)
        {
            GameObject itemGO = ItemPool.ItemBorrow();
            itemGO.transform.SetParent(ContentT);
            itemGO.SetActive(true);
            itemGO.transform.localScale = Vector3.one;
            itemGO.GetComponent<ListViewItem>().Setup(data[dataTail]);
            dataTail++;
        }
    }

    #region UI Event Handling

    public void OnDragDetectionPositionChange(Vector2 dragNormalizePos)
    {
        float dragDelta = DragDetectionT.anchoredPosition.y - dragDetectionAnchorPreviousY;

        ContentT.anchoredPosition = new Vector2(ContentT.anchoredPosition.x, ContentT.anchoredPosition.y + dragDelta);

        UpdateContentBuffer();

        dragDetectionAnchorPreviousY = DragDetectionT.anchoredPosition.y;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        dragDetectionAnchorPreviousY = DragDetectionT.anchoredPosition.y;
    }
    #endregion

    #region Infinite Scroll Mechanism

    void UpdateContentBuffer()
    {
        if(TopItemOutOfView > BufferSize)
        {
            if(dataTail >= data.Length)
            {
                return;
            }

            Transform firstChildT = ContentT.GetChild(0);
            firstChildT.SetSiblingIndex(ContentT.childCount - 1);
            firstChildT.gameObject.GetComponent<ListViewItem>().Setup(data[dataTail]);
            ContentT.anchoredPosition = new Vector2(ContentT.anchoredPosition.x, ContentT.anchoredPosition.y - firstChildT.gameObject.GetComponent<ListViewItem>().ItemHeight);
            dataHead++;
            dataTail++;
        }
        else if(TopItemOutOfView < BufferSize)
        {
            if(dataHead <= 0)
            {
                return;
            }

            Transform lastChildT = ContentT.GetChild(ContentT.childCount - 1);
            lastChildT.SetSiblingIndex(0);
            dataHead--;
            dataTail--;
            lastChildT.gameObject.GetComponent<ListViewItem>().Setup(data[dataHead]);
            ContentT.anchoredPosition = new Vector2(ContentT.anchoredPosition.x, ContentT.anchoredPosition.y + lastChildT.gameObject.GetComponent<ListViewItem>().ItemHeight);

        }
    }
    #endregion
}
