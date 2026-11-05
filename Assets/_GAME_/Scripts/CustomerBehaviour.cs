using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using Unity.VisualScripting;
using TMPro;

public enum CustomerStatus
{
    Entering,
    Leaving
}

public class CustomerBehaviour : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private Vector2 gridSize = new Vector2(1f, 1f);
    [SerializeField] private TMP_Text orderedText;

    private float serviceWaitTime = 80f;
    private float waitTime = 40f;
    private float waitTime2 = 20f;
    private float eatTime = 20f;

    private ObstacleTilemap obstacleTilemap;
    private SeatingTilemap seatingTilemap;
    private SeatingTilemap.Seat chosenSeat;
    private DatabaseManager databaseManager;
    private Transform spawnPoint;
    private TMP_Text moneyText;

    private Vector2 targetPosition;
    private bool isMoving = false;
    private bool paid = false;

    private CustomerStatus customerStatus;
    public bool ordered;
    public bool foodServed;
    public DatabaseManager.Recipe orderedItem;
    private Coroutine moveRoutine;

    public void Initialize(ObstacleTilemap obstacleMap, SeatingTilemap seatingMap, Transform spawn, DatabaseManager database, TMP_Text money) //sets the important data for the customer
    {
        obstacleTilemap = obstacleMap;
        seatingTilemap = seatingMap;
        spawnPoint = spawn;
        customerStatus = CustomerStatus.Entering;
        databaseManager = database;
        moneyText = money;
    }

    void Update()
    {
        HandleMovementInput();
        waitTimer(); 
    }
    public void HandleMovementInput() //see if customer is moving or seated if not selects a free seat and path finds to free seat
    {
        if (TimeManager.Instance.currentPhase == Phase.Closed && customerStatus != CustomerStatus.Leaving)
        {
            StartLeaving();
            return;
        }
        if (customerStatus != CustomerStatus.Entering)
        {
            return;
        }
        if (isMoving || chosenSeat != null)
        {
            return;
        }
        if (seatingTilemap == null || seatingTilemap.seats.Count == 0)
        {
            return;
        }

        List<SeatingTilemap.Seat> freeSeats = seatingTilemap.seats.FindAll(s => !s.isOccupied);

        if (freeSeats.Count == 0)
        {
            Debug.Log("freeSeats.Count == 0");
            gameObject.GetComponent<SpriteRenderer>().color = Color.red;
            Destroy(gameObject, 2f);
            return;
        }

        chosenSeat = freeSeats[Random.Range(0, freeSeats.Count)]; //sets the chosen seat to occupied so that multiple cannot sit at once
        chosenSeat.isOccupied = true;
        targetPosition = GridUtils.GridToWorld(chosenSeat.approachTile);
        FindPathToTargetPos();
    }

    private void FindPathToTargetPos() //uses A star to find path to the seat
    {
        Vector2 startPosition = transform.position;
        List<Vector2> path = AStar.FindPath(startPosition, targetPosition, gridSize, obstacleTilemap.isTileObstacle); //Main path finder
        if (path != null && path.Count > 0)
        {
            if (moveRoutine != null)
            {
                StopCoroutine(moveRoutine);
            }
            moveRoutine = StartCoroutine(MoveAlongPath(path));
        }
    }
    private IEnumerator MoveAlongPath(List<Vector2> path) //moves the customer along the found path
    {
        isMoving = true;
        int currentWayPointIndex = 0;
        while (currentWayPointIndex < path.Count)
        {
            targetPosition = path[currentWayPointIndex] + gridSize / 2;
            while ((Vector2)transform.position != targetPosition)
            {
                float step = moveSpeed * Time.fixedDeltaTime;
                transform.position = Vector3.MoveTowards(transform.position, targetPosition, step);

                yield return new WaitForFixedUpdate();
            }

            currentWayPointIndex++;
        }
        if (customerStatus == CustomerStatus.Entering) //when they arrive by the seat, moves the customer onto the seat to sit down
        {
            Vector2 sitWorldPos = GridUtils.GridToWorld((Vector2Int)chosenSeat.seatTile) + gridSize / 2f;
            transform.position = sitWorldPos;
            chosenSeat.currentCustomer = this;
            isMoving = false;
        }
        else //they would be by the exit door if they are not going to sit so allows them to exit
        {
            isMoving = false;
            Destroy(gameObject, 1f);
        }
        moveRoutine = null;
    }

    private void OnDestroy()
    {
        if (chosenSeat != null)
        {
            chosenSeat.isOccupied = false;
            chosenSeat.currentCustomer = null;
        }
    }

    private void waitTimer() //patience timer for customers
    {
        if (customerStatus != CustomerStatus.Entering)
        {
            return;
        }
        
        if (!ordered && !foodServed) //if their ordered doesnt get taken in set time, they get angry and leave
        {
            serviceWaitTime -= Time.deltaTime;
            if (serviceWaitTime <= 0)
            {
                gameObject.GetComponent<SpriteRenderer>().color = Color.red;
                StartLeaving();
                ResetTimers();
            }
            return;
        }


        if (ordered && !foodServed) //if their order is taken and have not get served, slightly get mad and if its taking longer, get angry and leave
        {
            orderedText.text = "Ordered: " + orderedItem.name; 
            if (waitTime > 0f)
            {
                waitTime -= Time.deltaTime;
            }
            else if (waitTime2 > 0f)
            {
                gameObject.GetComponent<SpriteRenderer>().color = Color.orange;
                waitTime2 -= Time.deltaTime;
            }
            else
            {
                Debug.Log("Customer ran out of patience");
                gameObject.GetComponent<SpriteRenderer>().color = Color.red;
                StartLeaving();
                ResetTimers();
            }
            return;
        }

        gameObject.GetComponent<SpriteRenderer>().color = Color.green; //if food gets served they turn green to show they are satisfied

        if (!paid) //adds the amount of money to wallet
        {
            databaseManager.AddMoney(orderedItem.price);
            moneyText.text = $"{databaseManager.MoneyAmount()}";
            TimeManager.Instance.profits += orderedItem.price;
            TimeManager.Instance.sales += 1;
            orderedText.text = " ";
            paid = true;
        }
        eatTime -= Time.deltaTime; 
        if (eatTime <= 0f) //leaves after eating
        {
            Debug.Log("Customer finished eating");
            StartLeaving();
            ResetTimers();
        }
    }
    public void ResetTimers() //sets thier timers back to full
    {
        serviceWaitTime = 80f;
        waitTime = 40f;  
        waitTime2 = 20f;
        eatTime = 20f;
    }
    private void StartLeaving() //leave by setting path find target to the exit door
    {
        if (customerStatus == CustomerStatus.Leaving)
        {
            return;
        }

        customerStatus = CustomerStatus.Leaving;

        if (chosenSeat != null)
        {
            chosenSeat.isOccupied = false;
            chosenSeat.currentCustomer = null;
            chosenSeat = null;
        }

        targetPosition = spawnPoint.position;
        FindPathToTargetPos();
    }
}
