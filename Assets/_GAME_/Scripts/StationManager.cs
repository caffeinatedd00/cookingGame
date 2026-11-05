using JetBrains.Annotations;
using NUnit.Framework;
using System.Collections.Generic;
using System.Data.Common;
using UnityEngine;
public enum StationStatus
{
    Idle,
    Cooking,
    Ready
}


public class StationManager : MonoBehaviour
{
    [SerializeField] private DatabaseManager dbManager;

    private List<string> Station = new List<string>();
    public Dictionary<string, List<DatabaseManager.Recipe>> StationRecipes = new Dictionary<string, List<DatabaseManager.Recipe>>();

    void Start() //creates a dictonary that uses station as key and list of recipes as values
    {
        Station.Clear();
        StationRecipes.Clear();
        Station = dbManager.LoadStations();
        foreach (string item in Station)
        {
            var list = dbManager.LoadStationRecipes(item);
            Debug.Log($"Station '{item}' recipe count: {list.Count}");
            StationRecipes.Add(item, list);
        }
    }
}
