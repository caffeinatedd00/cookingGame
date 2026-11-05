using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using System;

public class BuyingManager : MonoBehaviour
{
    [SerializeField] private TMP_Text itemName;
    [SerializeField] private DatabaseManager dbManager;
    [SerializeField] private StorageManager storageManager;
    [SerializeField] private ShopButtonControl shopButtonController;
    [SerializeField] private Toggle toggle;
    [SerializeField] private TMP_Text moneyText;

    public List<DatabaseManager.Ingredients> Ingredients = new List<DatabaseManager.Ingredients>();

    public void buyItem() //gets the item name, id and how much is bought from database and adds to its amount and reduces the cost from money
    {
        
        string buyingItem = itemName.text;
        int boughtItemId = dbManager.FindId(buyingItem);
        int boughQty = dbManager.FindQuantity(boughtItemId);
        Ingredients = dbManager.LoadIngredients();
        int cost = dbManager.FindCost(Ingredients, boughtItemId, 0, Ingredients.Count - 1);
        if (dbManager.MoneyAmount() > cost)
        {
            dbManager.ReduceMoney(cost);
            moneyText.text = $"{dbManager.MoneyAmount()}";
            dbManager.UpdateQuantity(boughtItemId, boughQty);
            storageManager.BindButtons();
            toggle.isOn = false;
            toggle.isOn = true;
            shopButtonController.OnToggle(toggle.isOn);
        }
        else
        {
            Debug.Log("no money");
        }

    }
}
 