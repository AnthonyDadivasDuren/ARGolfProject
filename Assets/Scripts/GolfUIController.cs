using TMPro;
using UnityEngine;

public class GolfUIController : MonoBehaviour
{
    [SerializeField] private GolfBallController ball;
    [SerializeField] private TMP_Text strokeText;
    [SerializeField] private GameObject completionPanel;
    [SerializeField] private TMP_Text resultText;
    [SerializeField] private GolfHole hole;

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
            resultText.text =
                $"Hole complete!\n{ball.StrokeCount} {label}";

            completionPanel.SetActive(true);
        }
    }

    public void RestartGame()
    {
        hole.ResetDetection();
        ball.RestartHole();

        resultShown = false;
        strokeText.text = "Strokes: 0";
        completionPanel.SetActive(false);
    }
}