using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Decode : MonoBehaviour
{
    [Header("UI References")]
    public List<TMP_Dropdown> letterDropdowns; // 26 dropdowns de A a Z

    [Header("Text References")]
    public TMP_Text messageText;

    [Header("Fonts")]
    public TMP_FontAsset normalFont;
    public TMP_FontAsset symbolFont;

    private string originalEncodedMessage = "";
    private bool isDecoded = false;

    void Start()
    {
        if (messageText != null)
        {
            originalEncodedMessage = messageText.text;
            messageText.font = symbolFont; // Começa com a fonte de símbolos
        }
    }

    public void ToggleDecodeMessage()
    {
        if (messageText == null) return;

        if (!isDecoded)
        {
            // 1. Criar o dicionário de mapeamento: Símbolo -> Letra Normal
            Dictionary<char, char> symbolToNormalMap = new Dictionary<char, char>();

            for (int i = 0; i < letterDropdowns.Count; i++)
            {
                char normalLetter = (char)('A' + i);
                TMP_Dropdown dropdown = letterDropdowns[i];

                if (dropdown != null && dropdown.value > 0)
                {
                    string selectedOptionText = dropdown.options[dropdown.value].text;
                    if (!string.IsNullOrEmpty(selectedOptionText))
                    {
                        char symbolChar = selectedOptionText[0];
                        if (!symbolToNormalMap.ContainsKey(symbolChar))
                        {
                            symbolToNormalMap.Add(symbolChar, normalLetter);
                        }
                    }
                }
            }

            // 2. Construir o texto decodificado
            string decodedResult = "";
            foreach (char c in originalEncodedMessage)
            {
                // Se o símbolo foi associado, usa a letra normal correspondente
                // Se NÃO foi associado, mantém o símbolo original
                if (symbolToNormalMap.ContainsKey(c))
                {
                    decodedResult += symbolToNormalMap[c];
                }
                else
                {
                    decodedResult += c;
                }
            }

            messageText.text = decodedResult;

            // ATENÇÃO AQUI: Para misturar fontes no mesmo TMP sem mudar tudo, 
            // idealmente usas a fonte normal como principal e a de símbolos como Fallback, 
            // ou vice-versa. 
            messageText.font = normalFont;

            isDecoded = true;
        }
        else
        {
            // Reverte para a mensagem original com símbolos
            messageText.text = originalEncodedMessage;
            messageText.font = symbolFont;
            isDecoded = false;
        }
    }
}