using TMPro;
using UnityEngine;

public class GolfUIController : MonoBehaviour
{
    [SerializeField] private GolfBallController ball;
    [SerializeField] private TMP_Text strokeText;
    [SerializeField] private GameObject completionPanel;
    [SerializeField] private TMP_Text resultText;
    [SerializeField] private GolfLevelManager levelManager;
    [SerializeField] private GameObject nextLevelButton;

    private bool resultShown;

    private void Start()
    {
        completionPanel.SetActive(false);
        strokeText.text = "Strokes: 0";
    }

    private void Update()
    {
        strokeText.text = $"Strokes: {ball.StrokeCount}";

        if (ball.HoleComplete && !resultShown)
        {
            resultShown = true;

            string label = ball.StrokeCount == 1 ? "stroke" : "strokes";
            bool moreLevels = !levelManager.IsLastLevel;

            nextLevelButton.SetActive(moreLevels);

            resultText.text = moreLevels
                ? $"Hole {levelManager.CurrentIndex + 1} complete!\n{ball.StrokeCount} {label}"
                : $"Course complete!\n{ball.StrokeCount} {label}";

            completionPanel.SetActive(true);
        }
    }

    public void RestartGame()
    {
        levelManager.RestartLevel();
        ResetUI();
    }

    public void NextLevel()
    {
        levelManager.NextLevel();
        ResetUI();
    }

    private void ResetUI()
    {
        resultShown = false;
        strokeText.text = "Strokes: 0";
        completionPanel.SetActive(false);
    }
}