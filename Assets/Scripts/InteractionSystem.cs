using UnityEngine;
using UnityEngine.InputSystem;

public class InteractionSystem : MonoBehaviour
{
    public Transform playerCamera;
    public float interactionDistance = 3f;
    public LayerMask interactableLayer;

    private PlayerItem playerItem;
    public InputActionReference interactAction;

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
        if (interactAction.action.WasPressedThisFrame())
        {
            TryInteract();
        }
    }

    private void TryInteract()
    {
        Ray ray = new Ray(playerCamera.position, playerCamera.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactionDistance, interactableLayer))
        {
            Interactable interactable = hit.collider.GetComponent<Interactable>();
            if (interactable != null)
            {
                interactable.Interact(playerItem);
            }
        }
    }
}
