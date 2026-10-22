using UnityEngine;
using TMPro;

public class SubmissionController : MonoBehaviour
{
    public TMP_InputField inputField;
    public string correctAnswer = "Pierre Wilkinson";
    
    public void CheckSubmission()
    {
        string playerInput = inputField.text.Trim().ToLower();
        string expectedAnswer = correctAnswer.Trim().ToLower();

        if (playerInput == expectedAnswer)
        {
            HandleCorrectAnswer();
        }
        else
        {
            HandleWrongAnswer();
        }
    }

    private void HandleCorrectAnswer()
    {
        Debug.Log("GG");
        inputField.text = "";
    }

    private void HandleWrongAnswer()
    {
        Debug.Log("F");
        inputField.text = "";
    }
}
