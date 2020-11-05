using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class FolderItemSelection : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI folderName;
    [SerializeField] private Toggle toggle;
    private bool isClicked = true;
    private string fullDirectoryName;
    public TextMeshProUGUI FolderName { get => folderName; set => folderName = value; }
    public string FullDirectoryName { get => fullDirectoryName; set => fullDirectoryName = value; }
    
    private void OnEnable()
    {
        toggle.onValueChanged.AddListener(SetFolderStatus);
    }
    private void OnDisable()
    {
        toggle.onValueChanged.RemoveListener(SetFolderStatus);
    }
    private void Start()
    {
        toggle.isOn = false;
    }
    public void SetFolderStatus(bool toggleStatus)
    {
        isClicked = toggleStatus;
    }
    public string GetFolderName()
    {
        if(isClicked)
        {
            return fullDirectoryName;
        }
        else
        {
            return string.Empty;
        }
    }
}
