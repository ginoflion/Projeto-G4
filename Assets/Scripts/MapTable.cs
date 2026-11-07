using SUPERCharacter;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class MapTable : Interactable
{
    [Header("Câmara e Jogador")]
    public Camera playerCamera;
    public SUPERCharacterAIO playerController;
    public Transform mapCameraPosition;
    public MapDrawer mapDrawer;
    private bool isLookingAtMap = false;
    private Vector3 originalCameraPosition;
    private Quaternion originalCameraRotation;
    public InteractionSystem playerInteractionSystem;
    private CrosshairController crosshairController;
    public TextMeshProUGUI textDisplay;

    private void Start()
    {
        crosshairController = FindObjectsByType<CrosshairController>(FindObjectsSortMode.None)[0];
    }

    public override void Interact(PlayerItem playerHand)
    {
        if (!isLookingAtMap && !playerHand.HasItem()) 
        {
            EnterMapView();
        }
    }

    private void EnterMapView()
    {
        textDisplay.text = "1 - Penci\n2 - Eraser";
        crosshairController.SetCrosshairVisibility(false);
        isLookingAtMap = true;
        if (playerInteractionSystem != null) playerInteractionSystem.enabled = false;
        originalCameraPosition = playerCamera.transform.position;
        originalCameraRotation = playerCamera.transform.rotation;
        if (playerController != null) 
        {
            playerController.PausePlayer(PauseModes.FreezeInPlace);
        }
        playerCamera.transform.position = mapCameraPosition.position;
        playerCamera.transform.rotation = mapCameraPosition.rotation;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        mapDrawer.isDrawingModeActive = true;
    }

    void Update()
    {
        if (isLookingAtMap && Keyboard.current.eKey.wasPressedThisFrame)
        {
            ExitMapView();
        }
    }

    private void ExitMapView()
    {
        textDisplay.text = "Press E to Interact";
        isLookingAtMap = false;
        crosshairController.SetCrosshairVisibility(true);
        playerCamera.transform.position = originalCameraPosition;
        playerCamera.transform.rotation = originalCameraRotation;
        if (playerController != null) 
        {
            playerController.UnpausePlayer();
        }
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        mapDrawer.isDrawingModeActive = false;
        if (playerInteractionSystem != null) playerInteractionSystem.enabled = true;
    }
}