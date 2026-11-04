using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class RadioController : MonoBehaviour
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
        [Tooltip("Material padrão quando NÃO está selecionado.")]
        public Material normalMaterial;
    }

    [Header("Indicadores Visuais (6 no total)")]
    [Tooltip("Coloca aqui os 6 indicadores visuais na ordem de 0 a 5.")]
    [SerializeField] private IndicatorInfo[] indicators = new IndicatorInfo[6];
    [Tooltip("Material a aplicar quando o indicador estiver SELECIONADO.")]
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

    [Header("Ponto de Spawn (Opcional)")]
    [SerializeField] private Transform spawnPoint;

    private bool jaSpawnouHoje = false;

    void Start()
    {
        UpdateDisplay();
        UpdateVisualIndicators();
    }

    // --- NAVEGAÇÃO BLOQUEADA NAS PONTAS (Não faz loop) ---
    public void SelectPreviousDigit()
    {
        // Se estiver no 0, mantém-se no 0
        selectedIndex = Mathf.Max(0, selectedIndex - 1);
        UpdateVisualIndicators();
        Debug.Log("Dígito selecionado: " + selectedIndex);
    }

    public void SelectNextDigit()
    {
        // Se estiver no último (digits.Length - 1), mantém-se lá
        selectedIndex = Mathf.Min(digits.Length - 1, selectedIndex + 1);
        UpdateVisualIndicators();
        Debug.Log("Dígito selecionado: " + selectedIndex);
    }

    // --- ALTERAÇÃO DE VALOR BLOQUEADA NO 0 E NO 6 ---
    public void IncreaseSelectedDigit()
    {
        // Se chegar a 6, fica bloqueado no 6
        digits[selectedIndex] = Mathf.Min(6, digits[selectedIndex] + 1);
        UpdateDisplay();
    }

    public void DecreaseSelectedDigit()
    {
        // Se chegar a 0, fica bloqueado no 0
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

    private void UpdateVisualIndicators()
    {
        if (indicators == null) return;

        for (int i = 0; i < indicators.Length; i++)
        {
            if (indicators[i].indicatorRenderer != null)
            {
                if (i == selectedIndex && selectedMaterial != null)
                {
                    indicators[i].indicatorRenderer.material = selectedMaterial;
                }
                else if (indicators[i].normalMaterial != null)
                {
                    indicators[i].indicatorRenderer.material = indicators[i].normalMaterial;
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
                Vector3 posicaoSpawn = spawnPoint != null ? spawnPoint.position : transform.position;
                Quaternion rotacaoSpawn = spawnPoint != null ? spawnPoint.rotation : Quaternion.identity;

                Instantiate(prefabParaSpawnar, posicaoSpawn, rotacaoSpawn);
                jaSpawnouHoje = true;
                Debug.Log("Objeto spawnado com sucesso!");
            }
        }
        else
        {
            Debug.Log("<color=red>Frequência incorreta!</color>");
        }
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

        Debug.LogWarning($"A frequência correta para o Dia {diaAtual} não foi configurada no RadioController!");
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