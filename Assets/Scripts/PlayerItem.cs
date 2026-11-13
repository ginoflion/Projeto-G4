using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerItem : MonoBehaviour
{
    public Transform handTransform;
    private GameObject currentItem;
    
    public InputActionReference useAction;
    public GameObject lanternSpot;
    private InteractionSystem interactionSystem;
    
    void OnEnable()
    {
        if (useAction != null) useAction.action.Enable();
    }

    void OnDisable()
    {
        if (useAction != null) useAction.action.Disable();
    }

    void Awake()
    {
        interactionSystem = GetComponent<InteractionSystem>();
    }

    void Update()
    {
        if (useAction != null && useAction.action.WasPressedThisFrame())
        {
            if (HasItem())
            {
                UseCurrentItem();
            }
            else if (interactionSystem != null && interactionSystem.enabled)
            {
                interactionSystem.UseHoveredObject();
            }
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
    
        Rigidbody rb = currentItem.GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = true;
    
        Collider col  = currentItem.GetComponent<Collider>();
        if (col != null) col.enabled = false;
    
        currentItem.transform.SetParent(handTransform);
    
        currentItem.transform.localPosition = Vector3.zero;
        currentItem.transform.localRotation = Quaternion.identity;

        if (currentItem != null && GetItemName() == "Paper")
        {
            ReadablePaper paperScript = currentItem.GetComponent<ReadablePaper>();
        
            currentItem.transform.localPosition = paperScript.normalPosition;
            currentItem.transform.localRotation = Quaternion.Euler(paperScript.normalRotation);
        
            Transform jornalChild = currentItem.transform.Find("jornal");
            Transform getrotadedChild = currentItem.transform.Find("getrotaded");
            if (jornalChild != null) jornalChild.gameObject.SetActive(false);
            if (getrotadedChild != null) getrotadedChild.gameObject.SetActive(true);
        }
        if (currentItem != null && GetItemName() == "Instruction")
        {
            NewPaper paperScript = currentItem.GetComponent<NewPaper>();
        
            currentItem.transform.localPosition = paperScript.normalPosition;
            currentItem.transform.localRotation = Quaternion.Euler(paperScript.normalRotation);
        }
        
        if (currentItem != null && GetItemName() == "Answer")
        {
            PhysicalPaperInput paperScript = currentItem.GetComponent<PhysicalPaperInput>();
            paperScript.SetIgnoreCRTLayer();
            currentItem.transform.localPosition = paperScript.normalPosition;
            currentItem.transform.localRotation = Quaternion.Euler(paperScript.normalRotation);
        }
        
        if (currentItem != null && GetItemName() == "Instructions")
        {
            InstructionsPaper paperScript = currentItem.GetComponent<InstructionsPaper>();
        
            currentItem.transform.localPosition = paperScript.normalPosition;
            currentItem.transform.localRotation = Quaternion.Euler(paperScript.normalRotation);
        }
    
        if (currentItem != null && GetItemName() == "UVLantern")
        {
            lanternSpot.layer = LayerMask.NameToLayer("Interactable");
        }
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
    
    public GameObject GetCurrentItem()
    {
        return currentItem;
    }
}
