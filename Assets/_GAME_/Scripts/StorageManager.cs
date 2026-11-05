using UnityEngine;
using System;
using System.Data;
using System.Collections.Generic;

public class StorageManager : MonoBehaviour
{
    [SerializeField] private GameObject GridItem;
    [SerializeField] private DatabaseManager dbManager;

    public List<StorageButtons> buttons = new List<StorageButtons>();
    public List<DatabaseManager.StorageSlot> StorageData = new List<DatabaseManager.StorageSlot>();

    void Start()
    {
        StoreButtons(buttons, GridItem); 
        BindButtons();
    }

    public void StoreButtons(List<StorageButtons> buttons, GameObject obj) //store storages buttons into a list
    {
        buttons.Clear();
        if (GridItem.transform.childCount > 0)
        {
            foreach (Transform child in obj.transform)
            {
                var data = child.GetComponent<StorageButtons>();
                buttons.Add(data);
            }
        }
    }

    public void BindButtons() //adds information to storage buttons
    {
        StorageData = dbManager.LoadStorageSlot();

        for (int i = 0; i < buttons.Count; i++)
        {
            buttons[i].SetData(StorageData[i]);
        }
    }
}
