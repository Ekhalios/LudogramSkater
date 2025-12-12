using UnityEngine;
using UnityEngine.UI;

public class ComboGlowController : MonoBehaviour
{
    [Tooltip("Matériau appliqué à l'Image de remplissage.")]
    public Material glowMaterial;
    [Tooltip("Couleur de base pour l'émission.")]
    public Color baseGlowColor = Color.yellow;
    [Tooltip("Intensité maximale de la pulsation (ex: 2.0f pour un effet très vif).")]
    public float maxIntensity = 2.0f;
    [Tooltip("Vitesse de la pulsation.")]
    public float pulseSpeed = 4f;

    private float _initialIntensity;

    void Start()
    {
        if (glowMaterial == null || !glowMaterial.HasProperty("_GlowColor"))
        {
            Debug.LogError("Matériau non valide ou propriété _GlowColor manquante. Vérifiez votre Shader Graph.");
            enabled = false;
        }
        // Sauvegarder l'intensité initiale si vous en avez besoin, sinon utilisez '1.0f'
        _initialIntensity = baseGlowColor.a; 
    }

    void Update()
    {
        // Utilisation de la fonction Sinus pour créer une pulsation douce entre 0 et 1.
        float pulseFactor = Mathf.Sin(Time.time * pulseSpeed) * 0.5f + 0.5f; 
        
        // Multiplier la couleur de base par le facteur de pulsation et l'intensité maximale
        Color finalColor = baseGlowColor * (1f + pulseFactor * maxIntensity);

        // Appliquer la couleur au matériau. Le nom dépend de votre Shader Graph (souvent _GlowColor ou _EmissionColor)
        glowMaterial.SetColor("_GlowColor", finalColor);
    }
}