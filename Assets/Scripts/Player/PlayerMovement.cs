using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    // --- Paramètres de Mouvement ---
    
    [Header("Paramètres de Mouvement")]
    [Tooltip("Vitesse de déplacement avant.")]
    public float vitesseAvance = 6.0f;
    [Tooltip("Vitesse de rotation latérale au sol.")]
    public float vitesseRotationSol = 100.0f; // Renommée pour plus de clarté
    [Tooltip("Vitesse de rotation latérale en l'air. Doit être plus élevée.")]
    public float vitesseRotationAerienne = 150.0f; // NOUVEAU PARAMÈTRE
    [Tooltip("Vitesse de décélération/freinage.")]
    public float deceleration = 10.0f;
    [Tooltip("Vitesse d'accélération.")]
    public float acceleration = 2.0f;
    
    [Header("Paramètres de Saut")]
    [Tooltip("Force initiale du saut.")]
    public float forceSaut = 8.0f;
    [Tooltip("Force de la gravité.")]
    public float gravite = 20.0f;
    [Tooltip("Layer pour la détection du sol.")]
    public LayerMask groundLayer;
    [Tooltip("Distance du Raycast pour le sol.")]
    public float groundCheckDistance = 0.2f;

    // --- Variables Internes ---
    private CharacterController _controller;
    private Vector3 _deplacementDirection = Vector3.zero;
    private float _vitesseActuelle = 0.0f;
    private float _accumulatedRotation = 0f; // Track total rotation in air
    private float _jumpCooldownTimer = 0f; // Prevent ground check immediately after jump

    private bool wasJumping = false;

    private CameraShakeController cameraShakeController;

    public GameObject speedEffect;
    [Tooltip("Vitesse d'activation de l'effet.")]
    public float activationEffectSpeed = 6.0f;

    private PlayerScore playerScore;
    private TricksManager tricksManager;

    void Start()
    {
        cameraShakeController = FindFirstObjectByType<CameraShakeController>();
        tricksManager = FindFirstObjectByType<TricksManager>();
        _controller = GetComponent<CharacterController>();
        if (_controller == null)
        {
            Debug.LogError("Le CharacterController est manquant sur l'objet joueur.");
            enabled = false; 
        }
        playerScore = GetComponent<PlayerScore>();
    }

    void Update()
    {
        bool isGrounded = CheckGrounded();
        float inputHorizontal = Input.GetAxis("Horizontal");

        if (isGrounded)
        {
            CheckLanding(isGrounded);
            _deplacementDirection.y = 0f; 

            float inputVertical = Input.GetAxis("Vertical"); 

            if (inputVertical > 0.1f)
            {
                _vitesseActuelle = Mathf.Min(_vitesseActuelle + inputVertical * Time.deltaTime * acceleration, vitesseAvance);
            }
            else
            {
                _vitesseActuelle = Mathf.Max(_vitesseActuelle - Time.deltaTime * deceleration, 0.0f);
            }
            if (_vitesseActuelle >= activationEffectSpeed)
            {
                speedEffect.SetActive(true);
                cameraShakeController.ShakeCameraSpeed(0.5f);
            }
            else
            {
                speedEffect.SetActive(false);
                cameraShakeController.StopShakeCameraSpeed();
            }

            _deplacementDirection = transform.forward * _vitesseActuelle;

            if (Input.GetButtonDown("Jump"))
            {
                _deplacementDirection.y = forceSaut;
                wasJumping = true;
                _accumulatedRotation = 0f; // Reset rotation tracking
                _jumpCooldownTimer = 0.2f; // Cooldown to exit ground check
            }
        }
        
        // Update jump cooldown
        if (_jumpCooldownTimer > 0)
        {
            _jumpCooldownTimer -= Time.deltaTime;
        }
        
        float vitesseEffectiveRotation;
        
        if (isGrounded)
        {
            vitesseEffectiveRotation = vitesseRotationSol;
        }
        else 
        {
            vitesseEffectiveRotation = vitesseRotationAerienne;
            wasJumping = true;
        }

        // Apply and accumulate rotation
        float rotationStep = inputHorizontal * vitesseEffectiveRotation * Time.deltaTime;
        transform.Rotate(0, rotationStep, 0);
        
        if (!isGrounded)
        {
            _accumulatedRotation += Mathf.Abs(rotationStep);
        }

        _deplacementDirection.y -= gravite * Time.deltaTime;

        _controller.Move(_deplacementDirection * Time.deltaTime);
    }

    private void CheckLanding(bool isGrounded)
    {
        if (isGrounded && wasJumping)
        {
            // --- Integration Tricks Manager (Rotation Based) ---
            if (tricksManager != null)
            {
                tricksManager.RegisterLanding(_accumulatedRotation);
            }

            // Optional: Penalty logic for bad alignment could still remain if desired, 
            // but for now we focus on the trick scoring.
            Vector3 mvtDir = new Vector3(_deplacementDirection.x, 0, _deplacementDirection.z);
            Vector3 faceDir = transform.forward;
            
            if (mvtDir.sqrMagnitude > 0.01f)
            {
                mvtDir.Normalize();
                float alignment = Vector3.Dot(mvtDir, faceDir);
                float speedFactor = Mathf.Clamp01(alignment); 
                _vitesseActuelle *= speedFactor;
            }

            wasJumping = false;
            _accumulatedRotation = 0f; // Reset after landing
            cameraShakeController.ShakeCameraLanding(1f, 0.2f);
        }
    }
    private bool CheckGrounded()
    {        
        if (_jumpCooldownTimer > 0) return false;

        Vector3 origin = transform.position + Vector3.up * 0.15f; 
        Debug.DrawRay(origin, Vector3.down * groundCheckDistance, Color.red);
        return Physics.Raycast(origin, Vector3.down, groundCheckDistance, groundLayer);
    }
}