using UnityEngine;

public class Interactable : MonoBehaviour
{
    public string itemName = "Lantern";

    public virtual void Interact(PlayerItem playerHand)
    {
        playerHand.PickUpItem(this.gameObject);
    }
}
