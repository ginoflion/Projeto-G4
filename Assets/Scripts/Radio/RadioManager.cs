using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class RadioManager : MonoBehaviour
{
    [Header("Configuração dos Dígitos")]
    private int[] digits = new int[6];
    private int selectedIndex = 0; // Posição atual selecionada (0 a 5)

    [Header("Interface (TextMeshPro 3D)")]
    public TextMeshPro[] digitTexts;

    [System.Serializable]
    public struct IndicatorInfo
    {
        [Tooltip("O GameObject ou Renderer do indicador visual deste dígito.")]
        public Renderer indicatorRenderer;
        [HideInInspector]
        public Material[] normalMaterials; // Recolhido automaticamente pelo script!
    }

    [Header("Indicadores Visuais (6 no total)")]
    [Tooltip("Coloca aqui os 6 indicadores visuais na ordem de 0 a 5.")]
    [SerializeField] private IndicatorInfo[] indicators = new IndicatorInfo[6];
    [Tooltip("Material a aplicar a TODOS os slots do indicador quando estiver SELECIONADO.")]
    [SerializeField] private Material selectedMaterial;

    [System.Serializable]
    public struct FrequenciaPorDia
    {
        public int dia;
        public int frequenciaCorreta;
        public GameObject prefabParaSpawnar;
    }

    [Header("Respostas e Spawns por Dia")]
    [SerializeField] private List<FrequenciaPorDia> frequenciasDosDias = new List<FrequenciaPorDia>();

    [Header("Pontos de Animação do Papel")]
    [Tooltip("O objeto vazio que define onde o papel nasce (dentro da ranhura).")]
    [SerializeField] private Transform spawnPoint;
    [Tooltip("O objeto vazio que define onde o papel pára (já impresso e fora).")]
    [SerializeField] private Transform finalPoint;
    [SerializeField] private float duracaoMovimento = 1.0f;  // Tempo que demora a viagem

    [Header("Áudio do Rádio")]
    [SerializeField] private AudioSource radioAudioSource;
    [SerializeField] private AudioClip somImpressaoRadio;
    [SerializeField] private AudioClip somInteracaoPapel; // Som tocado quando o player agarra o papel

    private bool jaSpawnouHoje = false;
    private Transform papelGeradoTransform; // Referência para vigiar o papel gerado

    void Start()
    {
        CacheOriginalMaterials(); // Vai buscar os materiais automaticamente ao iniciar
        UpdateDisplay();
        UpdateVisualIndicators();
    }

    // --- CAPTURA AUTOMÁTICA DOS MATERIAIS ORIGINAIS ---
    private void CacheOriginalMaterials()
    {
        if (indicators == null) return;

        for (int i = 0; i < indicators.Length; i++)
        {
            if (indicators[i].indicatorRenderer != null)
            {
                // Guarda uma cópia dos materiais que o objeto já tem aplicados no Unity
                indicators[i].normalMaterials = indicators[i].indicatorRenderer.sharedMaterials;
            }
        }
    }

    // --- NAVEGAÇÃO BLOQUEADA NAS PONTAS (Não faz loop) ---
    public void SelectPreviousDigit()
    {
        selectedIndex = Mathf.Max(0, selectedIndex - 1);
        UpdateVisualIndicators();
        Debug.Log("Dígito selecionado: " + selectedIndex);
    }

    public void SelectNextDigit()
    {
        selectedIndex = Mathf.Min(digits.Length - 1, selectedIndex + 1);
        UpdateVisualIndicators();
        Debug.Log("Dígito selecionado: " + selectedIndex);
    }

    // --- ALTERAÇÃO DE VALOR BLOQUEADA NO 0 E NO 6 ---
    public void IncreaseSelectedDigit()
    {
        digits[selectedIndex] = Mathf.Min(6, digits[selectedIndex] + 1);
        UpdateDisplay();
    }

    public void DecreaseSelectedDigit()
    {
        digits[selectedIndex] = Mathf.Max(0, digits[selectedIndex] - 1);
        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        for (int i = 0; i < digits.Length; i++)
        {
            if (digitTexts != null && i < digitTexts.Length && digitTexts[i] != null)
            {
                digitTexts[i].text = digits[i].ToString();
            }
        }
    }

    // --- ATUALIZAÇÃO DOS MATERIAIS ---
    private void UpdateVisualIndicators()
    {
        if (indicators == null) return;

        for (int i = 0; i < indicators.Length; i++)
        {
            Renderer rend = indicators[i].indicatorRenderer;
            if (rend != null)
            {
                if (i == selectedIndex && selectedMaterial != null)
                {
                    // Aplica o material de seleção a todos os slots
                    Material[] mats = new Material[rend.sharedMaterials.Length];
                    for (int m = 0; m < mats.Length; m++)
                    {
                        mats[m] = selectedMaterial;
                    }
                    rend.materials = mats;
                }
                else
                {
                    // Restaura os materiais originais que foram guardados no Start()
                    if (indicators[i].normalMaterials != null && indicators[i].normalMaterials.Length > 0)
                    {
                        rend.materials = indicators[i].normalMaterials;
                    }
                }
            }
        }
    }

    public void VerificarFrequencia()
    {
        float currentFreq = GetCurrentFrequencyFloat();
        Debug.Log("Frequência FM atual a verificar: " + currentFreq.ToString("F2") + " MHz");

        if (IsCurrentFrequencyCorrect(out GameObject prefabParaSpawnar))
        {
            Debug.Log("<color=green>Acertaste na frequência correta de hoje!</color>");

            if (!jaSpawnouHoje && prefabParaSpawnar != null)
            {
                if (spawnPoint == null || finalPoint == null)
                {
                    Debug.LogError("Falta atribuir o SpawnPoint ou o FinalPoint no RadioManager!");
                    return;
                }

                // 1. Instancia o papel na posição exata do ponto inicial
                GameObject papelInstanciado = Instantiate(prefabParaSpawnar, spawnPoint.position, spawnPoint.rotation);

                // 2. Torna o papel filho do rádio para o acompanhar
                papelInstanciado.transform.SetParent(transform);

                // Guarda a referência para vigiar quando o player o agarrar
                papelGeradoTransform = papelInstanciado.transform;

                // 3. Calcula as posições e rotações locais em relação ao rádio
                Vector3 startLocalPos = transform.InverseTransformPoint(spawnPoint.position);
                Vector3 endLocalPos = transform.InverseTransformPoint(finalPoint.position);
                Quaternion startLocalRot = Quaternion.Inverse(transform.rotation) * spawnPoint.rotation;
                Quaternion endLocalRot = Quaternion.Inverse(transform.rotation) * finalPoint.rotation;

                // Coloca o papel imediatamente no sítio de início
                papelInstanciado.transform.localPosition = startLocalPos;
                papelInstanciado.transform.localRotation = startLocalRot;

                // 4. Dispara a animação entre os dois pontos
                StartCoroutine(AnimarMovimentoEntrePontos(papelInstanciado.transform, startLocalPos, endLocalPos, startLocalRot, endLocalRot));

                // 5. Dispara a coroutine que vigia o momento em que o player agarra o papel
                StartCoroutine(VigiarInteracaoPapel());

                // 6. Toca o som de impressão no rádio
                if (radioAudioSource != null && somImpressaoRadio != null)
                {
                    radioAudioSource.PlayOneShot(somImpressaoRadio);
                }

                jaSpawnouHoje = true;
                Debug.Log("Papel a mover-se do SpawnPoint para o FinalPoint!");
            }
        }
        else
        {
            Debug.Log("<color=red>Frequência incorreta!</color>");
        }
    }

    // Coroutine que faz o papel deslizar suavemente entre o ponto inicial e o final
    private System.Collections.IEnumerator AnimarMovimentoEntrePontos(Transform papelTransform, Vector3 startPos, Vector3 endPos, Quaternion startRot, Quaternion endRot)
    {
        float tempoDecorrido = 0f;

        while (tempoDecorrido < duracaoMovimento)
        {
            if (papelTransform == null) yield break;

            tempoDecorrido += Time.deltaTime;
            float progresso = Mathf.Clamp01(tempoDecorrido / duracaoMovimento);

            // Efeito suave (Ease Out) para a animação começar e acabar com suavidade
            progresso = Mathf.Sin(progresso * Mathf.PI * 0.5f);

            // Interpola a posição e a rotação localmente
            papelTransform.localPosition = Vector3.Lerp(startPos, endPos, progresso);
            papelTransform.localRotation = Quaternion.Lerp(startRot, endRot, progresso);

            yield return null;
        }

        if (papelTransform != null)
        {
            papelTransform.localPosition = endPos;
            papelTransform.localRotation = endRot;
        }
    }

    // Coroutine que vigia se o papel deixou de ser filho do rádio (quando o player o apanha)
    private System.Collections.IEnumerator VigiarInteracaoPapel()
    {
        while (papelGeradoTransform != null && papelGeradoTransform.parent == transform)
        {
            yield return null;
        }

        // Se o loop terminou, significa que o pai mudou (o player interagiu e o tirou de lá)
        if (radioAudioSource != null && somInteracaoPapel != null)
        {
            radioAudioSource.PlayOneShot(somInteracaoPapel);
        }

        papelGeradoTransform = null;
    }

    public bool IsCurrentFrequencyCorrect()
    {
        return IsCurrentFrequencyCorrect(out _);
    }

    private bool IsCurrentFrequencyCorrect(out GameObject prefabEncontrado)
    {
        prefabEncontrado = null;
        int diaAtual = 1;
        if (DayManager.Instance != null)
        {
            diaAtual = DayManager.Instance.GetCurrentDay();
        }

        int frequenciaAtual = GetCurrentFrequencyAsInt();

        foreach (var item in frequenciasDosDias)
        {
            if (item.dia == diaAtual)
            {
                prefabEncontrado = item.prefabParaSpawnar;
                return frequenciaAtual == item.frequenciaCorreta;
            }
        }

        Debug.LogWarning($"A frequência correta para o Dia {diaAtual} não foi configurada no RadioManager!");
        return false;
    }

    public float GetNormalizedFrequency()
    {
        int rawValue = GetCurrentFrequencyAsInt();
        if (rawValue <= 0) return 0f;

        float w0 = digits[0] * 1.0f;
        float w1 = digits[1] * 0.5f;
        float w2 = digits[2] * 0.25f;
        float w3 = digits[3] * 0.12f;
        float w4 = digits[4] * 0.06f;
        float w5 = digits[5] * 0.03f;

        float total = w0 + w1 + w2 + w3 + w4 + w5;
        float maxPossible = 11.76f;

        return Mathf.Clamp01(total / maxPossible);
    }

    public float GetCurrentFrequencyFloat()
    {
        return GetCurrentFrequencyAsInt() / 100f;
    }

    public int GetCurrentFrequencyAsInt()
    {
        string freqStr = "";
        foreach (int d in digits)
        {
            freqStr += d.ToString();
        }
        int.TryParse(freqStr, out int result);
        return result;
    }
}