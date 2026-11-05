using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Xml;

public class StorageButtons : MonoBehaviour
{
    [SerializeField] private TMP_Text quantityText;
    [SerializeField] private Image Icon;
    [SerializeField] private Button button;
    [SerializeField] private TMP_Text descriptionText;  
    [SerializeField] private Button closeButton;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private Toggle shop;
    [SerializeField] private GrabManager grabManager;

    private int click;
    private string dataDescription;
    private string dataName;
    private int ingredient_id;
    public bool InStock { get; private set; }
    public void SetData(DatabaseManager.StorageSlot data) //sets information for storage button
    {
        ingredient_id = data.id;
        dataDescription = data.description;
        dataName = data.name;

        quantityText.text = data.quantity.ToString();

        InStock = data.quantity > 0;

        if (InStock)
        {
            button.interactable = true;
            Icon.color = Color.green;
        }
        else
        {
            button.interactable = false;
            Icon.color = Color.red;
        }
    }

    public void OnClick() //manages double click mechanic - if clicked once show info and if clicked again on same item, grab
    {
        if (grabManager.itemGrabbed == false)
        {
            Debug.Log("shop status: " + shop.isOn);
            if (shop.isOn)
            {
                FirstClick();
            }
            else
            {
                click++;
                Debug.Log("click: " + click);

                if (click == 1 || nameText.text != dataName)
                {
                    click = 1;
                    FirstClick();
                }
                if (click == 2)
                {
                    Debug.Log("second click");
                    SecondClick();
                }
            }
        }
        else
        {
            FirstClick();
            Debug.Log("item already grabbed");
        }
    }

    public void FirstClick() //shows item info on first click
    {
        descriptionText.text = dataDescription;
        nameText.text = dataName;
        Debug.Log("first click");
    }

    public void SecondClick() //grabes item and close the storage panel on second click
    {
        closeButton.onClick.Invoke();
        click = 0;
        grabManager.GrabItem(ingredient_id, dataName);
        descriptionText.text = " ";
        nameText.text = " ";
    }

}
