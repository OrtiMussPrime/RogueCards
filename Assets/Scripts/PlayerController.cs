using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

/// <summary>
/// Controls the player character in the dungeon
/// </summary>
public class PlayerController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private DungeonManager dungeonManager;
    [SerializeField] private GameManager gameManager;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float moveTime = 0.3f;
    [SerializeField] private Ease moveEase = Ease.OutQuint;

    [Header("Stats")]
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int currentHealth;
    [SerializeField] private int defense = 1;
    [SerializeField] private float damageMultiplier = 1.0f;

    [Header("Effects")]
    [SerializeField] private GameObject moveEffectPrefab;
    [SerializeField] private GameObject damageEffectPrefab;
    [SerializeField] private GameObject healEffectPrefab;

    private Vector2Int currentGridPosition;
    private bool isMoving = false;

    private void Start()
    {
        // Find references if not set
        if (dungeonManager == null)
            dungeonManager = FindObjectOfType<DungeonManager>();

        if (gameManager == null)
            gameManager = FindObjectOfType<GameManager>();

        // Initialize health
        currentHealth = maxHealth;
    }

    private void Update()
    {
        // Only handle input in exploration mode
        if (gameManager.GetCurrentState() != GameManager.GameState.Exploring)
            return;

        // Don't process input if already moving
        if (isMoving)
            return;

        // Check for adjacent cells to move to
        HandleMovementInput();
    }

    /// <summary>
    /// Handle movement input from keyboard
    /// </summary>
    private void HandleMovementInput()
    {
        Vector2Int direction = Vector2Int.zero;

        // WASD or arrow key input
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
            direction = Vector2Int.up;
        else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
            direction = Vector2Int.down;
        else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
            direction = Vector2Int.left;
        else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
            direction = Vector2Int.right;

        // Try to move in the desired direction
        if (direction != Vector2Int.zero)
        {
            TryMove(direction);
        }
    }

    /// <summary>
    /// Attempt to move in the specified direction
    /// </summary>
    private void TryMove(Vector2Int direction)
    {
        Vector2Int targetPosition = currentGridPosition + direction;

        // Check if we can move to the target position
        if (dungeonManager.CanMoveTo(targetPosition))
        {
            // Start movement
            StartCoroutine(MoveToCell(targetPosition));
        }
        else
        {
            // Play "can't move" feedback
            transform.DOShakePosition(0.2f, 0.1f, 10, 90);
        }
    }

    /// <summary>
    /// Smoothly move to a new grid cell
    /// </summary>
    private IEnumerator MoveToCell(Vector2Int targetPosition)
    {
        isMoving = true;

        // Get world position
        Vector3 worldPosition = dungeonManager.GridToWorldPosition(targetPosition);

        // Create movement effect
        if (moveEffectPrefab != null)
        {
            Instantiate(moveEffectPrefab, transform.position, Quaternion.identity);
        }

        // Move smoothly using DOTween
        transform.DOMove(worldPosition, moveTime)
            .SetEase(moveEase);

        // Wait for movement to complete
        yield return new WaitForSeconds(moveTime);

        // Update grid position
        currentGridPosition = targetPosition;

        // Apply any cell effects
        dungeonManager.ApplyThemeEffects(this, currentGridPosition);

        // Check for special cells
        CheckForSpecialCells();

        // Player is no longer moving
        isMoving = false;
    }

    /// <summary>
    /// Check for special interactions at the current position
    /// </summary>
    private void CheckForSpecialCells()
    {
        GridCell.CellState cellState = dungeonManager.GetCellState(currentGridPosition);

        switch (cellState)
        {
            case GridCell.CellState.Exit:
                // Reached the exit
                gameManager.OnExitReached();
                break;

            case GridCell.CellState.Enemy:
                // Encounter an enemy
                StartCombat();
                break;

            case GridCell.CellState.Trap:
                // Triggered a trap
                TriggerTrap();
                break;

            case GridCell.CellState.Item:
                // Found an item
                CollectItem();
                break;
        }
    }

    /// <summary>
    /// Start combat with an enemy at the current position
    /// </summary>
    private void StartCombat()
    {
        // Get the enemy at this position
        EnemyController enemy = dungeonManager.GetEnemyAtPosition(currentGridPosition);
        if (enemy != null)
        {
            // Transition to combat mode
            gameManager.SetGameState(GameManager.GameState.Combat);

            // Setup combat
            GameManager.Instance.combatManager.InitiateCombat(this, enemy);
        }
    }

    /// <summary>
    /// Trigger a trap effect
    /// </summary>
    private void TriggerTrap()
    {
        // Get the trap at this position
        TrapController trap = dungeonManager.GetTrapAtPosition(currentGridPosition);
        if (trap != null)
        {
            // Trigger the trap effect
            trap.Trigger(this);

            // Visual feedback
            transform.DOShakePosition(0.3f, 0.2f, 10, 90);

            // Play trap effect
            if (damageEffectPrefab != null)
            {
                Instantiate(damageEffectPrefab, transform.position, Quaternion.identity);
            }
        }
    }

    /// <summary>
    /// Collect an item at the current position
    /// </summary>
    private void CollectItem()
    {
        // Get the item at this position
        ItemController item = dungeonManager.GetItemAtPosition(currentGridPosition);
        if (item != null)
        {
            // Collect the item
            item.Collect(this);

            // Visual feedback
            transform.DOPunchScale(Vector3.one * 0.2f, 0.3f, 10, 1);

            // Play collect effect
            if (healEffectPrefab != null)
            {
                Instantiate(healEffectPrefab, transform.position, Quaternion.identity);
            }
        }
    }

    /// <summary>
    /// Set player position (used when initializing or teleporting)
    /// </summary>
    public void SetPosition(Vector2Int gridPosition)
    {
        currentGridPosition = gridPosition;
        transform.position = dungeonManager.GridToWorldPosition(gridPosition);
    }

    /// <summary>
    /// Apply ice effect (slippery movement)
    /// </summary>
    public void ApplyIceEffect(float intensity)
    {
        // Implementation could make player slide an extra space
        // or reduce control for a time

        // Visual feedback
        transform.DOShakePosition(0.2f, 0.1f * intensity, 10, 90);
    }

    /// <summary>
    /// Apply damage over time (e.g., for fire/poison effects)
    /// </summary>
    public void TakeDamageOverTime(int damage)
    {
        // Take damage
        TakeDamage(damage);

        // Visual feedback
        if (damageEffectPrefab != null)
        {
            Instantiate(damageEffectPrefab, transform.position, Quaternion.identity);
        }
    }

    /// <summary>
    /// Set visibility range (e.g., for dark areas)
    /// </summary>
    public void SetVisibilityRange(float range)
    {
        // Implementation depends on how fog of war is handled
        // Could update a shader parameter or adjust a light radius
    }

    /// <summary>
    /// Take damage from an attack
    /// </summary>
    public void TakeDamage(int damage)
    {
        // Apply defense
        int actualDamage = Mathf.Max(1, damage - defense);

        // Apply damage
        currentHealth -= actualDamage;

        // Visual feedback
        transform.DOShakePosition(0.3f, 0.2f, 10, 90);

        // Check for death
        if (currentHealth <= 0)
        {
            Die();
        }

        // Update UI
        // Implementation depends on UI setup
    }

    /// <summary>
    /// Heal the player
    /// </summary>
    public void Heal(int amount)
    {
        // Apply healing
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);

        // Visual feedback
        transform.DOPunchScale(Vector3.one * 0.2f, 0.3f, 10, 1);

        // Play healing effect
        if (healEffectPrefab != null)
        {
            Instantiate(healEffectPrefab, transform.position, Quaternion.identity);
        }

        // Update UI
        // Implementation depends on UI setup
    }

    /// <summary>
    /// Handle player death
    /// </summary>
    private void Die()
    {
        // Visual feedback
        transform.DOShakePosition(0.5f, 0.3f, 10, 90);

        // Notify game manager
        gameManager.OnPlayerDeath();
    }

    /// <summary>
    /// Get the player's damage multiplier
    /// </summary>
    public float GetDamageMultiplier()
    {
        return damageMultiplier;
    }

    /// <summary>
    /// Add experience points
    /// </summary>
    public void AddExperience(int expAmount)
    {
        // Implementation depends on progression system
    }

    /// <summary>
    /// Add an item to player inventory
    /// </summary>
    public void AddToInventory(Item item)
    {
        // Implementation depends on inventory system
    }

    /// <summary>
    /// Get the current health
    /// </summary>
    public int GetCurrentHealth()
    {
        return currentHealth;
    }

    /// <summary>
    /// Get the maximum health
    /// </summary>
    public int GetMaxHealth()
    {
        return maxHealth;
    }

    /// <summary>
    /// Get the defense value
    /// </summary>
    public int Defense
    {
        get { return defense; }
    }
}