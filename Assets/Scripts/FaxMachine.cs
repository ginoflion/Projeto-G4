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
        DayData currentDay = AnswerManager.Instance.GetCurrentDayData();
        bool isStationCorrect = false;
        bool areAnswersCorrect = true;

        if (currentDay != null)
        {
            isStationCorrect = (currentIndex == currentDay.correctFaxStationIndex);
            if (!isStationCorrect)
            {
                Debug.Log($"[Erro no Fax] Destino incorreto! Mandaste para '{stationNames[currentIndex]}', mas o destino correto era outro.");
            }

            PhysicalPaperInput paperInput = insertedPaper.GetComponent<PhysicalPaperInput>();
            if (paperInput != null)
            {
                if (paperInput.simbolos.Count != currentDay.paperAnswers.Count)
                {
                    areAnswersCorrect = false;
                    Debug.Log($"[Erro no Fax] O número de palavras escritas não coincide com o puzzle (Escritas: {paperInput.simbolos.Count}, Esperadas: {currentDay.paperAnswers.Count}).");
                }
                else
                {
                    for (int i = 0; i < currentDay.paperAnswers.Count; i++)
                    {
                        string playerInputWritten = paperInput.simbolos[i].currentInput.Trim().ToLower();
                        string expectedAnswer = currentDay.paperAnswers[i].Trim().ToLower();

                        if (playerInputWritten != expectedAnswer)
                        {
                            areAnswersCorrect = false;
                            Debug.Log($"[Erro no Fax] Símbolo #{i + 1} incorreto. Escreveste '{playerInputWritten}', mas a resposta esperada era '{expectedAnswer}'.");
                        }
                    }
                }
            }
            else
            {
                areAnswersCorrect = false;
                Debug.Log("[Erro no Fax] O papel inserido não tem o componente PhysicalPaperInput anexado.");
            }
        }

        bool daySuccess = isStationCorrect && areAnswersCorrect;
        AnswerManager.Instance.SetDayResult(daySuccess);

        if (daySuccess)
        {
            CartasManager.Instance.DefinirEscolha("certa");
            Debug.Log("Sucesso! Respostas corretas e transmissão enviada para a estação certa.");
        }
        else
        {
            CartasManager.Instance.DefinirEscolha("errada");
            Debug.Log("Falha na transmissão. Consulta os logs acima para ver o que estava incorreto.");
        }

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