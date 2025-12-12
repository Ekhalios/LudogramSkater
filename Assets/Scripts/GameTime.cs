using UnityEngine;
using TMPro;

public class GameTime : MonoBehaviour
{
    [Tooltip("Temps maximum en secondes (ex: 120 pour 2 minutes).")]
    public float maxTime = 120.0f;
    
    private float _timer;
    private bool _isGameRunning = true;
    
    private PlayerMovement _playerMovement;
    private GameMenu _gameMenu;

    public ScoreEffect timerText;

    private int _lastSecond = -1;

    void Start()
    {
        _timer = maxTime;
        _playerMovement = FindFirstObjectByType<PlayerMovement>();
        _gameMenu = FindFirstObjectByType<GameMenu>();
        
        if (_playerMovement == null) Debug.LogError("PlayerMovement introuvable via GameTime.");
        if (_gameMenu == null) Debug.LogError("GameMenu introuvable via GameTime.");
    }

    void Update()
    {
        if (_isGameRunning)
        {
            _timer -= Time.deltaTime;
            
            // Only update UI if the second has changed
            int currentSecond = Mathf.CeilToInt(_timer);
            if (currentSecond != _lastSecond)
            {
                int minutes = Mathf.FloorToInt(_timer / 60);
                int seconds = Mathf.FloorToInt(_timer % 60);
                
                if (timerText != null)
                {
                    timerText.UpdateText(string.Format("{0}:{1:00}", minutes, seconds), false);
                }
                _lastSecond = currentSecond;
            }
            
            if (_timer <= 0f)
            {
                timerText.UpdateText("0:00", false);
                EndGame();
            }
        }
    }
    
    private void EndGame()
    {
        _isGameRunning = false;
        _timer = 0f;
        
        // Disable Player Movement
        if (_playerMovement != null)
        {
            _playerMovement.enabled = false;
        }
        
        // Open Pause Menu
        if (_gameMenu != null)
        {
            _gameMenu.OpenScoreMenu();
        }
    }

    private void OnEnable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        if (scene.name == "GameScene")
        {
            _timer = maxTime;
            _isGameRunning = true;
            _lastSecond = -1;
            
            _playerMovement = FindFirstObjectByType<PlayerMovement>();
            _gameMenu = FindFirstObjectByType<GameMenu>();
            
            if (timerText != null) timerText.UpdateText(maxTime.ToString("F2"), false); // Reset text or handle better
        }
    }
}
