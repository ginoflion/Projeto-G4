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
                helper.SetActive(true);
                if (currentHoveredObject != null) currentHoveredObject.OnHoverEnter();
            }
        }
        else
        {
            if (currentHoveredObject != null)
            {
                helper.SetActive(false);
                currentHoveredObject.OnHoverExit();
                currentHoveredObject = null;
            }
        }
    }
}
