using UnityEngine;

public class UVLanternSpot : Interactable
{
    public GameObject outlineVisual;
    private GameObject placedPaper;
    private PlayerItem playerHand;

    void Start()
    {
        if (outlineVisual != null) outlineVisual.SetActive(false);
        playerHand = FindObjectsByType<PlayerItem>(FindObjectsSortMode.None)[0];
    }

    void Update()
    {
        if (placedPaper != null && placedPaper.transform.parent != null)
        {
            placedPaper = null;
        }
    }
    
    public override void OnHoverEnter()
    {
        if (placedPaper == null && outlineVisual != null && playerHand != null && (playerHand.GetItemName() == "UVLantern"))
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
        if (playerHand.HasItem() && (playerHand.GetItemName() == "UVLantern") && placedPaper == null)
        {
            placedPaper = playerHand.DropItem();
            
            UVLantern Uv = placedPaper.GetComponent<UVLantern>();
            if (Uv != null && Uv.isLightOn) Uv.Use();
            
            placedPaper.transform.position = transform.position;
            placedPaper.transform.rotation = transform.rotation;

            Rigidbody rb = placedPaper.GetComponent<Rigidbody>();
            if (rb != null) rb.isKinematic = true;

            Collider col = placedPaper.GetComponent<Collider>();
            if (col != null) col.enabled = false;

            if (outlineVisual != null) outlineVisual.SetActive(false);
        }
        else if (!playerHand.HasItem() && placedPaper != null)
        {
            GameObject itemToPick = placedPaper;
            placedPaper = null;
            
            playerHand.PickUpItem(itemToPick); 
            
            if (outlineVisual != null) outlineVisual.SetActive(true);
        }
    }
}