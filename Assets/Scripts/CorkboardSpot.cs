using UnityEngine;

public class CorkboardSpot : Interactable
{
    public GameObject outlineVisual;
    private GameObject placedPaper;

    void Start()
    {
        if (outlineVisual != null) outlineVisual.SetActive(false);
    }
    
    public override void OnHoverEnter()
    {
        if (placedPaper == null && outlineVisual != null)
        {
            outlineVisual.SetActive(true);
        }
    }

    public override void OnHoverExit()
    {
        if (outlineVisual != null) outlineVisual.SetActive(false);
    }

    public override void Interact(PlayerItem playerHand)
    {
        if (playerHand.HasItem() && playerHand.GetItemName() == "Paper" && placedPaper == null)
        {
            placedPaper = playerHand.DropItem(); 
            
            placedPaper.transform.position = transform.position;
            placedPaper.transform.rotation = transform.rotation;

            Rigidbody rb = placedPaper.GetComponent<Rigidbody>();
            if (rb != null) rb.isKinematic = true;
            if (outlineVisual != null) outlineVisual.SetActive(false);
        }
        else if (!playerHand.HasItem() && placedPaper != null)
        {
            playerHand.PickUpItem(placedPaper); 
            placedPaper = null;
            if (outlineVisual != null) outlineVisual.SetActive(true);
        }
    }
}
