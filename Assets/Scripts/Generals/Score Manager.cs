using TMPro;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    [SerializeField] TMP_Text scoreText;
    [SerializeField] int scoreToIncrement;

    private int score = 0;

    private void Start()
    {
        score = 0;
        UpdateScoreText();
    }

    public void IncrementScore()
    {
        score += scoreToIncrement;
        UpdateScoreText();
    }

    void UpdateScoreText()
    {
        scoreText.text = $"Score : {score}";
    }
}
