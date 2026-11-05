using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class InteractableObj : MonoBehaviour
{
    [SerializeField] public string displayName;
    [SerializeField] public string hint;

    public Vector2Int prox;
    public Vector2Int objectLocation;

    public List<Vector2Int> table;
    

    protected virtual void Awake() //when game is opened, sets a fixed proximity of interactable stations
    {
        table.Clear();
        table = new List<Vector2Int>() {new Vector2Int(-12, -4), new Vector2Int(-12, -3), new Vector2Int(-12, -2), new Vector2Int(-12, -1)};

        Vector2 pos = transform.position;
        Debug.Log("transform position" + pos);
        objectLocation = GridUtils.WorldToGrid(pos);
        Debug.Log("object location" + objectLocation);

        if (table.Contains(objectLocation))
        {
            prox = new Vector2Int (-13, objectLocation.y);
        }
        else if (objectLocation == new Vector2Int(-13, 0))
        {
            prox = new Vector2Int(-13, -1);
        }
        else if(objectLocation == new Vector2Int(-12, 0))
        {
            prox = new Vector2Int(-13, -1);
        }
        else if (objectLocation == new Vector2Int(-11, -6))
        {
            prox = new Vector2Int(-11, -5);
        }
        else
        {
            prox = new Vector2Int(-12, -5);
        }
        Debug.Log(displayName + " " + objectLocation + " " + prox);
    }

}
