using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class RadioController : MonoBehaviour
{
    [Header("Configuração dos Dígitos")]
    private int[] digits = new int[6];

    [Header("Interface (TextMeshPro 3D)")]
    public TextMeshPro[] digitTexts;

    [System.Serializable]
    public struct FrequenciaPorDia
    {
        [Tooltip("Número do dia (Ex: 1, 2, 3...)")]
        public int dia;
        [Tooltip("Frequência correta para este dia em formato inteiro (Ex: 9520 para 95.2 MHz)")]
        public int frequenciaCorreta;
    }

    [Header("Respostas Corretas por Dia")]
    [Tooltip("Lista que define a frequência exata que o jogador tem de sintonizar em cada dia.")]
    [SerializeField] private List<FrequenciaPorDia> frequenciasDosDias = new List<FrequenciaPorDia>();

    void Start()
    {
        UpdateDisplay();
    }

    public void IncreaseDigit(int digitIndex)
    {
        if (IsValidIndex(digitIndex))
        {
            digits[digitIndex] = (digits[digitIndex] + 1) % 10;
            UpdateDisplay();
            OnFrequencyChanged();
        }
    }

    public void DecreaseDigit(int digitIndex)
    {
        if (IsValidIndex(digitIndex))
        {
            digits[digitIndex] = (digits[digitIndex] - 1 + 10) % 10;
            UpdateDisplay();
            OnFrequencyChanged();
        }
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

    private void OnFrequencyChanged()
    {
        float currentFreq = GetCurrentFrequencyFloat();
        Debug.Log("Frequência FM atual: " + currentFreq.ToString("F2") + " MHz");

        if (IsCurrentFrequencyCorrect())
        {
            Debug.Log("<color=green>Acertaste na frequência correta de hoje!</color>");
        }
    }

    public bool IsCurrentFrequencyCorrect()
    {
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
                return frequenciaAtual == item.frequenciaCorreta;
            }
        }

        Debug.LogWarning($"A frequência correta para o Dia {diaAtual} não foi configurada no RadioController!");
        return false;
    }

    public float GetNormalizedFrequency()
    {
        int rawValue = GetCurrentFrequencyAsInt();
        return (float)rawValue / 999999f;
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

    private bool IsValidIndex(int index)
    {
        return index >= 0 && index < digits.Length;
    }
}