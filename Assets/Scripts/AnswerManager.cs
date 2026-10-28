using System;
using UnityEngine;

public class AnswerManager : MonoBehaviour
{
    public static AnswerManager Instance; 
    public DayData[] allDays; 
    public int currentDayIndex = 0;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        currentDayIndex = DayManager.Instance.GetCurrentDay();
    }

    public DayData GetCurrentDayData()
    {
        if (currentDayIndex >= 0 && currentDayIndex < allDays.Length)
        {
            return allDays[currentDayIndex];
        }
        return null;
    }
    
    public void SetDayResult(bool success)
    {
        DayData currentDay = GetCurrentDayData();
        if (currentDay != null)
        {
            currentDay.dayStatus = success ? "Success" : "Failure";
            Debug.Log($"[AnswerManager] {currentDay.dayName} marcado como: {currentDay.dayStatus}");
        }
    }
}