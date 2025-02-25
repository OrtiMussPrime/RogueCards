using UnityEngine;

/// <summary>
/// Base class for all card data
/// </summary>
[CreateAssetMenu(fileName = "New Card", menuName = "RogueCards/Card")]
public class CardData : ScriptableObject
{
    [Header("Basic Info")]
    public string cardName;
    public string description;
    public Sprite cardArt;
    public CardType cardType;
    public Rarity rarity;

    [Header("Card Properties")]
    public int manaCost = 1;

    public enum CardType
    {
        Path,
        Combat,
        Special
    }

    public enum Rarity
    {
        Common,
        Uncommon,
        Rare,
        Epic,
        Legendary
    }

    /// <summary>
    /// Called when the card is played on a grid position
    /// </summary>
    public virtual void OnCardPlayed(Vector2Int gridPosition, GameManager gameManager)
    {
        Debug.Log($"Card {cardName} played at position {gridPosition}");
    }
}

/// <summary>
/// Card that creates paths in the dungeon
/// </summary>
[CreateAssetMenu(fileName = "New Path Card", menuName = "RogueCards/Path Card")]
public class PathCardData : CardData
{
    [Header("Path Properties")]
    public PathDirection[] availableDirections;

    public enum PathDirection
    {
        North,
        East,
        South,
        West
    }

    public override void OnCardPlayed(Vector2Int gridPosition, GameManager gameManager)
    {
        base.OnCardPlayed(gridPosition, gameManager);

        // Create paths based on available directions
        gameManager.dungeonManager.CreatePaths(gridPosition, availableDirections);
    }
}

/// <summary>
/// Card used in combat
/// </summary>
[CreateAssetMenu(fileName = "New Combat Card", menuName = "RogueCards/Combat Card")]
public class CombatCardData : CardData
{
    [Header("Combat Properties")]
    public int baseDamage;
    public int[] diceSides = { 1, 2, 3, 4, 5, 6 }; // Default 6-sided die
    public DiceModifier[] modifiers;

    [System.Serializable]
    public class DiceModifier
    {
        public ModifierType type;
        public int value;

        public enum ModifierType
        {
            AddToRoll,
            MultiplyRoll,
            ReRoll,
            MinimumRoll
        }
    }

    public override void OnCardPlayed(Vector2Int gridPosition, GameManager gameManager)
    {
        base.OnCardPlayed(gridPosition, gameManager);

        // Apply combat effects
        gameManager.combatManager.InitiateCombat(this, gridPosition);
    }
}

/// <summary>
/// Card with special effects
/// </summary>
[CreateAssetMenu(fileName = "New Special Card", menuName = "RogueCards/Special Card")]
public class SpecialCardData : CardData
{
    [Header("Special Properties")]
    public SpecialEffect effectType;
    public float effectPower;
    public int effectDuration;

    public enum SpecialEffect
    {
        Teleport,
        Stasis,
        Invisibility,
        RevealMap,
        HealPlayer,
        DamageAllEnemies
    }

    public override void OnCardPlayed(Vector2Int gridPosition, GameManager gameManager)
    {
        base.OnCardPlayed(gridPosition, gameManager);

        // Apply special effect based on type
        switch (effectType)
        {
            case SpecialEffect.Teleport:
                TeleportPlayer(gridPosition, gameManager);
                break;

            case SpecialEffect.HealPlayer:
                HealPlayer(gameManager);
                break;

            case SpecialEffect.RevealMap:
                RevealMap(gameManager);
                break;

            case SpecialEffect.DamageAllEnemies:
                DamageAllEnemies(gameManager);
                break;

                // Additional effects can be implemented here
        }
    }

    /// <summary>
    /// Teleport the player to the target position
    /// </summary>
    private void TeleportPlayer(Vector2Int gridPosition, GameManager gameManager)
    {
        if (gameManager.playerController != null)
        {
            gameManager.playerController.SetPosition(gridPosition);

            // Visual effect
            Camera.main.transform.DOShakePosition(0.3f, 0.2f, 10, 90);
        }
    }

    /// <summary>
    /// Heal the player
    /// </summary>
    private void HealPlayer(GameManager gameManager)
    {
        if (gameManager.playerController != null)
        {
            int healAmount = Mathf.RoundToInt(effectPower);
            gameManager.playerController.Heal(healAmount);
        }
    }

    /// <summary>
    /// Reveal the map
    /// </summary>
    private void RevealMap(GameManager gameManager)
    {
        // Reveal all cells in the dungeon
        gameManager.dungeonManager.RevealMap();

        // Visual effect
        Camera.main.transform.DOShakePosition(0.2f, 0.1f, 10, 90);
    }

    /// <summary>
    /// Damage all enemies in the dungeon
    /// </summary>
    private void DamageAllEnemies(GameManager gameManager)
    {
        int damage = Mathf.RoundToInt(effectPower);
        gameManager.dungeonManager.DamageAllEnemies(damage);

        // Visual effect
        Camera.main.transform.DOShakePosition(0.4f, 0.3f, 10, 90);
    }
}