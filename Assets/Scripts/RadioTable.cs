using SUPERCharacter;
using UnityEngine;
using UnityEngine.InputSystem;

public class RadioTable : Interactable
{
    [Header("Câmara e Jogador")]
    public Camera playerCamera;
    public SUPERCharacterAIO playerController;
    public Transform radioCameraPosition; 

    public static bool isLookingAtRadio = false;

    private Vector3 originalCameraPosition;
    private Quaternion originalCameraRotation;

    public override void Interact(PlayerItem playerHand)
    {
        if (!isLookingAtRadio && !playerHand.HasItem())
        {
            EnterRadioView();
        }
    }

    private void EnterRadioView()
    {
        isLookingAtRadio = true;

        originalCameraPosition = playerCamera.transform.position;
        originalCameraRotation = playerCamera.transform.rotation;

        if (playerController != null)
        {
            playerController.PausePlayer(PauseModes.FreezeInPlace);
        }

        playerCamera.transform.position = radioCameraPosition.position;
        playerCamera.transform.rotation = radioCameraPosition.rotation;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void Update()
    {
        if (isLookingAtRadio && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            ExitRadioView();
        }
    }

    private void ExitRadioView()
    {
        isLookingAtRadio = false;

        playerCamera.transform.position = originalCameraPosition;
        playerCamera.transform.rotation = originalCameraRotation;

        if (playerController != null)
        {
            playerController.UnpausePlayer();
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}