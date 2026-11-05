using System;
using TMPro;
using UnityEngine;

public class CustomerSpawnManager : MonoBehaviour
{
    [SerializeField] private GameObject customer;
    [SerializeField] private ObstacleTilemap obstacleTilemap;
    [SerializeField] private SeatingTilemap seatingTilemap;
    [SerializeField] private DatabaseManager databaseManager;
    [SerializeField] private TMP_Text moneyText;
    [SerializeField] public Transform spawnPoint;

    private float spawnTime;

    void Start()
    {
        spawnTime = UnityEngine.Random.Range(5f, 25f); //customers spawn in this set random time interval
    }

    void Update()
    {
        if (spawnTime > 0)
        {
            spawnTime -= Time.deltaTime;
        }
        if (spawnTime <= 0)
        {
            Spawn();
            spawnTime = UnityEngine.Random.Range(3, 25); //spwans at the entrance
        }

    }

    private void Spawn()
    {
        var spawn = Instantiate(customer, spawnPoint.position, spawnPoint.rotation);
        
        Debug.Log("Customer spawned");
        Debug.Log("spawn location: " + spawn.transform.position);
        var customerBehaviour = spawn.GetComponent<CustomerBehaviour>();
        customerBehaviour.Initialize(obstacleTilemap, seatingTilemap, spawnPoint, databaseManager, moneyText);  //initialises import information for the script 
    }
}
