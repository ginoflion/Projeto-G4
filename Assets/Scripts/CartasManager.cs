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

    [Header("Pontos de Animação da Carta (Trajeto Completo)")]
    [Tooltip("Coloca aqui os teus 4 transforms em ordem: [0] Spawn, [1] Intermédio 1, [2] Intermédio 2, [3] Final.")]
    [SerializeField] private Transform[] pontosDeTrajeto;

    [Header("Duração e Animações do Mecanismo Externo")]
    [SerializeField] private float duracaoMovimento = 1.0f;
    [Tooltip("Arrasta para aqui o GameObject externo que tem o Animator.")]
    [SerializeField] private Animator animatorMecanismo;

    [Header("Animação do Dia 1 vs Trigger Restantes Dias")]
    [Tooltip("Nome exato da animação (state name) que toca diretamente no Dia 1.")]
    [SerializeField] private string nomeAnimacaoDia1 = "NomeDaAnimacaoDia1";
    [Tooltip("Nome do parâmetro Trigger que toca nos Dias 2 em diante.")]
    [SerializeField] private string triggerOutrosDias = "SpawnTrigger";

    [Header("Debug / Visualização no Inspetor")]
    [SerializeField] private string ultimaEscolhaRecebida = "certa";

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

    public void SpawnJornal1()
    {
        SpawnarCarta(ultimaEscolhaRecebida);
    }

    public void DefinirEscolha(string escolha)
    {
        ultimaEscolhaRecebida = escolha;
    }

    public void SpawnarCarta()
    {
        if (pontosDeTrajeto == null || pontosDeTrajeto.Length < 2)
        {
            Debug.LogError("Precisas de atribuir pelo menos 2 pontos no array 'Pontos De Trajeto' do CartasManager!", this);
            return;
        }

        if (pontosDeTrajeto[0] == null || pontosDeTrajeto[pontosDeTrajeto.Length - 1] == null)
        {
            Debug.LogError("O primeiro e o último ponto do trajeto não podem ser nulos!", this);
            return;
        }

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
            Transform pontoInicial = pontosDeTrajeto[0];

            GameObject cartaInstanciada = Instantiate(prefabParaSpawnar, pontoInicial.position, pontoInicial.rotation);
            cartaInstanciada.transform.SetParent(pontoInicial);

            if (animatorMecanismo != null)
            {
                if (diaAtual == 1)
                {
                    if (!string.IsNullOrEmpty(nomeAnimacaoDia1))
                    {
                        animatorMecanismo.Play(nomeAnimacaoDia1);
                        Debug.Log($"Executou a animação direta '{nomeAnimacaoDia1}' para o Dia 1.");
                    }
                }
                else
                {
                    if (!string.IsNullOrEmpty(triggerOutrosDias))
                    {
                        animatorMecanismo.SetTrigger(triggerOutrosDias);
                        Debug.Log($"Disparou a trigger '{triggerOutrosDias}' para o Dia {diaAtual}.");
                    }
                }
            }

            int qtdPontos = pontosDeTrajeto.Length;
            Vector3[] localPositions = new Vector3[qtdPontos];
            Quaternion[] localRotations = new Quaternion[qtdPontos];

            for (int i = 0; i < qtdPontos; i++)
            {
                if (pontosDeTrajeto[i] != null)
                {
                    localPositions[i] = pontoInicial.InverseTransformPoint(pontosDeTrajeto[i].position);
                    localRotations[i] = Quaternion.Inverse(pontoInicial.rotation) * pontosDeTrajeto[i].rotation;
                }
                else
                {
                    localPositions[i] = localPositions[i - 1];
                    localRotations[i] = localRotations[i - 1];
                }
            }

            cartaInstanciada.transform.localPosition = localPositions[0];
            cartaInstanciada.transform.localRotation = localRotations[0];

            StartCoroutine(AnimarTrajetoMultiplo(cartaInstanciada.transform, localPositions, localRotations));
        }
        else
        {
            Debug.LogError($"O prefab a spawnar para o Dia {diaAtual} é nulo!", this);
        }
    }

    public void SpawnarCarta(string escolha)
    {
        DefinirEscolha(escolha);
        SpawnarCarta();
    }

    private System.Collections.IEnumerator AnimarTrajetoMultiplo(Transform cartaTransform, Vector3[] posicoes, Quaternion[] rotacoes)
    {
        float tempoDecorrido = 0f;
        int totalSegmentos = posicoes.Length - 1;

        while (tempoDecorrido < duracaoMovimento)
        {
            if (cartaTransform == null) yield break;

            tempoDecorrido += Time.deltaTime;
            float progressoGlobal = Mathf.Clamp01(tempoDecorrido / duracaoMovimento);

            progressoGlobal = Mathf.Sin(progressoGlobal * Mathf.PI * 0.5f);

            float progressoEscalado = progressoGlobal * totalSegmentos;
            int indiceSegmento = Mathf.FloorToInt(progressoEscalado);

            if (indiceSegmento >= totalSegmentos)
            {
                indiceSegmento = totalSegmentos - 1;
            }

            float progressoSegmento = progressoEscalado - indiceSegmento;

            cartaTransform.localPosition = Vector3.Lerp(posicoes[indiceSegmento], posicoes[indiceSegmento + 1], progressoSegmento);
            cartaTransform.localRotation = Quaternion.Lerp(rotacoes[indiceSegmento], rotacoes[indiceSegmento + 1], progressoSegmento);

            yield return null;
        }

        if (cartaTransform != null)
        {
            cartaTransform.localPosition = posicoes[posicoes.Length - 1];
            cartaTransform.localRotation = rotacoes[posicoes.Length - 1];
        }
    }
}