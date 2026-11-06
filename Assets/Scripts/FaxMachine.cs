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

    [Header("Trajeto do Papel na Fax (Pontos)")]
    [Tooltip("Coloca aqui os pontos em ordem: [0] Ponto Inicial, [1] Intermédio, [2] Ponto Final.")]
    [SerializeField] private Transform[] pontosDeTrajeto;
    [SerializeField] private float tempoViagemPapel = 0.8f;

    [Header("Áudio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip somInserirPapel;
    [SerializeField] private AudioClip somRetirarPapel;

    private bool isLookingAtFax = false;
    private Vector3 originalCameraPosition;
    private Quaternion originalCameraRotation;

    private int currentIndex = 0;
    private GameObject insertedPaper;
    private PlayerItem currentPlayerHand;
    private CrosshairController _crosshairController;

    void Start()
    {
        itemName = "Inserir Resposta";
        _crosshairController = FindObjectsByType<CrosshairController>(FindObjectsSortMode.None)[0];
        UpdateScreen();
    }

    public override void Interact(PlayerItem playerHand)
    {
        if (!isLookingAtFax && playerHand.HasItem() && playerHand.GetItemName() == requiredItemName && insertedPaper == null)
        {
            insertedPaper = playerHand.DropItem();
            insertedPaper.SetActive(true);
            currentPlayerHand = playerHand;

            if (audioSource != null && somInserirPapel != null)
            {
                audioSource.PlayOneShot(somInserirPapel);
            }

            StartCoroutine(InserirEAnimarPapelRoutine());
        }
    }

    private IEnumerator InserirEAnimarPapelRoutine()
    {
        if (pontosDeTrajeto == null || pontosDeTrajeto.Length < 2)
        {
            Debug.LogError("Precisas de configurar pelo menos 2 pontos no array 'Pontos De Trajeto' da FaxMachine!");
            EnterFaxView();
            yield break;
        }

        Transform pontoInicial = pontosDeTrajeto[0];
        insertedPaper.transform.position = pontoInicial.position;
        insertedPaper.transform.rotation = pontoInicial.rotation;
        insertedPaper.transform.SetParent(transform);

        int qtd = pontosDeTrajeto.Length;
        Vector3[] localPos = new Vector3[qtd];
        Quaternion[] localRot = new Quaternion[qtd];

        for (int i = 0; i < qtd; i++)
        {
            localPos[i] = transform.InverseTransformPoint(pontosDeTrajeto[i].position);
            localRot[i] = Quaternion.Inverse(transform.rotation) * pontosDeTrajeto[i].rotation;
        }

        yield return StartCoroutine(AnimarTrajetoPapel(insertedPaper.transform, localPos, localRot, false, tempoViagemPapel));

        EnterFaxView();
    }

    private void EnterFaxView()
    {
        isLookingAtFax = true;
        itemName = "A configurar transmissão...";
        _crosshairController.SetCrosshairVisibility(false);
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
        _crosshairController.SetCrosshairVisibility(true);
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
        if (Mouse.current.rightButton.wasPressedThisFrame || Keyboard.current.eKey.wasPressedThisFrame)
        {
            if (insertedPaper != null && itemName != "A transmitir...")
            {
                ExitFaxView();
                StartCoroutine(RetirarEVoltarMaoRoutine());
                return;
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
            PhysicalPaperInput paperInput = insertedPaper.GetComponent<PhysicalPaperInput>();

            if (paperInput != null)
            {
                if (paperInput.simbolos.Count != currentDay.paperAnswers.Count)
                {
                    areAnswersCorrect = false;
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
                        }
                    }
                }
            }
            else
            {
                areAnswersCorrect = false;
            }
        }

        bool daySuccess = isStationCorrect && areAnswersCorrect;
        AnswerManager.Instance.SetDayResult(daySuccess);

        if (daySuccess)
        {
            CartasManager.Instance.DefinirEscolha("certa");
            Debug.Log("Sucesso! Respostas corretas e transmissão enviada.");
        }
        else
        {
            CartasManager.Instance.DefinirEscolha("errada");
            Debug.Log("Falha na transmissão.");
        }
        Door.CartaSent = true;

        ExitFaxView();
        StartCoroutine(RetirarEVoltarMaoRoutine());
    }

    private IEnumerator RetirarEVoltarMaoRoutine()
    {
        if (insertedPaper != null && pontosDeTrajeto != null && pontosDeTrajeto.Length >= 2)
        {
            if (audioSource != null && somRetirarPapel != null)
            {
                audioSource.PlayOneShot(somRetirarPapel);
            }

            int qtd = pontosDeTrajeto.Length;
            Vector3[] localPos = new Vector3[qtd];
            Quaternion[] localRot = new Quaternion[qtd];

            for (int i = 0; i < qtd; i++)
            {
                localPos[i] = transform.InverseTransformPoint(pontosDeTrajeto[i].position);
                localRot[i] = Quaternion.Inverse(transform.rotation) * pontosDeTrajeto[i].rotation;
            }

            yield return StartCoroutine(AnimarTrajetoPapel(insertedPaper.transform, localPos, localRot, true, tempoViagemPapel));
        }

        ReturnPaper();
    }

    private IEnumerator AnimarTrajetoPapel(Transform alvo, Vector3[] posicoes, Quaternion[] rotacoes, bool reverso, float duracao)
    {
        float tempoDecorrido = 0f;
        int totalSegmentos = posicoes.Length - 1;

        while (tempoDecorrido < duracao)
        {
            if (alvo == null) yield break;

            tempoDecorrido += Time.deltaTime;
            float progressoGlobal = Mathf.Clamp01(tempoDecorrido / duracao);
            progressoGlobal = Mathf.Sin(progressoGlobal * Mathf.PI * 0.5f);

            if (reverso)
            {
                progressoGlobal = 1f - progressoGlobal;
            }

            float progressoEscalado = progressoGlobal * totalSegmentos;
            int indiceSegmento = Mathf.FloorToInt(progressoEscalado);

            if (indiceSegmento >= totalSegmentos) indiceSegmento = totalSegmentos - 1;
            if (indiceSegmento < 0) indiceSegmento = 0;

            float progressoSegmento = progressoEscalado - indiceSegmento;

            alvo.localPosition = Vector3.Lerp(posicoes[indiceSegmento], posicoes[indiceSegmento + 1], progressoSegmento);
            alvo.localRotation = Quaternion.Lerp(rotacoes[indiceSegmento], rotacoes[indiceSegmento + 1], progressoSegmento);

            yield return null;
        }

        if (alvo != null)
        {
            int targetIdx = reverso ? 0 : posicoes.Length - 1;
            alvo.localPosition = posicoes[targetIdx];
            alvo.localRotation = rotacoes[targetIdx];
        }
    }

    private void ReturnPaper()
    {
        if (insertedPaper == null) return;

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