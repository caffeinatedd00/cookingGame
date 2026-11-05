using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class InteractionManager : MonoBehaviour
{
    [SerializeField] private GameObject Objects;
    public List<InteractableObjects> CanInteract = new List<InteractableObjects>();


    void Start()
    {
        StoreData(Objects, CanInteract);
    }

    public class InteractableObjects
    {
        public string Name { get; set; }
        public Vector2 Prox{ get; set; }
        public string Hint {  get; set; }
        public Vector2 Location {  get; set; }
        public StationStatus Status { get; set; }
        public string CurrentRecipie { get; set; }

        public InteractableObjects(string name, Vector2 prox, string hint, Vector2 location, StationStatus status, string currentRecipie)
        {
            Name = name;
            Prox = prox;
            Hint = hint;
            Location = location;
            Status = status;
            CurrentRecipie = currentRecipie;
        }
    }
    
    private void StoreData(GameObject obj, List<InteractableObjects> caninteract) //makes a list of interactable stations
    {
        caninteract.Clear();
        if (Objects.transform.childCount > 0)
        {
            foreach (Transform child in obj.transform)
            {
                var data = child.GetComponent<InteractableObj>();

                var entry = new InteractableObjects(data.displayName, data.prox, data.hint, data.objectLocation, StationStatus.Idle, null);

                caninteract.Add(entry);
                Debug.Log("Added " + entry.Name);
            }
        }
    }
}
