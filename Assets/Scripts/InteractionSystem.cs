using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class InteractionSystem : MonoBehaviour
{
    public Transform playerCamera;
    public float interactionDistance = 3f;
    public LayerMask interactableLayer;

    private PlayerItem playerItem;
    public InputActionReference interactAction;
    private Interactable currentHoveredObject;
    public GameObject helper;

    void Awake()
    {
        playerItem = GetComponent<PlayerItem>();
    }

    void OnEnable()
    {
        interactAction.action.Enable();
    }
    
    void OnDisable()
    {
        interactAction.action.Disable();
    }

    void Update()
    {
        HandleHover();
        if (interactAction.action.WasPressedThisFrame())
        {
            if (currentHoveredObject != null)
            {
                currentHoveredObject.Interact(playerItem);
            }
        }
    }
    
    public void UseHoveredObject()
    {
        if (currentHoveredObject != null && currentHoveredObject is IUsable usable && currentHoveredObject.itemName == "Door")
        {
            usable.Use();
        }
    }

    private void HandleHover()
    {
        Ray ray = new Ray(playerCamera.position, playerCamera.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactionDistance, interactableLayer))
        {
            Interactable interactable = hit.collider.GetComponent<Interactable>();

            if (interactable != currentHoveredObject)
            {
                if (currentHoveredObject != null) currentHoveredObject.OnHoverExit();
                currentHoveredObject = interactable;
                if (currentHoveredObject != null) currentHoveredObject.OnHoverEnter();
            }

            if (currentHoveredObject != null)
            {
                bool shouldShowHelper = false;
                FaxMachine fax = currentHoveredObject.GetComponent<FaxMachine>();
                UVLanternSpot lanternSpot = currentHoveredObject.GetComponent<UVLanternSpot>();
                CorkboardSpot corkboardSpot = currentHoveredObject.GetComponent<CorkboardSpot>();
                Door door = currentHoveredObject.GetComponent<Door>();
                
                if (door != null)
                {
                    if (playerItem != null && !playerItem.HasItem())
                    {
                        helper.GetComponent<TextMeshProUGUI>().text = "Mouse 1 to lock\nPress E to Interact";
                        shouldShowHelper = true;
                    }
                }
                if (fax != null)
                {
                    if (playerItem != null && playerItem.HasItem() && playerItem.GetItemName() == "Answer")
                    {
                        shouldShowHelper = true;
                    }
                }
                if (lanternSpot != null)
                {
                    if (playerItem != null && playerItem.HasItem() && playerItem.GetItemName() == "UVLantern")
                    {
                        shouldShowHelper = true;
                    }
                }
                if (corkboardSpot != null)
                {
                    if (playerItem != null && playerItem.HasItem() && (playerItem.GetItemName() == "Paper" || playerItem.GetItemName() == "Answer" || playerItem.GetItemName() == "Instructions"|| playerItem.GetItemName() == "Instruction"))
                    {
                        shouldShowHelper = true;
                    }
                    else if (corkboardSpot.placedPaper != null)
                    {
                        shouldShowHelper = true;
                    }
                }
                else
                {
                    if (playerItem != null && !playerItem.HasItem())
                    {
                        if (currentHoveredObject == fax) shouldShowHelper = false;
                        else
                        {
                            shouldShowHelper = true;
                        }
                       
                    }
                }

                if (helper != null) helper.SetActive(shouldShowHelper);
            }
        }
        else
        {
            if (currentHoveredObject != null)
            {
                if (helper != null) helper.SetActive(false);
                helper.GetComponent<TextMeshProUGUI>().text = "Press E to Interact";
                currentHoveredObject.OnHoverExit();
                currentHoveredObject = null;
            }
        }
    }
}
