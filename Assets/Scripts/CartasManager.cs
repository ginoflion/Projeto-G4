using System.Collections.Generic;
using UnityEngine;

public class CartasManager : MonoBehaviour
{
    // Estrutura para os dias com escolhas (Dia 2 em diante)
    [System.Serializable]
    public struct DiaComEscolha
    {
        [Tooltip("Nome ou número descritivo do dia (ex: Dia 2, Dia 3...)")]
        public string nomeDoDia;

        [Tooltip("Prefab a instanciar se a escolha for Certa.")]
        public GameObject cartaCertaPrefab;

        [Tooltip("Prefab a instanciar se a escolha for Errada.")]
        public GameObject cartaErradaPrefab;
    }

    [Header("Configuração - Dia 1 (Fixo)")]
    [Tooltip("O único prefab que spawna sempre no Dia 1.")]
    [SerializeField] private GameObject prefabDia1;

    [Header("Configuração - Outros Dias (Dia 2 em diante)")]
    [Tooltip("Lista de dias com escolhas. O índice 0 aqui corresponde ao Dia 2, o índice 1 ao Dia 3, e assim sucessivamente.")]
    [SerializeField] private List<DiaComEscolha> listaDiasComEscolha = new List<DiaComEscolha>();

    [Header("Destino")]
    [Tooltip("O Transform onde a carta será spawnada.")]
    [SerializeField] private Transform localDeSpawn;

    [Header("Debug / Visualização no Inspetor")]
    [Tooltip("Mostra a última escolha recebida pelo sistema.")]
    [SerializeField] private string ultimaEscolha = "Nenhuma";

    public void SpawnarCarta(string escolha)
    {
        // Guarda a string no campo para consegui-la ver no Inspetor
        ultimaEscolha = escolha;

        if (localDeSpawn == null)
        {
            Debug.LogError("O local de spawn não foi atribuído no Inspetor!", this);
            return;
        }

        // Descobre que dia é através do DayManager
        int diaAtual = 1;
        if (DayManager.Instance != null)
        {
            diaAtual = DayManager.Instance.GetCurrentDay();
        }

        GameObject prefabParaSpawnar = null;

        if (diaAtual == 1)
        {
            // Dia 1: Usa sempre o objeto fixo, independentemente da escolha
            prefabParaSpawnar = prefabDia1;
        }
        else
        {
            // Dia 2 em diante: Converte o dia atual para o índice da lista 
            // (Dia 2 = Índice 0 da lista, Dia 3 = Índice 1, etc.)
            int indiceLista = diaAtual - 2;

            if (indiceLista < 0 || indiceLista >= listaDiasComEscolha.Count)
            {
                Debug.LogError($"O Dia {diaAtual} não foi configurado na lista de escolhas do CartasManager!", this);
                return;
            }

            DiaComEscolha dadosDoDia = listaDiasComEscolha[indiceLista];

            // Verifica se a escolha foi "certa" ou "errada"
            if (escolha.ToLower() == "certa")
            {
                prefabParaSpawnar = dadosDoDia.cartaCertaPrefab;
            }
            else if (escolha.ToLower() == "errada")
            {
                prefabParaSpawnar = dadosDoDia.cartaErradaPrefab;
            }
            else
            {
                Debug.LogWarning($"Escolha inválida recebida: '{escolha}'. Use 'certa' ou 'errada'.", this);
                return;
            }
        }

        // Faz o Instantiate se o prefab existir
        if (prefabParaSpawnar != null)
        {
            Instantiate(prefabParaSpawnar, localDeSpawn.position, localDeSpawn.rotation, localDeSpawn);
            Debug.Log(diaAtual == 1 ? "Spawnou o objeto fixo do Dia 1." : $"Spawnou a carta para o Dia {diaAtual} ({escolha}).");
        }
        else
        {
            Debug.LogError($"O prefab a spawnar para o Dia {diaAtual} é nulo!", this);
        }
    }
}