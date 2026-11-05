using UnityEngine;

public class StorageInteraction : InteractableObj
{
    protected override void Awake() //inhertits the interactable object class but changes sepcific prox for storage
    {
        base.Awake();
        prox = new Vector2Int(objectLocation.x, objectLocation.y + 1);
        Debug.Log(displayName + " " + objectLocation + " " + prox);
    }
}
