using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Manages combat between player and enemies
/// </summary>
public class CombatManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameManager gameManager;
    [SerializeField] private DungeonManager dungeonManager;
    [SerializeField] private Transform combatAreaTransform;

    [Header("UI")]
    [SerializeField] private GameObject combatUI;
    [SerializeField] private TextMeshProUGUI combatLogText;
    [SerializeField] private GameObject damagePopupPrefab;

    [Header("Dice")]
    [SerializeField] private GameObject dicePrefab;
    [SerializeField] private Transform diceSpawnPoint;
    [SerializeField] private float diceRollDuration = 1.5f;
    [SerializeField] private float resultDisplayDuration = 2.0f;

    [Header("Visual Effects")]
    [SerializeField] private GameObject hitEffectPrefab;
    [SerializeField] private GameObject criticalHitEffectPrefab;
    [SerializeField] private GameObject missEffectPrefab;
    [SerializeField] private GameObject healEffectPrefab;

    private PlayerController player;
    private EnemyController currentEnemy;
    private System.Random rng;
    private bool inCombat = false;

    private void Awake()
    {
        rng = new System.Random();

        // Find references if not set
        if (gameManager == null)
            gameManager = FindObjectOfType<GameManager>();

        if (dungeonManager == null)
            dungeonManager = FindObjectOfType<DungeonManager>();
    }

    /// <summary>
    /// Start combat with a specific enemy
    /// </summary>
    public void InitiateCombat(PlayerController player, EnemyController enemy)
    {
        this.player = player;
        currentEnemy = enemy;
        inCombat = true;

        // Enable combat UI
        SetCombatUIActive(true);

        // Show combat start animation
        StartCoroutine(CombatStartSequence());
    }

    /// <summary>
    /// Start combat using a combat card
    /// </summary>
    public void InitiateCombat(CombatCardData combatCard, Vector2Int targetPosition)
    {
        // Find the enemy at the target position
        EnemyController enemy = dungeonManager.GetEnemyAtPosition(targetPosition);
        if (enemy == null)
        {
            Debug.LogWarning("No enemy found at target position");
            return;
        }

        // Start a single attack without entering full combat mode
        StartCoroutine(CardAttackSequence(combatCard, enemy));
    }

    /// <summary>
    /// Show the combat UI
    /// </summary>
    public void SetCombatUIActive(bool active)
    {
        if (combatUI != null)
        {
            combatUI.SetActive(active);
        }
    }

    /// <summary>
    /// Animation sequence for starting combat
    /// </summary>
    private IEnumerator CombatStartSequence()
    {
        // Update combat log
        UpdateCombatLog($"Combat started with {currentEnemy.EnemyName}!");

        // Camera shake to indicate combat start
        Camera.main.transform.DOShakePosition(0.5f, 0.3f, 10, 90);

        // Wait a moment
        yield return new WaitForSeconds(1.0f);

        // Start the combat turns
        StartCoroutine(CombatLoop());
    }

    /// <summary>
    /// Main combat loop
    /// </summary>
    private IEnumerator CombatLoop()
    {
        // Continue until combat ends
        while (inCombat)
        {
            // Player's turn
            yield return StartCoroutine(PlayerTurn());

            // Check if combat should end
            if (!inCombat) break;

            // Enemy's turn
            yield return StartCoroutine(EnemyTurn());

            // Check if combat should end
            if (!inCombat) break;

            // Short pause between rounds
            yield return new WaitForSeconds(1.0f);
        }
    }

    /// <summary>
    /// Player's turn in combat
    /// </summary>
    private IEnumerator PlayerTurn()
    {
        UpdateCombatLog("Your turn! Select a card to attack.");

        // TODO: Wait for player to select a combat card
        // This will be handled through the UI

        // For now, simulate player attack
        yield return new WaitForSeconds(1.0f);

        // Roll dice for attack
        int rollResult = yield return StartCoroutine(RollDiceCoroutine(6));

        // Calculate damage
        int damage = 5 + rollResult; // Basic damage for testing

        // Apply damage to enemy
        yield return StartCoroutine(ApplyDamageToEnemy(damage));

        // Check if enemy is defeated
        if (currentEnemy.CurrentHealth <= 0)
        {
            yield return StartCoroutine(EnemyDefeatedSequence());
            inCombat = false;
        }
    }

    /// <summary>
    /// Enemy's turn in combat
    /// </summary>
    private IEnumerator EnemyTurn()
    {
        if (!currentEnemy.CanAttack())
        {
            UpdateCombatLog($"{currentEnemy.EnemyName} is unable to attack.");
            yield break;
        }

        UpdateCombatLog($"{currentEnemy.EnemyName} prepares to attack!");
        yield return new WaitForSeconds(1.0f);

        // Enemy rolls dice
        int enemyRoll = rng.Next(1, 7);
        UpdateCombatLog($"{currentEnemy.EnemyName} rolled: {enemyRoll}");

        // Calculate enemy damage
        int enemyDamage = currentEnemy.BaseDamage + enemyRoll - player.Defense;
        enemyDamage = Mathf.Max(1, enemyDamage); // Minimum 1 damage

        // Apply damage to player
        UpdateCombatLog($"{currentEnemy.EnemyName} deals {enemyDamage} damage!");
        player.TakeDamage(enemyDamage);

        // Show damage effect on player
        GameObject hitEffect = Instantiate(
            hitEffectPrefab,
            player.transform.position,
            Quaternion.identity
        );

        // Camera shake effect
        Camera.main.transform.DOShakePosition(0.3f, 0.2f, 10, 90);

        // Cleanup hit effect
        Destroy(hitEffect, 1.0f);

        // Check if player is defeated
        if (player.GetCurrentHealth() <= 0)
        {
            UpdateCombatLog("You have been defeated!");
            gameManager.OnPlayerDeath();
            inCombat = false;
        }

        yield return new WaitForSeconds(1.0f);
    }

    /// <summary>
    /// Sequence for using a combat card against an enemy
    /// </summary>
    private IEnumerator CardAttackSequence(CombatCardData combatCard, EnemyController enemy)
    {
        // Update combat log
        UpdateCombatLog($"Using {combatCard.cardName} against {enemy.EnemyName}");
        yield return new WaitForSeconds(0.5f);

        // Roll dice with visual effect
        int rollResult = yield return StartCoroutine(RollDiceCoroutine(combatCard.diceSides.Length));

        // Apply modifiers
        int modifiedRoll = ApplyDiceModifiers(rollResult, combatCard.modifiers);

        // Calculate final damage
        int finalDamage = CalculateDamage(combatCard.baseDamage, modifiedRoll, enemy);

        // Apply damage to enemy
        yield return StartCoroutine(ApplyDamageToEnemy(finalDamage, enemy));

        // Check for enemy defeat
        if (enemy.CurrentHealth <= 0)
        {
            yield return StartCoroutine(EnemyDefeatedSequence(enemy));
        }
    }

    /// <summary>
    /// Roll dice with visual effect
    /// </summary>
    private IEnumerator RollDiceCoroutine(int diceSides)
    {
        // Instantiate 3D dice
        GameObject diceObj = Instantiate(dicePrefab, diceSpawnPoint.position, Quaternion.identity);
        DiceController dice = diceObj.GetComponent<DiceController>();

        if (dice != null)
        {
            // Set up dice properties
            dice.SetDiceSides(diceSides);

            // Animate dice roll
            dice.Roll(rollResult =>
            {
                // This callback will be executed when roll animation completes
                UpdateCombatLog($"Rolled: {rollResult}");
            });

            // Wait for animation to complete
            yield return new WaitForSeconds(diceRollDuration);

            // Get roll result
            int rollResult = dice.GetRollResult();

            // Cleanup dice object after showing result
            yield return new WaitForSeconds(0.5f);
            Destroy(diceObj);

            return rollResult;
        }
        else
        {
            // Fallback if dice controller not found
            int rollResult = rng.Next(1, diceSides + 1);
            UpdateCombatLog($"Rolled: {rollResult}");
            yield return new WaitForSeconds(1.0f);
            return rollResult;
        }
    }

    /// <summary>
    /// Apply dice modifiers from combat cards
    /// </summary>
    private int ApplyDiceModifiers(int rollResult, CombatCardData.DiceModifier[] modifiers)
    {
        int modifiedRoll = rollResult;
        string modifierLog = "";

        if (modifiers != null && modifiers.Length > 0)
        {
            foreach (var modifier in modifiers)
            {
                switch (modifier.type)
                {
                    case CombatCardData.DiceModifier.ModifierType.AddToRoll:
                        modifiedRoll += modifier.value;
                        modifierLog += $" +{modifier.value}";
                        break;

                    case CombatCardData.DiceModifier.ModifierType.MultiplyRoll:
                        modifiedRoll *= modifier.value;
                        modifierLog += $" ×{modifier.value}";
                        break;

                    case CombatCardData.DiceModifier.ModifierType.ReRoll:
                        if (rollResult <= modifier.value)
                        {
                            // Re-roll if result is too low
                            int newRoll = rng.Next(1, 7); // Assuming 6-sided die
                            modifierLog += $" (Re-rolled {rollResult} → {newRoll})";
                            modifiedRoll = newRoll;
                        }
                        break;

                    case CombatCardData.DiceModifier.ModifierType.MinimumRoll:
                        if (modifiedRoll < modifier.value)
                        {
                            modifierLog += $" (Minimum {modifier.value})";
                            modifiedRoll = modifier.value;
                        }
                        break;
                }
            }
        }

        if (!string.IsNullOrEmpty(modifierLog))
        {
            UpdateCombatLog($"Applied modifiers: {rollResult}{modifierLog} = {modifiedRoll}");
        }

        return modifiedRoll;
    }

    /// <summary>
    /// Calculate damage based on roll and modifiers
    /// </summary>
    private int CalculateDamage(int baseDamage, int diceResult, EnemyController enemy)
    {
        // Basic damage calculation
        int damage = baseDamage + diceResult;

        // Check for critical hit (natural max roll)
        bool isCritical = diceResult == 6; // Assuming 6-sided die

        if (isCritical)
        {
            damage *= 2;
            UpdateCombatLog("Critical hit! Double damage!");
        }

        // Apply enemy defense
        damage -= enemy.Defense;
        damage = Mathf.Max(1, damage); // Minimum 1 damage

        // Apply any player bonuses
        float damageMultiplier = player.GetDamageMultiplier();
        if (damageMultiplier != 1.0f)
        {
            damage = Mathf.RoundToInt(damage * damageMultiplier);
        }

        UpdateCombatLog($"Calculated final damage: {damage}");
        return damage;
    }

    /// <summary>
    /// Apply damage to an enemy with visual effects
    /// </summary>
    private IEnumerator ApplyDamageToEnemy(int damage, EnemyController enemy = null)
    {
        // Use current enemy if not specified
        if (enemy == null)
        {
            enemy = currentEnemy;
        }

        // Apply the damage
        enemy.TakeDamage(damage);

        // Show damage popup
        ShowDamagePopup(enemy.transform.position, damage);

        // Show hit effect
        GameObject hitEffect = Instantiate(
            damage > 10 ? criticalHitEffectPrefab : hitEffectPrefab,
            enemy.transform.position,
            Quaternion.identity
        );

        // Camera shake
        Camera.main.transform.DOShakePosition(0.3f, 0.2f, 10, 90);

        // Cleanup hit effect
        Destroy(hitEffect, 1.0f);

        yield return new WaitForSeconds(0.5f);
    }

    /// <summary>
    /// Sequence for enemy defeat
    /// </summary>
    private IEnumerator EnemyDefeatedSequence(EnemyController enemy = null)
    {
        // Use current enemy if not specified
        if (enemy == null)
        {
            enemy = currentEnemy;
        }

        UpdateCombatLog($"{enemy.EnemyName} defeated!");

        // Award experience/loot
        int expGained = enemy.ExperienceReward;
        player.AddExperience(expGained);
        UpdateCombatLog($"Gained {expGained} experience!");

        // Play defeat animation
        enemy.PlayDeathAnimation();

        yield return new WaitForSeconds(1.0f);

        // Handle enemy loot drops
        if (enemy.HasLoot())
        {
            Item loot = enemy.DropLoot();
            UpdateCombatLog($"Enemy dropped: {loot.itemName}");
            player.AddToInventory(loot);
        }

        // End combat if this was the main enemy
        if (enemy == currentEnemy)
        {
            EndCombat(true);
        }
    }

    /// <summary>
    /// Show a damage popup
    /// </summary>
    private void ShowDamagePopup(Vector3 position, int damage)
    {
        GameObject popupObj = Instantiate(damagePopupPrefab, position + Vector3.up * 0.5f, Quaternion.identity);
        TextMeshPro popupText = popupObj.GetComponent<TextMeshPro>();

        if (popupText != null)
        {
            popupText.text = damage.ToString();

            // Larger text for bigger damage
            float scaleFactor = Mathf.Clamp(1f + (damage / 20f), 1f, 2f);
            popupObj.transform.localScale *= scaleFactor;

            // Animation sequence
            popupObj.transform.DOMove(position + Vector3.up * 1.5f, 1f).SetEase(Ease.OutCubic);
            popupObj.transform.DOScale(Vector3.zero, 0.5f).SetDelay(0.7f).OnComplete(() =>
            {
                Destroy(popupObj);
            });
        }
        else
        {
            Destroy(popupObj);
        }
    }

    /// <summary>
    /// End combat and return to exploration
    /// </summary>
    public void EndCombat(bool playerVictory)
    {
        inCombat = false;

        // Hide combat UI
        SetCombatUIActive(false);

        // Return to exploration mode
        gameManager.SetGameState(GameManager.GameState.Exploring);

        // Clear references
        currentEnemy = null;
    }

    /// <summary>
    /// Update the combat log text
    /// </summary>
    private void UpdateCombatLog(string message)
    {
        if (combatLogText != null)
        {
            combatLogText.text = message;
            Debug.Log($"Combat Log: {message}");
        }
    }
}