using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Day", menuName = "DayData")]
public class DayData : ScriptableObject
{
    public string dayName = "Dia 1";
    public List<string> paperAnswers = new List<string>();
    public int correctFaxStationIndex = 4;
    public string dayStatus = "Not Finished";
}