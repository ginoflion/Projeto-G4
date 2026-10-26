using UnityEngine;

public class PlacementZone : Interactable
{
    public Transform placementPoint;
    public GameObject startingItem;
    private GameObject placedItem;
    
    void Start()
    {
        if (startingItem != null)
        {
            placedItem = startingItem;
            
            placedItem.transform.position = placementPoint.position;
            placedItem.transform.rotation = placementPoint.rotation;

            Rigidbody rb = placedItem.GetComponent<Rigidbody>();
            if (rb != null) rb.isKinematic = true;
            
            Collider col = placedItem.GetComponent<Collider>();
            if (col != null) col.enabled = true;
        }
    }

    public override void Interact(PlayerItem playerHand)
    {
        if (playerHand.HasItem() && placedItem == null)
        {
            placedItem = playerHand.DropItem();
            
            placedItem.transform.position = placementPoint.position;
            placedItem.transform.rotation = placementPoint.rotation;

            Rigidbody rb = placedItem.GetComponent<Rigidbody>();
            if (rb != null) rb.isKinematic = true;
        }
        else if (!playerHand.HasItem() && placedItem != null)
        {
            playerHand.PickUpItem(placedItem);
            placedItem = null;
        }
    }
}
