using UnityEngine;

public class CorkboardSpot : Interactable
{
    public GameObject outlineVisual;
    private GameObject placedPaper;
    private PlayerItem playerHand;

    void Start()
    {
        if (outlineVisual != null) outlineVisual.SetActive(false);
        playerHand = FindObjectOfType<PlayerItem>();
    }
    
    public override void OnHoverEnter()
    {
        if (placedPaper == null && outlineVisual != null && playerHand != null && (playerHand.GetItemName() == "Paper" || playerHand.GetItemName() == "Answer"))
        {
            outlineVisual.SetActive(true);
        }
    }

    public override void OnHoverExit()
    {
        if (outlineVisual != null ) outlineVisual.SetActive(false);
    }

    public override void Interact(PlayerItem playerHand)
    {
        if (playerHand.HasItem() && (playerHand.GetItemName() == "Paper" || playerHand.GetItemName() == "Answer") && placedPaper == null)
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
