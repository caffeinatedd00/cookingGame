using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ShopButtonControl : MonoBehaviour
{
    [SerializeField] private GameObject grid;
    public List<Button> InteractbleButtons = new List<Button>();

    public void OnToggle(bool isOn) //when shop is toggled on, turns on all storage buttons
    {
        foreach (StorageButtons sb in grid.GetComponentsInChildren<StorageButtons>(true))
        {
            Button btn = sb.GetComponent<Button>();

            if (isOn)
            {
                btn.interactable = true;
            }
            else
            {
                btn.interactable = sb.InStock;
            }
        }
    }

}