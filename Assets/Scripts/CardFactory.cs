using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Factory for creating different card types
/// </summary>
public class CardFactory : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameplayCardController cardController;

    [Header("Card Data")]
    [SerializeField] private List<CardData> pathCards = new List<CardData>();
    [SerializeField] private List<CardData> combatCards = new List<CardData>();
    [SerializeField] private List<CardData> specialCards = new List<CardData>();

    private System.Random rng;

    private void Awake()
    {
        rng = new System.Random();
    }

    private void Start()
    {
        if (cardController == null)
        {
            cardController = FindObjectOfType<GameplayCardController>();
        }
    }

    /// <summary>
    /// Create a random card of specified type
    /// </summary>
    public CardData CreateRandomCard(CardData.CardType cardType)
    {
        List<CardData> cardPool = GetCardPoolByType(cardType);

        if (cardPool.Count == 0)
        {
            Debug.LogWarning($"No cards of type {cardType} available");
            return null;
        }

        int randomIndex = rng.Next(cardPool.Count);
        return cardPool[randomIndex];
    }

    /// <summary>
    /// Create a random card of any type
    /// </summary>
    public CardData CreateRandomCard()
    {
        // Equal chance of any card type
        CardData.CardType randomType = (CardData.CardType)rng.Next(3);
        return CreateRandomCard(randomType);
    }

    /// <summary>
    /// Deal initial hand to player
    /// </summary>
    public void DealInitialHand(int pathCount = 3, int combatCount = 2, int specialCount = 1)
    {
        // Deal path cards
        for (int i = 0; i < pathCount; i++)
        {
            CardData card = CreateRandomCard(CardData.CardType.Path);
            if (card != null)
            {
                cardController.AddCardToHand(card);
            }
        }

        // Deal combat cards
        for (int i = 0; i < combatCount; i++)
        {
            CardData card = CreateRandomCard(CardData.CardType.Combat);
            if (card != null)
            {
                cardController.AddCardToHand(card);
            }
        }

        // Deal special cards
        for (int i = 0; i < specialCount; i++)
        {
            CardData card = CreateRandomCard(CardData.CardType.Special);
            if (card != null)
            {
                cardController.AddCardToHand(card);
            }
        }
    }

    /// <summary>
    /// Get the appropriate card pool based on type
    /// </summary>
    private List<CardData> GetCardPoolByType(CardData.CardType cardType)
    {
        switch (cardType)
        {
            case CardData.CardType.Path:
                return pathCards;
            case CardData.CardType.Combat:
                return combatCards;
            case CardData.CardType.Special:
                return specialCards;
            default:
                return new List<CardData>();
        }
    }

    /// <summary>
    /// Add a card to the appropriate pool
    /// </summary>
    public void AddCardToPool(CardData card)
    {
        GetCardPoolByType(card.cardType).Add(card);
    }
}