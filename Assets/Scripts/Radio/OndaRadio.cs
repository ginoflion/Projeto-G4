using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class OndaRadio : MonoBehaviour
{
    [Header("Referências")]
    public RadioManager radioController;

    [Header("Configuração da Onda")]
    [Range(0f, 50f)] public float minFrequency = 5f;
    [Range(10f, 150f)] public float maxFrequency = 80f;
    public float scrollSpeed = 1.5f;
    public float heightAmplitude = 0.2f;
    [Range(0.01f, 0.5f)] public float edgeFade = 0.2f;

    [Header("Linha Central (Baseline)")]
    public bool showBaseline = true;
    public Color baselineColor = new Color(0, 0.4f, 0, 0.5f);
    public float baselineWidth = 0.005f;

    [Header("Aspeto")]
    public Color waveColor = Color.green;
    [Range(0.001f, 0.05f)] public float lineWidth = 0.015f;

    private Material waveMaterial;

    void Start()
    {
        Renderer rend = GetComponent<Renderer>();
        if (rend != null)
        {
            waveMaterial = rend.material;
        }
    }

    void Update()
    {
        if (radioController == null || waveMaterial == null) return;

        int rawValue = radioController.GetCurrentFrequencyAsInt();

        // SE ESTIVER TUDO A 0, a amplitude fica a 0 (linha reta pura)
        float currentAmplitude = (rawValue <= 0) ? 0f : heightAmplitude;

        // Normalização ajustada para dar mais sensibilidade
        float normalizedFreq = radioController.GetNormalizedFrequency();
        float currentWaveFreq = Mathf.Lerp(minFrequency, maxFrequency, normalizedFreq);

        // Envia para o Shader
        waveMaterial.SetFloat("_Frequency", currentWaveFreq);
        waveMaterial.SetFloat("_Amplitude", currentAmplitude);
        waveMaterial.SetFloat("_Speed", scrollSpeed);
        waveMaterial.SetFloat("_LineWidth", lineWidth);
        waveMaterial.SetFloat("_EdgeFade", edgeFade);
        waveMaterial.SetFloat("_ShowBaseline", showBaseline ? 1f : 0f);
        waveMaterial.SetColor("_LineColor", waveColor);
        waveMaterial.SetColor("_BaselineColor", baselineColor);
        waveMaterial.SetFloat("_BaselineWidth", baselineWidth);
    }
}