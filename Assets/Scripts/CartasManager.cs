using System.Collections.Generic;
using UnityEngine;

public class CartasManager : MonoBehaviour
{
    public static CartasManager Instance { get; private set; }

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
    [SerializeField] private GameObject prefabDia1;

    [Header("Configuração - Outros Dias (Dia 2 em diante)")]
    [SerializeField] private List<DiaComEscolha> listaDiasComEscolha = new List<DiaComEscolha>();

    [Header("Destino")]
    [SerializeField] private Transform localDeSpawn;

    [Header("Debug / Visualização no Inspetor")]
    [SerializeField] private string ultimaEscolhaRecebida = "certa"; // Podes definir uma predefinição

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        // Spawna o objeto do Dia 1 logo no início do jogo
        SpawnarCarta(ultimaEscolhaRecebida);
    }

    // Outros scripts (como sistemas de escolha) podem chamar esta função para atualizar a escolha guardada
    public void DefinirEscolha(string escolha)
    {
        ultimaEscolhaRecebida = escolha;
    }

    // A porta apenas chama esta função ao avançar o dia
    public void SpawnarCarta()
    {
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
            prefabParaSpawnar = prefabDia1;
        }
        else
        {
            int indiceLista = diaAtual - 2;

            if (indiceLista < 0 || indiceLista >= listaDiasComEscolha.Count)
            {
                Debug.LogError($"O Dia {diaAtual} não foi configurado na lista de escolhas do CartasManager!", this);
                return;
            }

            DiaComEscolha dadosDoDia = listaDiasComEscolha[indiceLista];

            // Verifica a string guardada para decidir se é Certa ou Errada
            if (ultimaEscolhaRecebida.ToLower() == "certa")
            {
                prefabParaSpawnar = dadosDoDia.cartaCertaPrefab;
            }
            else if (ultimaEscolhaRecebida.ToLower() == "errada")
            {
                prefabParaSpawnar = dadosDoDia.cartaErradaPrefab;
            }
            else
            {
                Debug.LogWarning($"Escolha inválida registada: '{ultimaEscolhaRecebida}'. Use 'certa' ou 'errada'.", this);
                return;
            }
        }

        if (prefabParaSpawnar != null)
        {
            Instantiate(prefabParaSpawnar, localDeSpawn.position, localDeSpawn.rotation, localDeSpawn);
            Debug.Log(diaAtual == 1 ? "Spawnou o objeto fixo do Dia 1." : $"Spawnou a carta para o Dia {diaAtual} com resultado: {ultimaEscolhaRecebida}");
        }
        else
        {
            Debug.LogError($"O prefab a spawnar para o Dia {diaAtual} é nulo!", this);
        }
    }

    // Sobrecarga caso queiras passar a escolha diretamente no momento do spawn
    public void SpawnarCarta(string escolha)
    {
        DefinirEscolha(escolha);
        SpawnarCarta();
    }
}