using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

/// <summary>
/// Core game manager that coordinates all gameplay systems
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Systems")]
    public DungeonManager dungeonManager;
    public CombatManager combatManager;
    public PlayerController playerController;
    public GameplayCardController cardController;
    public CardFactory cardFactory;
    public ProgressionManager progressionManager;

    [Header("Game State")]
    [SerializeField] private GameState currentState;
    [SerializeField] private int currentFloor = 1;
    [SerializeField] private int enemiesDefeated = 0;
    [SerializeField] private int bossesDefeated = 0;
    [SerializeField] private float runStartTime;

    public enum GameState
    {
        MainMenu,
        Exploring,
        Combat,
        CardSelection,
        GameOver,
        Victory
    }

    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Find components if not set
        if (dungeonManager == null)
            dungeonManager = FindObjectOfType<DungeonManager>();

        if (combatManager == null)
            combatManager = FindObjectOfType<CombatManager>();

        if (playerController == null)
            playerController = FindObjectOfType<PlayerController>();

        if (cardController == null)
            cardController = FindObjectOfType<GameplayCardController>();

        if (cardFactory == null)
            cardFactory = FindObjectOfType<CardFactory>();

        if (progressionManager == null)
            progressionManager = FindObjectOfType<ProgressionManager>();
    }

    private void Start()
    {
        StartNewGame();
    }

    /// <summary>
    /// Start a new game run
    /// </summary>
    public void StartNewGame()
    {
        // Reset game state
        currentFloor = 1;
        enemiesDefeated = 0;
        bossesDefeated = 0;
        runStartTime = Time.time;

        // Initialize dungeon
        int seed = System.DateTime.Now.Millisecond;
        dungeonManager.Initialize(seed);

        // Deal initial cards
        cardFactory.DealInitialHand();

        // Place player at entrance
        if (playerController != null)
        {
            playerController.SetPosition(dungeonManager.GetEntrancePosition());
        }

        // Set game state
        SetGameState(GameState.Exploring);
    }

    /// <summary>
    /// Transition to a new game state
    /// </summary>
    public void SetGameState(GameState newState)
    {
        GameState oldState = currentState;
        currentState = newState;

        // Handle state transitions
        switch (newState)
        {
            case GameState.Exploring:
                EnableExplorationMode();
                break;

            case GameState.Combat:
                EnableCombatMode();
                break;

            case GameState.CardSelection:
                EnableCardSelectionMode();
                break;

            case GameState.GameOver:
                HandleGameOver(false);
                break;

            case GameState.Victory:
                HandleGameOver(true);
                break;
        }

        // Trigger state change events
        OnGameStateChanged(oldState, newState);
    }

    /// <summary>
    /// Enable exploration mode
    /// </summary>
    private void EnableExplorationMode()
    {
        // Enable player movement
        if (playerController != null)
        {
            playerController.enabled = true;
        }

        // Enable card dragging
        if (cardController != null)
        {
            cardController.enabled = true;
        }

        // Disable combat UI if any
        if (combatManager != null)
        {
            combatManager.SetCombatUIActive(false);
        }
    }

    /// <summary>
    /// Enable combat mode
    /// </summary>
    private void EnableCombatMode()
    {
        // Disable player movement
        if (playerController != null)
        {
            playerController.enabled = false;
        }

        // Disable card dragging for exploration cards
        // (Combat cards will still be draggable)
        if (cardController != null)
        {
            // Only allow combat cards
            // Implementation depends on how cards are filtered
        }

        // Enable combat UI
        if (combatManager != null)
        {
            combatManager.SetCombatUIActive(true);
        }
    }

    /// <summary>
    /// Enable card selection mode (between floors)
    /// </summary>
    private void EnableCardSelectionMode()
    {
        // Disable player movement
        if (playerController != null)
        {
            playerController.enabled = false;
        }

        // Show card selection UI
        // (Implementation depends on UI setup)
    }

    /// <summary>
    /// Handle end of game (win or lose)
    /// </summary>
    private void HandleGameOver(bool victory)
    {
        // Calculate run time
        float runTime = Time.time - runStartTime;

        // Create run results
        if (progressionManager != null)
        {
            ProgressionManager.RunResultsData results = new ProgressionManager.RunResultsData
            {
                floorReached = currentFloor,
                enemiesDefeated = enemiesDefeated,
                bossesDefeated = bossesDefeated,
                wasSuccessful = victory,
                runTime = runTime
            };

            // Process run results for meta-progression
            progressionManager.ProcessRunResults(results);
        }

        // Show game over UI
        // (Implementation depends on UI setup)
    }

    /// <summary>
    /// Called when player defeats an enemy
    /// </summary>
    public void OnEnemyDefeated(EnemyController enemy)
    {
        enemiesDefeated++;

        if (enemy.IsBoss)
        {
            bossesDefeated++;
        }

        // Update UI
        // (Implementation depends on UI setup)
    }

    /// <summary>
    /// Called when player reaches the exit
    /// </summary>
    public void OnExitReached()
    {
        // Advance to next floor
        currentFloor++;

        // Check for victory condition
        if (currentFloor > 10) // Arbitrary number, adjust as needed
        {
            SetGameState(GameState.Victory);
            return;
        }

        // Show card selection screen before next floor
        SetGameState(GameState.CardSelection);

        // Generate new floor after selection
        // This will be triggered after card selection
    }

    /// <summary>
    /// Advance to the next floor after card selection
    /// </summary>
    public void AdvanceToNextFloor()
    {
        // Generate new dungeon floor
        int seed = System.DateTime.Now.Millisecond + currentFloor;
        dungeonManager.Initialize(seed);

        // Place player at entrance
        if (playerController != null)
        {
            playerController.SetPosition(dungeonManager.GetEntrancePosition());
        }

        // Return to exploration mode
        SetGameState(GameState.Exploring);
    }

    /// <summary>
    /// Called when player health reaches zero
    /// </summary>
    public void OnPlayerDeath()
    {
        SetGameState(GameState.GameOver);
    }

    /// <summary>
    /// Event triggered when game state changes
    /// </summary>
    private void OnGameStateChanged(GameState oldState, GameState newState)
    {
        // You could add event system here if needed
        Debug.Log($"Game state changed from {oldState} to {newState}");

        // Camera effects on state change
        Camera.main.transform.DOShakePosition(0.5f, 0.2f, 10, 90);
    }

    /// <summary>
    /// Pause/unpause the game
    /// </summary>
    public void TogglePause()
    {
        // Implementation depends on pause system
    }

    /// <summary>
    /// Game Over - return to main menu
    /// </summary>
    public void ReturnToMainMenu()
    {
        // Implementation depends on scene management
    }
}
