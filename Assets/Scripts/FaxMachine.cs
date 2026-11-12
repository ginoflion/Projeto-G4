using SUPERCharacter;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

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
    [Tooltip("Tempo em segundos que o colisor do papel fica desativado ao inserir ou retirar.")]
    [SerializeField] private float duracaoDesativadoCollider = 1.0f;

    [Header("Áudio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip somInserirPapel;
    [SerializeField] private AudioClip somRetirarPapel;
    [SerializeField] private AudioClip transmitir;
    [SerializeField] private AudioClip somErroCarimbado;

    private bool isLookingAtFax = false;
    private bool isAnimating = false;
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
        if (isAnimating) return;

        if (!isLookingAtFax && playerHand.HasItem() && playerHand.GetItemName() == requiredItemName && insertedPaper == null)
        {
            GameObject itemNaMao = playerHand.GetCurrentItem();
            if (itemNaMao != null)
            {
                PhysicalPaperInput paperCheck = itemNaMao.GetComponent<PhysicalPaperInput>();
                if (paperCheck != null && paperCheck.jaFoiCarimbado)
                {
                    if (audioSource != null && somErroCarimbado != null)
                    {
                        audioSource.PlayOneShot(somErroCarimbado);
                    }
                    return;
                }
            }

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
        isAnimating = true; 

        if (pontosDeTrajeto == null || pontosDeTrajeto.Length < 2)
        {
            Debug.LogError("Precisas de configurar pelo menos 2 pontos no array 'Pontos De Trajeto' da FaxMachine!");
            EnterFaxView();
            isAnimating = false; 
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

        StartCoroutine(GerenciarColisorPapel(insertedPaper));
        PhysicalPaperInput paperInput = insertedPaper.GetComponent<PhysicalPaperInput>();
        if (paperInput != null)
        {
            paperInput.HideText();
        }

        EnterFaxView();
        yield return StartCoroutine(AnimarTrajetoPapel(insertedPaper.transform, localPos, localRot, false, tempoViagemPapel));

        isAnimating = false; 
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
        if (!isLookingAtFax || isAnimating) return;

        if (Mouse.current.rightButton.wasPressedThisFrame || Keyboard.current.eKey.wasPressedThisFrame)
        {
            if (insertedPaper != null && itemName != "A transmitir...")
            {
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
        if (isAnimating) return; 
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
        if (insertedPaper != null && itemName != "A transmitir..." && !isAnimating)
        {
            audioSource.PlayOneShot(transmitir);
            StartCoroutine(SendFaxRoutine());
        }
    }

    private IEnumerator SendFaxRoutine()
    {
        isAnimating = true;
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
                List<string> playerAnswers = new List<string>();

                foreach (var simbolo in paperInput.simbolos)
                {
                    if (simbolo != null && !string.IsNullOrEmpty(simbolo.currentInput))
                    {
                        string normalized = NormalizeAnswer(simbolo.currentInput);
                        playerAnswers.Add(normalized);
                    }
                }

                List<string> expectedAnswers = new List<string>();
                foreach (var ans in currentDay.paperAnswers)
                {
                    if (!string.IsNullOrEmpty(ans))
                    {
                        string normalized = NormalizeAnswer(ans);
                        expectedAnswers.Add(normalized);
                    }
                }

                areAnswersCorrect = AreListsEqualUnordered(playerAnswers, expectedAnswers);
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
        if (insertedPaper != null)
        {
            PhysicalPaperInput paperToStamp = insertedPaper.GetComponent<PhysicalPaperInput>();
            if (paperToStamp != null)
            {
                paperToStamp.AplicarCarimbo();
            }
        }

        StartCoroutine(RetirarEVoltarMaoRoutine());
    }

    // FUNÇÕES AUXILIARES

    private string NormalizeAnswer(string rawInput)
    {
        string input = rawInput.Trim().ToLower();

        if (input == "meia-noite" || input == "midnight") input = "00:00";
        if (input == "meio-dia" || input == "noon") input = "12:00";
        if (input == "dock") input = "docks";
        if (input == "stab" || input == "stabed" || input == "knife" || input == "knifed") input = "stabbing";
        if (input == "bar") input = "pub";
        if (input == "poisoned" || input == "rat poison" || input == "drugged") input = "poison";
        if (input == "garbage") input = "dumpsite";
        if (input == "car") input = "run over";
        if (input == "disco") input = "nightclub";
        if (input == "hanged" || input == "suffocated" || input == "suffocate") input = "hanging";
        if (input == "work") input = "office";
        if (input == "gun" || input == "shot at") input = "shot";

        input = NormalizeTimeFormat(input);

        return input;
    }

    private string NormalizeTimeFormat(string input)
    {
        bool isPM = input.Contains("pm");
        bool isAM = input.Contains("am");

        if (isPM || isAM)
        {
            string cleanTime = input.Replace("pm", "").Replace("am", "").Trim();
            if (int.TryParse(cleanTime, out int hour12))
            {
                if (isPM && hour12 < 12) hour12 += 12;
                if (isAM && hour12 == 12) hour12 = 0;
                return $"{hour12:D2}:00";
            }
        }

        if (input.Contains(" "))
        {
            string[] parts = input.Split(new char[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 2 && int.TryParse(parts[0], out int h) && int.TryParse(parts[1], out int m))
            {
                if (h >= 0 && h < 24 && m >= 0 && m < 60)
                {
                    return $"{h:D2}:{m:D2}";
                }
            }
        }

        if (!input.Contains(":") && int.TryParse(input, out int timeDigits))
        {
            if (input.Length == 4)
            {
                int hours = timeDigits / 100;
                int minutes = timeDigits % 100;
                if (hours >= 0 && hours < 24 && minutes >= 0 && minutes < 60)
                {
                    return $"{hours:D2}:{minutes:D2}";
                }
            }
            else if (input.Length == 3)
            {
                int hours = timeDigits / 100;
                int minutes = timeDigits % 100;
                if (hours >= 0 && hours < 24 && minutes >= 0 && minutes < 60)
                {
                    return $"{hours:D2}:{minutes:D2}";
                }
            }
        }

        if (int.TryParse(input, out int hourOnly) && hourOnly >= 0 && hourOnly < 24 && !input.Contains(":"))
        {
            return $"{hourOnly:D2}:00";
        }

        if (System.DateTime.TryParse(input, out System.DateTime parsedTime))
        {
            return parsedTime.ToString("HH:mm");
        }

        return input;
    }

    private bool AreListsEqualUnordered(List<string> list1, List<string> list2)
    {
        if (list1.Count != list2.Count) return false;

        List<string> temp2 = new List<string>(list2);

        foreach (string item in list1)
        {
            if (temp2.Contains(item))
            {
                temp2.Remove(item);
            }
            else
            {
                return false;
            }
        }

        return temp2.Count == 0;
    }

    private IEnumerator GerenciarColisorPapel(GameObject papel)
    {
        if (papel == null) yield break;

        BoxCollider[] colliders = papel.GetComponentsInChildren<BoxCollider>();

        foreach (var col in colliders)
        {
            if (col != null) col.enabled = false;
        }

        yield return new WaitForSeconds(duracaoDesativadoCollider);

        if (papel != null)
        {
            foreach (var col in colliders)
            {
                if (col != null) col.enabled = true;
            }
        }
    }

    private IEnumerator RetirarEVoltarMaoRoutine()
    {
        isAnimating = true; 

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

            StartCoroutine(GerenciarColisorPapel(insertedPaper));

            yield return StartCoroutine(AnimarTrajetoPapel(insertedPaper.transform, localPos, localRot, true, tempoViagemPapel));
        }

        ReturnPaper();
        ExitFaxView();
        isAnimating = false; 
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
        PhysicalPaperInput paperInput = insertedPaper.GetComponent<PhysicalPaperInput>();
        if (paperInput != null)
        {
            paperInput.ShowText();
        }

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