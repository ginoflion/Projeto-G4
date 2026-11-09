using UnityEngine;
using TMPro;

public class HiddenMessaage : MonoBehaviour
{
    public float distanciaMaxima = 3.5f;
    public float velocidadeAparecer = 6f;
    public float velocidadeDesaparecer = 1f;
    private TextMeshPro textoSecreto;
    private Color corOriginal;
    private UVLantern lanterna;
    
    void Start()
    {
        textoSecreto = GetComponent<TextMeshPro>();
        corOriginal = textoSecreto.color;
        
        corOriginal.a = 0f;
        textoSecreto.color = corOriginal;
        UVLantern[] lanternas = FindObjectsByType<UVLantern>(FindObjectsSortMode.None);
        if (lanternas.Length > 0)
        {
            lanterna = lanternas[0];
        }
    }
    
    void Update()
    {
        if (lanterna == null || lanterna.uvLight == null || textoSecreto == null) return;

        float alphaAlvo = 0f;

        if (lanterna.uvLight.enabled)
        {
            float distancia = Vector3.Distance(lanterna.uvLight.transform.position, transform.position);
            Vector3 direcaoParaTexto = (transform.position - lanterna.uvLight.transform.position).normalized;
            float angulo = Vector3.Angle(lanterna.uvLight.transform.forward, direcaoParaTexto);

            if (distancia <= distanciaMaxima && angulo <= lanterna.uvLight.spotAngle / 2f)
            {
                alphaAlvo = 1f; 
            }
        }
        float velocidadeAtual = (alphaAlvo > 0f) ? velocidadeAparecer : velocidadeDesaparecer;

        corOriginal.a = Mathf.Lerp(corOriginal.a, alphaAlvo, Time.deltaTime * velocidadeAtual);
        textoSecreto.color = corOriginal;
    }
}