using UnityEngine;

public class PlayerScore : MonoBehaviour
{

    private int score = 0;

    [SerializeField] private ScoreEffect scoreEffect;
    
    public void AddScore(int amount)
    {
        score += amount;
        scoreEffect.UpdateText(score.ToString(), false);
    }

    public int GetScore()
    {
        return score;
    }

    public void ResetScore()
    {
        score = 0;
        scoreEffect.UpdateText(score.ToString(), false);
    }
}
