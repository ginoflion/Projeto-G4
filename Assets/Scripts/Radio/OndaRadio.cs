using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class OndaRadio : MonoBehaviour
{
    [Header("Referências")]
    public RadioController radioController;

    [Header("Configuração Geral")]
    public int resolution = 50;
    public float width = 2f;
    public float heightAmplitude = 0.15f;
    public float scrollSpeed = 1.5f;
    [Range(0.01f, 0.5f)] public float edgeFade = 0.2f;

    [Header("Linha Reta Central (Baseline)")]
    [Tooltip("Ativa a linha reta fixa/subtil a passar pelo centro em simultâneo com a onda.")]
    public bool showBaseline = true;
    [Tooltip("Espessura da linha reta central.")]
    public float baselineWidth = 0.02f;

    private LineRenderer waveLineRenderer;
    private LineRenderer baselineRenderer;
    private float timeOffset;

    void Start()
    {
        // 1. Configura o LineRenderer principal (que vai desenhar a onda)
        waveLineRenderer = GetComponent<LineRenderer>();
        waveLineRenderer.positionCount = resolution;
        waveLineRenderer.useWorldSpace = false;

        // 2. Cria automaticamente um segundo objeto filho para a linha reta central (Baseline)
        if (showBaseline)
        {
            GameObject baselineObj = new GameObject("CentralBaseline");
            baselineObj.transform.SetParent(transform, false);

            baselineRenderer = baselineObj.AddComponent<LineRenderer>();
            baselineRenderer.positionCount = 2; // Apenas precisa de 2 pontos (início e fim)
            baselineRenderer.useWorldSpace = false;

            // Copia o material do LineRenderer principal para manter a mesma cor/estilo
            baselineRenderer.material = waveLineRenderer.material;

            // Define a grossura da linha central
            baselineRenderer.startWidth = baselineWidth;
            baselineRenderer.endWidth = baselineWidth;

            // Desenha a linha reta estática de uma ponta à outra no centro (y = 0)
            baselineRenderer.SetPosition(0, new Vector3(-width / 2f, 0f, 0f));
            baselineRenderer.SetPosition(1, new Vector3(width / 2f, 0f, 0f));
        }
    }

    void Update()
    {
        if (radioController == null) return;

        float normalizedFreq = radioController.GetNormalizedFrequency();
        timeOffset += Time.deltaTime * scrollSpeed;

        DrawWave(normalizedFreq);
    }

    void DrawWave(float freqFactor)
    {
        for (int i = 0; i < resolution; i++)
        {
            float progress = (float)i / (resolution - 1);
            float xPos = (progress - 0.5f) * width;

            float waveFrequency = Mathf.Lerp(4f, 15f, freqFactor);
            float sineWave = Mathf.Sin((progress * waveFrequency) + timeOffset);

            // Fade nas pontas para a onda morrer suavemente nas extremidades
            float distanceFromCenter = Mathf.Abs(progress - 0.5f);
            float fadeMultiplier = 1f - Mathf.Clamp01((distanceFromCenter - (0.5f - edgeFade)) / edgeFade);

            // Altura final da onda
            float yPos = sineWave * heightAmplitude * fadeMultiplier;

            Vector3 pointPos = new Vector3(xPos, yPos, 0f);
            waveLineRenderer.SetPosition(i, pointPos);
        }
    }
}