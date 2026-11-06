using UnityEngine;

public class CorkboardSpot : Interactable
{
    public GameObject outlineVisual;
    private GameObject placedPaper;
    private PlayerItem playerHand;

    void Start()
    {
        if (outlineVisual != null) outlineVisual.SetActive(false);
        playerHand = FindObjectsByType<PlayerItem>(FindObjectsSortMode.None)[0];
    }
    
    public override void OnHoverEnter()
    {
        if (placedPaper == null && outlineVisual != null && playerHand != null && (playerHand.GetItemName() == "Paper" || playerHand.GetItemName() == "Answer" || playerHand.GetItemName() == "Instructions"))
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
            placedPaper.transform.rotation = transform.rotation * Quaternion.Euler(90f, 0f, -90f);
            BoxCollider box = placedPaper.GetComponent<BoxCollider>();
            box.enabled = false;

            Rigidbody rb = placedPaper.GetComponent<Rigidbody>();
            if (rb != null) rb.isKinematic = true;
            if (outlineVisual != null) outlineVisual.SetActive(false);
        }
        else if (!playerHand.HasItem() && placedPaper != null)
        {
            BoxCollider box = placedPaper.GetComponent<BoxCollider>();
            box.enabled = true;
            playerHand.PickUpItem(placedPaper); 
            placedPaper = null;
            if (outlineVisual != null) outlineVisual.SetActive(true);
        }
    }
}
