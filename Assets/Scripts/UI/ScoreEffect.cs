using UnityEngine;
using TMPro;
using System.Collections;

public class ScoreEffect : MonoBehaviour
{
    [Header("UI Reference")]
    public TextMeshProUGUI scoreText;

    [Header("Idle Animation (Breathing)")]
    [Tooltip("Min scale for breathing.")]
    public float minScale = 0.9f;
    [Tooltip("Max scale for breathing.")]
    public float maxScale = 1.2f;
    [Tooltip("Speed of the breathing animation.")]
    public float breathingSpeed = 3.0f;

    [Header("Impact Animation (On Score)")]
    [Tooltip("Scale peak when adding score.")]
    public float punchScale = 1.5f;
    [Tooltip("Duration of the punch effect.")]
    public float punchDuration = 0.2f;
    [Tooltip("Color check to flash on score add?")]
    public bool useColorFlash = true;
    public Color flashColor = Color.yellow;

    private Vector3 _baseScale;
    private Color _baseColor;
    private bool _isPunching = false;
    private Coroutine _punchCoroutine;

    void Start()
    {
        if (scoreText == null)
        {
            scoreText = GetComponent<TextMeshProUGUI>();
        }

        if (scoreText != null)
        {
            _baseScale = scoreText.transform.localScale;
            _baseColor = scoreText.color;
        }
    }

    void Update()
    {
        if (scoreText == null) return;

        // Apply breathing only if not currently punching
        if (!_isPunching)
        {
            float sine = Mathf.Sin(Time.time * breathingSpeed);
            // Map sine [-1, 1] to [0, 1]
            float t = (sine + 1f) * 0.5f; 
            float currentScale = Mathf.Lerp(minScale, maxScale, t);

            scoreText.transform.localScale = _baseScale * currentScale;
        }
    }

    /// <summary>
    /// Updates the score text and triggers the impact effect.
    /// </summary>
    /// <param name="newTotalScore">The value to display.</param>
    public void UpdateText(string newTotalScore, bool disabledAtEnd)
    {
        if (scoreText != null)
        {
            scoreText.text = newTotalScore;
        }
        PlayImpactEffect(disabledAtEnd);
    }
    
    /// <summary>
    /// Overload if you just want to trigger the effect without changing text yet (or for testing).
    /// </summary>
    public void PlayImpactEffect(bool disabledAtEnd)
    {
        if (_punchCoroutine != null) StopCoroutine(_punchCoroutine);
        _punchCoroutine = StartCoroutine(ImpactRoutine(disabledAtEnd));
    }

    private IEnumerator ImpactRoutine(bool disabledAtEnd)
    {
        _isPunching = true;
        float elapsed = 0f;
        Vector3 startScale = scoreText.transform.localScale;
        Vector3 targetScale = _baseScale * punchScale;
        
        // Phase 1: Scale Up (Punch)
        while (elapsed < punchDuration * 0.5f)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / (punchDuration * 0.5f);
            
            // Ease out elastic or just smooth step for impact
            scoreText.transform.localScale = Vector3.Lerp(startScale, targetScale, t);
            
            if (useColorFlash)
            {
                scoreText.color = Color.Lerp(_baseColor, flashColor, t);
            }
            yield return null;
        }

        // Phase 2: Scale Down (Return)
        elapsed = 0f;
        while (elapsed < punchDuration * 0.5f)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / (punchDuration * 0.5f);
            
            scoreText.transform.localScale = Vector3.Lerp(targetScale, _baseScale * maxScale, t); // return to roughly max breathing scale to blend
            if (useColorFlash)
            {
                scoreText.color = Color.Lerp(flashColor, _baseColor, t);
            }
            yield return null;
        }

        // Reset
        if (useColorFlash) scoreText.color = _baseColor;
        _isPunching = false;
        if (disabledAtEnd)
        {
            scoreText.gameObject.SetActive(false);
        }
    }
}
