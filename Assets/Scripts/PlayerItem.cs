using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerItem : MonoBehaviour
{
    public Transform handTransform;
    private GameObject currentItem;
    
    public InputActionReference useAction;
    public GameObject lanternSpot;
    
    void OnEnable()
    {
        if (useAction != null) useAction.action.Enable();
    }

    void OnDisable()
    {
        if (useAction != null) useAction.action.Disable();
    }

    void Update()
    {
        if (useAction != null && useAction.action.WasPressedThisFrame())
        {
            UseCurrentItem();
        }
    }
    private void UseCurrentItem()
    {
        if (HasItem())
        {
            Debug.Log(currentItem);
            IUsable usableItem = currentItem.GetComponent<IUsable>();
            
            if (usableItem != null)
            {
                usableItem.Use();
            }
        }
    }

    public bool HasItem()
    {
        return currentItem != null;
    }

    public void PickUpItem(GameObject item)
    {
        if (HasItem()) return;
        currentItem = item;

        if (currentItem != null && GetItemName() == "Paper")
        {
            Transform jornalChild = currentItem.transform.Find("jornal");
            Transform getrotadedChild = currentItem.transform.Find("getrotaded");
            if (jornalChild != null) jornalChild.gameObject.SetActive(false);
            if (getrotadedChild != null) getrotadedChild.gameObject.SetActive(true);
        }
        
        if (currentItem != null && GetItemName() == "UVLantern")
        {
            lanternSpot.layer = LayerMask.NameToLayer("Interactable");
        }
        
        Rigidbody rb = currentItem.GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = true;
        
        Collider col  = currentItem.GetComponent<Collider>();
        if (col != null) col.enabled = false;
        
        currentItem.transform.SetParent(handTransform);
        currentItem.transform.localPosition = Vector3.zero;
        currentItem.transform.localRotation = Quaternion.identity;
    }

    public GameObject DropItem()
    {
        if (!HasItem()) return null;

        GameObject itemToDrop = currentItem;
        currentItem.transform.SetParent(null);

        Collider col = itemToDrop.GetComponent<Collider>();
        if (col != null) col.enabled = true;

        currentItem = null;
        return itemToDrop;
    }

    public string GetItemName()
    {
        if (currentItem != null)
        { 
            Interactable interactable = currentItem.GetComponent<Interactable>();
            return interactable.itemName != null ? interactable.itemName : "";
        }
        else
        {
            return "";
        }
    }
}
