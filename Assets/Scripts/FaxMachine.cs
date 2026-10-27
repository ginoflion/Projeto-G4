using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using TMPro;
using SUPERCharacter;

public class FaxMachine : Interactable
{
    public string requiredItemName = "Answer";
    public float animationDuration = 2.5f;
    public TextMeshPro screenText;
    public string[] stationNames = { "Estação Norte", "Porto", "Central Elétrica", "Arquivo", "Polícia", "Câmara" };
    public int correctStationIndex = 4;
    public Camera playerCamera;
    public Transform faxCameraPosition;
    public SUPERCharacterAIO playerController;
    public InteractionSystem interactionSystem;
    public LayerMask buttonLayer; 

    private bool isLookingAtFax = false;
    private Vector3 originalCameraPosition;
    private Quaternion originalCameraRotation;

    private int currentIndex = 0;
    private GameObject insertedPaper;
    private PlayerItem currentPlayerHand;

    void Start()
    {
        itemName = "Inserir Resposta";
        UpdateScreen();
    }

    public override void Interact(PlayerItem playerHand)
    {
        if (!isLookingAtFax && playerHand.HasItem() && playerHand.GetItemName() == requiredItemName && insertedPaper == null)
        {
            insertedPaper = playerHand.DropItem();
            insertedPaper.SetActive(false); 
            currentPlayerHand = playerHand;
            
            EnterFaxView();
        }
        else if (playerHand.HasItem())
        {
            Debug.Log("A máquina só aceita a folha de resposta final.");
        }
    }

    private void EnterFaxView()
    {
        isLookingAtFax = true;
        itemName = "A configurar transmissão...";
        originalCameraPosition = playerCamera.transform.position;
        originalCameraRotation = playerCamera.transform.rotation;

        if (playerController != null) playerController.PausePlayer(PauseModes.FreezeInPlace);
        if (interactionSystem != null) interactionSystem.enabled = false;
        playerCamera.transform.position = faxCameraPosition.position;
        playerCamera.transform.rotation = faxCameraPosition.rotation;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void ExitFaxView()
    {
        isLookingAtFax = false;
        itemName = "Inserir Resposta";
        playerCamera.transform.position = originalCameraPosition;
        playerCamera.transform.rotation = originalCameraRotation;
        if (playerController != null) playerController.UnpausePlayer();
        if (interactionSystem != null) interactionSystem.enabled = true;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        if (!isLookingAtFax) return;
        if (Mouse.current.rightButton.wasPressedThisFrame || Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (insertedPaper != null && itemName != "A transmitir...")
            {
                ReturnPaper();
            }
            if (itemName != "A transmitir...") ExitFaxView();
        }

        if (Mouse.current.leftButton.wasPressedThisFrame && itemName != "A transmitir...")
        {
            Ray ray = playerCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
            if (Physics.Raycast(ray, out RaycastHit hit, 10f, buttonLayer))
            {
                FaxButton clickedButton = hit.collider.GetComponent<FaxButton>();
                if (clickedButton != null)
                {
                    clickedButton.Interact(currentPlayerHand);
                }
            }
        }
    }

    public void CycleStation(int direction)
    {
        currentIndex += direction;
        if (currentIndex < 0) currentIndex = stationNames.Length - 1;
        if (currentIndex >= stationNames.Length) currentIndex = 0;
        UpdateScreen();
    }

    private void UpdateScreen()
    {
        if (screenText != null) screenText.text = stationNames[currentIndex];
    }

    public void SendFax()
    {
        if (insertedPaper != null && itemName != "A transmitir...")
        {
            StartCoroutine(SendFaxRoutine());
        }
    }

    private IEnumerator SendFaxRoutine()
    {
        Debug.Log("A transmitir para: " + stationNames[currentIndex]);
        itemName = "A transmitir...";
        yield return new WaitForSeconds(animationDuration);

        if (currentIndex == correctStationIndex)
            Debug.Log("Sucesso! Transmissão enviada para a estação correta.");
        else
            Debug.Log("Erro! A transmissão foi para a estação errada.");

        ReturnPaper();
        ExitFaxView();
    }

    private void ReturnPaper()
    {
        insertedPaper.SetActive(true);
        if (currentPlayerHand != null && !currentPlayerHand.HasItem())
        {
            currentPlayerHand.PickUpItem(insertedPaper);
        }
        else
        {
            insertedPaper.transform.position = transform.position + Vector3.up * 0.5f;
        }
        insertedPaper = null;
    }
}