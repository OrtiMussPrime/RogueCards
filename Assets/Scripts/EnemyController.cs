using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

/// <summary>
/// Controls an enemy in the dungeon
/// </summary>
public class EnemyController : MonoBehaviour
{
    [Header("Basic Info")]
    [SerializeField] private string enemyName = "Enemy";
    [SerializeField] private bool isBoss = false;
    [SerializeField] private Vector2Int gridPosition;

    [Header("Stats")]
    [SerializeField] private int maxHealth = 20;
    [SerializeField] private int currentHealth;
    [SerializeField] private int baseDamage = 5;
    [SerializeField] private int defense = 0;
    [SerializeField] private int experienceReward = 10;

    [Header("Animation")]
    [SerializeField] private Transform visualTransform;
    [SerializeField] private float idleAmplitude = 0.1f;
    [SerializeField] private float idleFrequency = 1f;

    [Header("Effects")]
    [SerializeField] private GameObject damageEffectPrefab;
    [SerializeField] private GameObject defeatEffectPrefab;

    private Vector3 startPosition;
    private bool canAttack = true;

    /// <summary>
    /// Properties for external access
    /// </summary>
    public string EnemyName => enemyName;
    public bool IsBoss => isBoss;
    public Vector2Int GridPosition => gridPosition;
    public int BaseDamage => baseDamage;
    public int Defense => defense;
    public int ExperienceReward => experienceReward;
    public int CurrentHealth => currentHealth;

    private void Start()
    {
        // Initialize health
        currentHealth = maxHealth;

        // Reference for animations
        if (visualTransform == null)
        {
            visualTransform = transform;
        }

        startPosition = visualTransform.localPosition;

        // Start idle animation
        StartIdleAnimation();
    }

    /// <summary>
    /// Setup the enemy with specific position and properties
    /// </summary>
    public void Initialize(Vector2Int position, EnemyData data = null)
    {
        gridPosition = position;

        // Apply data if provided
        if (data != null)
        {
            enemyName = data.enemyName;
            isBoss = data.isBoss;
            maxHealth = data.maxHealth;
            currentHealth = maxHealth;
            baseDamage = data.baseDamage;
            defense = data.defense;
            experienceReward = data.experienceReward;
        }

        // Apply boss scaling if needed
        if (isBoss)
        {
            transform.localScale = Vector3.one * 1.5f;
        }
    }

    /// <summary>
    /// Create an idle animation
    /// </summary>
    private void StartIdleAnimation()
    {
        // Create a sequence for the idle animation
        Sequence idleSequence = DOTween.Sequence();

        // Subtle bobbing up and down
        idleSequence.Append(visualTransform.DOLocalMoveY(
            startPosition.y + idleAmplitude,
            idleFrequency).SetEase(Ease.InOutSine));

        idleSequence.Append(visualTransform.DOLocalMoveY(
            startPosition.y - idleAmplitude,
            idleFrequency).SetEase(Ease.InOutSine));

        // Loop forever
        idleSequence.SetLoops(-1, LoopType.Restart);
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
        visualTransform.DOShakePosition(0.3f, 0.2f, 10, 90);

        // Spawn damage effect
        if (damageEffectPrefab != null)
        {
            Instantiate(damageEffectPrefab, transform.position, Quaternion.identity);
        }

        // Check for defeat
        if (currentHealth <= 0)
        {
            Defeated();
        }
    }

    /// <summary>
    /// Enemy is defeated
    /// </summary>
    private void Defeated()
    {
        // Spawn defeat effect
        if (defeatEffectPrefab != null)
        {
            Instantiate(defeatEffectPrefab, transform.position, Quaternion.identity);
        }

        // Notify game manager
        GameManager.Instance.OnEnemyDefeated(this);

        // Play defeat animation
        PlayDeathAnimation();
    }

    /// <summary>
    /// Play the death animation
    /// </summary>
    public void PlayDeathAnimation()
    {
        // Stop idle animation
        DOTween.Kill(visualTransform);

        // Death animation sequence
        Sequence deathSequence = DOTween.Sequence();

        // Shake and fade out
        deathSequence.Append(visualTransform.DOShakePosition(0.5f, 0.3f, 10, 90));
        deathSequence.Join(visualTransform.DOScale(Vector3.zero, 0.5f).SetEase(Ease.InBack));

        // Destroy after animation
        deathSequence.OnComplete(() =>
        {
            Destroy(gameObject);
        });
    }

    /// <summary>
    /// Check if the enemy can attack
    /// </summary>
    public bool CanAttack()
    {
        return canAttack;
    }

    /// <summary>
    /// Check if the enemy has loot to drop
    /// </summary>
    public bool HasLoot()
    {
        // Implementation depends on loot system
        return isBoss || Random.Range(0, 100) < 30; // 30% chance for normal enemies
    }

    /// <summary>
    /// Drop loot when defeated
    /// </summary>
    public Item DropLoot()
    {
        // Implementation depends on loot system
        // Create a temporary Item for now
        return new Item
        {
            itemName = isBoss ? "Rare Item" : "Common Item",
            // Additional properties would be set here
        };
    }
}

/// <summary>
/// Temporary class for Item
/// </summary>
[System.Serializable]
public class Item
{
    public string itemName;
    // Additional properties would be defined here
}

/// <summary>
/// Data for enemy configuration
/// </summary>
[System.Serializable]
public class EnemyData
{
    public string enemyName = "Enemy";
    public bool isBoss = false;
    public int maxHealth = 20;
    public int baseDamage = 5;
    public int defense = 0;
    public int experienceReward = 10;
    public Sprite enemySprite;
}