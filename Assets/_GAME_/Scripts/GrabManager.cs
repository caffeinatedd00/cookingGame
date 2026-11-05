using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GrabManager : MonoBehaviour
{
    [SerializeField] public GameObject GrabbedItem;

    [SerializeField] private DatabaseManager dbManager;
    [SerializeField] private StorageManager storageManager;
    [SerializeField] private StationManager stationManager;

    [SerializeField] private GameObject information;
    [SerializeField] private GameObject ingreidentBox;
    [SerializeField] private GameObject stationBox;

    [SerializeField] private TMP_Text GrabbedItemText;
    [SerializeField] private TMP_Text Ingredients;
    [SerializeField] private TMP_Text Recipes;
    [SerializeField] private TMP_Text Station;


    public bool itemGrabbed;
    public int grabbedItemId;
    public string grabbedItemName;
    public List<string> RecipesForIngredient = new List<string>();
    public List<string> ingredientNames = new List<string>();

    public void GrabItem(int id, string name) //grabs an item and display what the item is through the discription box
    {
        grabbedItemId = id;
        grabbedItemName = name;

        GrabbedItem.SetActive(true);
        itemGrabbed = true;

        if (id != 0) //if id is 0 that means player is grabbing a non ingredient
        {
            GrabbedItemText.text = name;
            ingreidentBox.SetActive(true);
            stationBox.SetActive(true);
            dbManager.UpdateQuantity(id, -1);
            storageManager.BindButtons();
            possibleRecipes();
            UpdateRecipesText();
            SetInfo(true);
        }
        else
        {
            GrabbedItemText.text = name;
            Recipes.text = "holding cooked item";
            SetInfo(true);
            ingreidentBox.SetActive(false);
            stationBox.SetActive(false);
        }
    }
    public void possibleRecipes() //finds possible recipies with the ingredient currently on hold by going through the station-recipie dictionary
    {
        int ingredientID = grabbedItemId;
        ingredientNames.Clear();
        RecipesForIngredient.Clear();
        foreach (var station in stationManager.StationRecipes)
        {
            string stationHint = station.Key;
            List<DatabaseManager.Recipe> recipiesForStation = stationManager.StationRecipes[stationHint];
            for (int j = 0; j < recipiesForStation.Count; j++)
            {
                if (recipiesForStation[j].ingredientsRequired.Contains(ingredientID))
                {
                    Station.text = recipiesForStation[j].station;
                    RecipesForIngredient.Add(recipiesForStation[j].name);
                    Debug.Log("ADED" +  recipiesForStation[j].name);
                    List<DatabaseManager.Ingredients> ingredients = dbManager.LoadIngredients();
                    foreach (int id in recipiesForStation[j].ingredientsRequired)
                    {
                        string entry = dbManager.FindName(ingredients, id, 0, ingredients.Count-1);
                        ingredientNames.Add(entry);
                    }
                }
            }
        }

    }
    public void UpdateRecipesText() //updates the recipe box
    {
        if (RecipesForIngredient.Count == 0)
        {
            Recipes.text = "No recipes";
            Ingredients.text = "Non";
            return;
        }

        Recipes.text = string.Join("\n", RecipesForIngredient);
        Ingredients.text = string.Join("\n", ingredientNames);
    }

    public void SetInfo(bool isItemGrabbed) //activates or closes the information panel - if close, clears the texts 
    {
        information.SetActive(isItemGrabbed);
        if (!isItemGrabbed)
        {
            GrabbedItemText.text = " ";
            Recipes.text = " ";
        }
        
    }
}