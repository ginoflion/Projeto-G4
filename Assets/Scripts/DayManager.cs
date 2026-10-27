using UnityEngine;
using TMPro;

public class DayManager : MonoBehaviour
{
    public static DayManager Instance { get; private set; }

    [Header("Configuração de Dias")]
    [SerializeField] private int currentDay = 1;

    [Header("UI")]
    [Tooltip("Texto na UI que mostra o dia atual.")]
    public TextMeshProUGUI dayText;

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
        UpdateDayUI();
    }

    public void AdvanceDay()
    {
        currentDay++;
        UpdateDayUI();
    }

    private void UpdateDayUI()
    {
        if (dayText != null)
        {
            dayText.text = "Dia " + currentDay;
        }
    }

    public int GetCurrentDay()
    {
        return currentDay;
    }
}