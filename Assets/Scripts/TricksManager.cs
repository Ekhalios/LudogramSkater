using UnityEngine;
using UnityEngine.UI;

public class TricksManager : MonoBehaviour
{
    [Header("Rotation Scores")]
    public int score360 = 500;
    public int score270 = 250;
    
    [Header("Combo Settings")]
    public float comboWindow = 3.0f; // Time to perform next trick
    public float comboMultiplierStep = 0.5f; // +0.5x per trick
    
    [Header("References")]
    public PlayerScore playerScore;
    public ScoreEffect TrickTextEffect; // Shows "360!", "270!"
    public ScoreEffect ComboTextEffect; // Shows "x2", "x3" (NEW)
    public Image comboBarImage; // Fills/Unfills based on time (NEW)

    private float _comboTimer = 0f;
    private int _currentCombo = 1;

    void Start()
    {
        if (playerScore == null)
        {
            playerScore = FindFirstObjectByType<PlayerScore>();
        }
        
        if (comboBarImage != null)
        {
            comboBarImage.fillAmount = 0f;
        }
    }

    void Update()
    {
        // Manage Combo Timer
        if (_comboTimer > 0)
        {
            _comboTimer -= Time.deltaTime;
            
            // Update Combo Bar
            if (comboBarImage != null)
            {
                comboBarImage.fillAmount = _comboTimer / comboWindow;
            }

            // Combo specific end check
            if (_comboTimer <= 0)
            {
                ResetCombo();
            }
        }
    }

    /// <summary>
    /// Evaluates the landing based on total rotation degrees performed in air.
    /// </summary>
    /// <param name="rotationDegrees">Total absolute degrees rotated.</param>
    public void RegisterLanding(float rotationDegrees)
    {
        if (playerScore == null) return;

        int baseScore = 0;
        string trickName = "";

        // Check for 360 (allow some margin, e.g., >= 330)
        if (rotationDegrees >= 330f)
        {
            baseScore = score360;
            trickName = "INSANE 360 !";
        }
        // Check for 270 (allow margin, e.g., >= 240)
        else if (rotationDegrees >= 240f)
        {
            baseScore = score270;
            trickName = "WAW ! 270 !";
        }
        else
        {
            Debug.Log($"No Trick. Rotation: {rotationDegrees:F1}");
            // Optional: Break combo on failed trick? Or just don't add to it?
            // For now, we just don't add points, but don't punish combo unless time runs out.
            return; 
        }

        // Apply Combo
        if (baseScore > 0)
        {
            // If timer was running, increment combo. Else start at 1.
            // Note: Update() handles the timeout, so if we are here and valid trick, we extend.
            if (_comboTimer > 0)
            {
                _currentCombo++;
            }
            else
            {
                _currentCombo = 1;
            }

            // Refresh Timer
            _comboTimer = comboWindow;

            // Calculate Multiplier
            // Example: Combo 1 = 1x
            // Combo 2 = 1.5x (if step is 0.5)
            // Combo 3 = 2.0x
            float multiplier = 1f + (_currentCombo - 1) * comboMultiplierStep;
            int finalScore = Mathf.RoundToInt(baseScore * multiplier);

            Debug.Log($"TRICK! {trickName} | Combo: x{multiplier} ({_currentCombo}) | Score: {finalScore}");

            // Add Score
            playerScore.AddScore(finalScore);

            // UI Feedback
            if (TrickTextEffect != null)
            {
                TrickTextEffect.gameObject.SetActive(true);
                TrickTextEffect.UpdateText(trickName, true);
            }

            if (ComboTextEffect != null && _currentCombo > 1)
            {
                ComboTextEffect.gameObject.SetActive(true);
                ComboTextEffect.UpdateText($"x{multiplier:F1}", false);
            }
        }
    }

    private void ResetCombo()
    {
        _currentCombo = 1;
        _comboTimer = 0f;
        if (comboBarImage != null) comboBarImage.fillAmount = 0f;
    }
}
