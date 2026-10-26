using UnityEngine;
using TMPro;

public class HiddenMessaage : MonoBehaviour
{
    public float distanciaMaxima = 3.5f;
    private TextMeshPro textoSecreto;
    private Color corOriginal;
    private UVLantern lanterna;
    
    void Start()
    {
        textoSecreto = GetComponent<TextMeshPro>();
        corOriginal = textoSecreto.color;
        
        corOriginal.a = 0f;
        textoSecreto.color = corOriginal;

        lanterna = FindObjectOfType<UVLantern>();
    }
    
    void Update()
    {
        if (lanterna == null || lanterna.uvLight == null || textoSecreto == null) return;

        float alphaAlvo = 0f;

        if (lanterna.uvLight.enabled)
        {
            float distancia = Vector3.Distance(lanterna.transform.position, transform.position);
            Vector3 direcaoParaTexto = (transform.position - lanterna.transform.position).normalized;
            float angulo = Vector3.Angle(lanterna.transform.forward, direcaoParaTexto);

            if (distancia <= distanciaMaxima && angulo <= lanterna.uvLight.spotAngle / 2f)
            {
                alphaAlvo = 1f; 
            }
        }

        corOriginal.a = Mathf.Lerp(corOriginal.a, alphaAlvo, Time.deltaTime * 6f);
        textoSecreto.color = corOriginal;
    }
}
