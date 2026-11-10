using UnityEngine;
using TMPro;

public class DayManager : MonoBehaviour
{
    public static DayManager Instance { get; private set; }

    [Header("Configuração de Dias")]
    [SerializeField] private int currentDay = 1;


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


    public void AdvanceDay()
    {
        AnswerManager.Instance.currentDayIndex = DayManager.Instance.GetCurrentDay();
        currentDay++;
    }

    public int GetCurrentDay()
    {
        return currentDay;
    }
}