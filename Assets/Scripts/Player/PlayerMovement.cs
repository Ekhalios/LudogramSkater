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
    [Tooltip("Layer pour la détection des rails de grind.")]
    public LayerMask grindLayer;
    [Tooltip("Distance du Raycast pour le sol.")]
    public float groundCheckDistance = 0.2f;

    // --- Variables Internes ---
    private CharacterController _controller;
    private Vector3 _deplacementDirection = Vector3.zero;
    private float _vitesseActuelle = 0.0f;
    private float _accumulatedRotation = 0f; // Track total rotation in air
    private float _jumpCooldownTimer = 0f; // Prevent ground check immediately after jump
    private bool _isWindPlaying = false; // Check if wind audio is active
    private bool _isGrinding = false; // Grind state
    private float _grindStartTime = 0f; // Track grind duration

    private bool wasJumping = false;

    private CameraShakeController cameraShakeController;

    public GameObject speedEffect;
    [Tooltip("Vitesse d'activation de l'effet.")]
    public float activationEffectSpeed = 6.0f;

    private PlayerScore playerScore;
    private TricksManager tricksManager;
    private AudioManager audioManager;

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
        audioManager = FindFirstObjectByType<AudioManager>();
    }

    void Update()
    {
        bool isGrounded = CheckGrounded();
        float inputHorizontal = Input.GetAxis("Horizontal");

        if (_isGrinding)
        {
            if (CheckGrind(out Vector3 grindPoint, out Vector3 grindDir))
            {
                _deplacementDirection.y = 0f;
                // Snap Y
                Vector3 targetPos = transform.position;
                targetPos.y = grindPoint.y + 0.15f; 
                transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * 20f);

                // Axis Movement
                float dot = Vector3.Dot(transform.forward, grindDir);
                Vector3 moveDir = (dot > 0) ? grindDir : -grindDir;
                
                // Speed Input
                float inputVertical = Input.GetAxis("Vertical");
                if (inputVertical > 0.1f)
                {
                        _vitesseActuelle = Mathf.Min(_vitesseActuelle + inputVertical * Time.deltaTime * acceleration, vitesseAvance);
                }
                
                _deplacementDirection = moveDir * _vitesseActuelle;

                // Jump Exit
                if (Input.GetButtonDown("Jump"))
                {
                    _isGrinding = false;
                    
                    // Register Grind Trick
                    float duration = Time.time - _grindStartTime;
                    if (tricksManager != null) tricksManager.RegisterGrind(duration);

                    _deplacementDirection.y = forceSaut;
                    wasJumping = true;
                    _accumulatedRotation = 0f;
                    _jumpCooldownTimer = 0.2f;
                    audioManager.StopAudio("Grind");
                    audioManager.PlayAudio("Jump");
                }
            }
            else
            {
                // Rail ended
                _isGrinding = false;
                
                // Register Grind Trick
                float duration = Time.time - _grindStartTime;
                if (tricksManager != null) tricksManager.RegisterGrind(duration);

                audioManager.StopAudio("Grind");
            }
        }
        else if (isGrounded)
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
                
                if (!_isWindPlaying)
                {
                    audioManager.PlayAudio("Wind");
                    _isWindPlaying = true;
                }
            }
            else
            {
                speedEffect.SetActive(false);   
                cameraShakeController.StopShakeCameraSpeed();
                
                if (_isWindPlaying)
                {
                    audioManager.StopAudio("Wind", true, 0.5f); // Fade out over 0.5s
                    _isWindPlaying = false;
                }
            }

            _deplacementDirection = transform.forward * _vitesseActuelle;

            if (Input.GetButtonDown("Jump"))
            {
                audioManager.PlayAudio("Jump");
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
            audioManager.PlayAudio("Landing");
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

        Vector3 origin = transform.position + Vector3.up * 0.1f; 
        Debug.DrawRay(origin, Vector3.down * groundCheckDistance, Color.red);
        return Physics.Raycast(origin, Vector3.down, groundCheckDistance, groundLayer);
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        // Check for Grind Entry via Collision
        if (!_isGrinding && _deplacementDirection.y < 0) // Falling onto it (removed invalid _isGrounded check)
        {
             if (((1 << hit.gameObject.layer) & grindLayer) != 0)
             {
                 _isGrinding = true;
                 _grindStartTime = Time.time; // Start Timer
                 audioManager.PlayAudio("Grind");
                 wasJumping = false; 
                 cameraShakeController.ShakeCameraLanding(0.5f, 0.1f);
             }
        }
    }

    private bool CheckGrind(out Vector3 hitPoint, out Vector3 hitDirection)
    {
        hitPoint = Vector3.zero;
        hitDirection = Vector3.forward;

        // Use SphereCast for wider detection (more forgiving) instead of a single Raycast
        if (Physics.SphereCast(transform.position + Vector3.up * 0.5f, 0.3f, Vector3.down, out RaycastHit hit, 1.0f, grindLayer))
        {
            hitPoint = hit.point;
            hitDirection = hit.transform.forward;
            return true;
        }
        return false;
    }
}