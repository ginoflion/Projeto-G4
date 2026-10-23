using System.Collections.Generic;
using UnityEngine;

public class CartasManager : MonoBehaviour
{
    [Header("Configuração de Cartas")]
    [Tooltip("Lista de prefabs de cartas na ordem em que devem aparecer.")]
    [SerializeField] private List<GameObject> listaDeCartas = new List<GameObject>();

    [Header("Destino")]
    [Tooltip("O Transform onde a carta será spawnada.")]
    [SerializeField] private Transform localDeSpawn;

    private int indiceAtual = 0;

    public void SpawnarProximaCarta()
    {
        if (listaDeCartas == null || listaDeCartas.Count == 0)
        {
            Debug.LogWarning("A lista de cartas está vazia!", this);
            return;
        }

        if (localDeSpawn == null)
        {
            Debug.LogError("O local de spawn não foi atribuído no Inspetor!", this);
            return;
        }

        if (indiceAtual >= listaDeCartas.Count)
        {
            Debug.Log("Todas as cartas já foram spawnadas!");
            return;
        }

        GameObject prefabCarta = listaDeCartas[indiceAtual];

        if (prefabCarta != null)
        {
            Instantiate(prefabCarta, localDeSpawn.position, localDeSpawn.rotation, localDeSpawn);

            Debug.Log($"Spawnou a carta número {indiceAtual + 1}: {prefabCarta.name}");
            indiceAtual++;
        }
        else
        {
            Debug.LogError($"O prefab na posição {indiceAtual} da lista é nulo!", this);
        }
    }

    public void ResetarContador()
    {
        indiceAtual = 0;
        Debug.Log("Contador de cartas reiniciado.");
    }
}