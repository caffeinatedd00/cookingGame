using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CloseButtonControl : MonoBehaviour
{
    [SerializeField] private Button buyButton;
    [SerializeField] private GameObject storagePanel;
    [SerializeField] private Toggle shop;
    [SerializeField] private TMP_Text description;
    [SerializeField] private TMP_Text currentItemName;

    public void OnClick() //sets the UI elemets to false when the storage panel is closed
    {
        buyButton.gameObject.SetActive(false);
        storagePanel.gameObject.SetActive(false);
        shop.isOn = false;
        description.text = " ";
        currentItemName.text = " ";
    }
}
