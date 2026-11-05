using System.Collections;
using System.Collections.Generic;
using System.Xml;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class GridMovementCharacter : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private Vector2 gridSize = new Vector2(1f, 1f);
    [SerializeField] private ObstacleTilemap obstacleTilemap;
    [SerializeField] private TileSelection tileSelection;

    [Header("Interaction")]
    [SerializeField] private InteractionManager interactionManager;
    [SerializeField] private GrabManager grabManager;
    [SerializeField] private StationManager stationManager;
    [SerializeField] private SeatingTilemap seatingTilemap;
    [SerializeField] private DatabaseManager databaseManager;

    [Header("Timer")]
    [SerializeField] private GameObject Timers;
    [SerializeField] private TMP_Text RiceTimer;
    [SerializeField] private TMP_Text SteamTimer;
    [SerializeField] private TMP_Text OvenTimer;
    [SerializeField] private TMP_Text FryingTimer;
    [SerializeField] private TMP_Text PlateTimer;
    [SerializeField] private TMP_Text TeaTimer;

    [Header("Customer")]
    [SerializeField] private CustomerSpawnManager CustomerSpawn;
    [SerializeField] private CustomerBehaviour customerBehaviour;

    [Header("UI")]
    [SerializeField]private GameObject storagePanel;
    [SerializeField]private GameObject pauseMenu;

    private Vector2 targetPosition;
    private bool isMoving = false;
    
    private void Update()
    {
        HandleMovementInput();
        if (storagePanel.activeSelf)
        {
            Timers.SetActive(false);
        }
        else
        {
            Timers.SetActive(true);
        }
    }

    private void HandleMovementInput() //check if player clicked on a tile and if so handles movement or interaction
    {
        if (!isMoving && Mouse.current.leftButton.wasPressedThisFrame && !storagePanel.activeSelf && !pauseMenu.activeSelf) //Check if character can move
        {
            targetPosition = tileSelection.GetHighlightedTilePos(); //where to move
            Vector2Int clickedTile = GridUtils.WorldToGrid(targetPosition);

            if (!obstacleTilemap.isTileObstacle(clickedTile))
            {
                FindPathToTargetPos();
            }
            Interaction(clickedTile);   
              
        }
    }

    private void FindPathToTargetPos() //find path to the clicked postion using A* path finder
    {
        Vector2 startPosition = GridUtils.GridToWorld(GridUtils.WorldToGrid(transform.position)); 
        List<Vector2> path = AStar.FindPath(startPosition, targetPosition, gridSize, obstacleTilemap.isTileObstacle); //Main path finder
        if (path != null && path.Count > 0)
        {
            StartCoroutine(MoveAlongPath(path)); 
        }
    }

    private IEnumerator MoveAlongPath(List<Vector2> path) //moves along found path
    {
        isMoving = true;
        int currentWayPointIndex = 0;
        while (currentWayPointIndex < path.Count)
        {
            targetPosition = path[currentWayPointIndex] + gridSize / 2;
            while ((Vector2)transform.position != targetPosition)
            {
                float step = moveSpeed + Time.fixedDeltaTime;
                transform.position = Vector3.MoveTowards(transform.position, targetPosition, step);

                yield return new WaitForFixedUpdate();
            }

            currentWayPointIndex++;
        }
        Debug.Log(path[currentWayPointIndex - 1]);
        //CheckifInInteractionProx(path[currentWayPointIndex - 1]);
        isMoving = false;
    }

    private bool CheckifInInteractionProx() //check if player near interactables
    {
        Vector2Int playerCell = GridUtils.WorldToGrid(transform.position);
        foreach (InteractionManager.InteractableObjects i in interactionManager.CanInteract)
        {
            if (i.Prox == playerCell)
            {
                return true;
            }
        }
        return false;
    }

    private bool CheckifNearSeat() //check if player is near a seat
    {
        Vector2Int playerCell = GridUtils.WorldToGrid(transform.position);
        foreach (SeatingTilemap.Seat seat in seatingTilemap.seats)
        {
            List<Vector2Int> neighbours = GetAdjacentPositions(((Vector2Int)seat.seatTile));
            if (neighbours.Contains(playerCell))
            {
                return true;
            }
        }
        return false;
    }

    private void Interaction(Vector2Int clickedTile) //handles actual interaction 
    {
        Vector2Int playerCell = GridUtils.WorldToGrid(transform.position);
        List<Vector2Int> neighbours = GetAdjacentPositions(((Vector2Int)playerCell));
        if (obstacleTilemap.isTileObstacle(clickedTile) & CheckifInInteractionProx()) //see if player clicked on an interactable object 
        {
            foreach (InteractionManager.InteractableObjects i in interactionManager.CanInteract)
            {
                if ((i.Location == targetPosition) & (i.Prox == playerCell)) //if so does accordingly to the hint
                {
                    string hint = i.Hint.ToUpper();
                    Debug.Log("player clicked: " + i.Name);

                    if ((grabManager.itemGrabbed) && (i.Status == StationStatus.Idle) && (hint != "DISCARD")) //see if they clicked on an idle station while holding an ingredient
                    {
                        List<int> ingredientsNeeded = new List<int>();
                        int ingredientID = grabManager.grabbedItemId;

                        if (stationManager.StationRecipes.ContainsKey(hint))
                        {
                            List<DatabaseManager.Recipe> recipesForStation = stationManager.StationRecipes[hint];
                            for (int j = 0; j < recipesForStation.Count; j++)
                            {
                                if (recipesForStation[j].ingredientsRequired.Contains(ingredientID)) //if the ingredient is part of the ingredients for a recipe for that station, - tick the recipe off the list
                                {                                                                    
                                    int timer = recipesForStation[j].cookingTime;
                                    ingredientsNeeded = new List<int>(recipesForStation[j].ingredientsRequired);
                                    ingredientsNeeded.Remove(ingredientID);

                                    grabManager.itemGrabbed = false;
                                    grabManager.GrabbedItem.SetActive(false);
                                    grabManager.SetInfo(false);
                                    
                                    if (ingredientsNeeded.Count == 0)//if list of needed ingredients empty, cook that recipe
                                    {
                                        i.Status = StationStatus.Cooking;
                                        i.CurrentRecipie = recipesForStation[j].name;
                                        StartCoroutine(updateTime(timer, i));
                                        return;
                                        //cook
                                    }
                                }
                            }
                        }
                    }
                    
                    if (i.Status == StationStatus.Ready) //if station finished cooking and currently not holding an item, grab the finished food
                    {
                        grabManager.GrabItem(0, i.CurrentRecipie);
                        i.CurrentRecipie = null;
                        i.Status = StationStatus.Idle;
                        alert(i, false);
                        return;
                    }
                    
                    switch (hint) 
                    {
                        case "STORAGE": //opens storage panel if clicked on storage
                            storagePanel.SetActive(true);
                            break;
                        case "DISCARD": //discard the currently holding item
                            grabManager.itemGrabbed = false;
                            grabManager.GrabbedItem.SetActive(false);
                            grabManager.SetInfo(false);
                            break;

                    }
                }
            }
        }
        else if (obstacleTilemap.isTileObstacle(clickedTile) & CheckifNearSeat() & neighbours.Contains(clickedTile)) //check if clicked tile is a seat
        {
            foreach (SeatingTilemap.Seat seat in seatingTilemap.seats)
            {
                if ((clickedTile == (Vector2Int)seat.seatTile) && (seat.isOccupied)) //if clicked seat is occupied - take order from customer or serve food if food on hold is what they ordered
                {
                    Debug.Log("Customer on seat clicked");
                    var customer = seat.currentCustomer;
                    if (customer.ordered == false)
                    {
                        customer.orderedItem = databaseManager.order();
                        customer.ordered = true;
                        Debug.Log("ordered: " + customer.orderedItem.name);
                    }
                    else if ((customer.ordered == true) && (grabManager.itemGrabbed))
                    {
                        if (grabManager.grabbedItemName == customer.orderedItem.name)
                        {
                            customer.foodServed = true;
                            grabManager.itemGrabbed = false;
                            grabManager.GrabbedItem.SetActive(false);
                            grabManager.SetInfo(false);
                        }
                    }
                }
            }
        }
    }

    private IEnumerator updateTime(int time, InteractionManager.InteractableObjects i) //updates the food timers
    {
        float timer = (float)time + 0.5f;
        while (timer > 0f) 
        {
            timer -= Time.deltaTime;
            int secondsLeft = Mathf.FloorToInt(timer);
            switch (i.Hint)
            {
                case "RICE":
                    RiceTimer.text = secondsLeft.ToString();
                    if (timer < 0f)
                    {
                        Debug.Log("finished cooking");
                        RiceTimer.text = "RiceCooker";
                        i.Status = StationStatus.Ready;
                        alert(i, true);

                    }
                    break;
                case "PLATE":
                    PlateTimer.text = secondsLeft.ToString();
                    if (timer < 0f)
                    {
                        Debug.Log("finished cooking");
                        PlateTimer.text = "Plate";
                        i.Status = StationStatus.Ready;
                        alert(i, true);
                    }
                    break;
                case "STEAM":
                    SteamTimer.text = secondsLeft.ToString();
                    if (timer < 0f)
                    {
                        Debug.Log("finished cooking");
                        SteamTimer.text = "Steamer";
                        i.Status = StationStatus.Ready;
                        alert(i, true);
                    }
                    break;
                case "OVEN":
                    OvenTimer.text = secondsLeft.ToString();
                    if (timer < 0f)
                    {
                        Debug.Log("finished cooking");
                        OvenTimer.text = "Oven";
                        i.Status = StationStatus.Ready;
                        alert(i, true);
                    }
                    break;
                case "FRYING":
                    FryingTimer.text = secondsLeft.ToString();
                    if (timer < 0f)
                    {
                        Debug.Log("finished cooking");
                        FryingTimer.text = "Pan";
                        i.Status = StationStatus.Ready;
                        alert(i, true);
                    }
                    break;
            }
            
            yield return null;
        }
    }

    private void alert(InteractionManager.InteractableObjects i, bool activate) //alerts player if the food is ready
    {
        Color color = Color.red;
        string alertText = "READY";
        if (activate == false)
        {
            color = new Color32(255, 255, 255, 116);
            alertText = i.Name;
        }
        switch (i.Hint)
        {
            case "RICE": 
                RiceTimer.text = alertText;
                RiceTimer.color = color;
                if (activate == false)
                {
                    RiceTimer.gameObject.SetActive(activate);
                }
                break;
            case "PLATE":
                PlateTimer.text = alertText;
                PlateTimer.color = color;
                if (activate == true)
                {
                    PlateTimer.gameObject.SetActive(activate);
                }
                break;
            case "STEAM":
                SteamTimer.text = alertText;
                SteamTimer.color = color;
                if (activate == true)
                {
                    SteamTimer.gameObject.SetActive(activate);
                }
                break;
            case "OVEN":
                OvenTimer.text = alertText;
                OvenTimer.color = color;
                if (activate == true)
                {
                    OvenTimer.gameObject.SetActive(activate);
                }
                break;
            case "FRYING":
                FryingTimer.text = alertText;
                FryingTimer.color = color;
                if (activate == true)
                {
                    FryingTimer.gameObject.SetActive(activate);
                }
                break;
        }
    }

    private static List<Vector2Int> GetAdjacentPositions(Vector2Int position) //gets the neighbouring tile positions
    {
        return new List<Vector2Int>
        {
            position + Vector2Int.up,
            position + Vector2Int.down,
            position + Vector2Int.left,
            position + Vector2Int.right,
        };
    }
}
