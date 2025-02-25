using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Bridge between the Card animation system and the Dungeon gameplay mechanics
/// </summary>
public class GameplayCardController : MonoBehaviour
{
    [Header("References")]
    public HorizontalCardHolder handHolder;
    public DungeonManager dungeonManager;
    public GameManager gameManager;

    [Header("Card Spawning")]
    public GameObject pathCardPrefab;
    public GameObject combatCardPrefab;
    public GameObject specialCardPrefab;
    
    private void Start()
    {
        if (handHolder == null)
        {
            handHolder = FindObjectOfType<HorizontalCardHolder>();
        }
        
        if (dungeonManager == null)
        {
            dungeonManager = FindObjectOfType<DungeonManager>();
        }
        
        if (gameManager == null)
        {
            gameManager = FindObjectOfType<GameManager>();
        }
        
        // Subscribe to card events from the existing system
        SubscribeToCards();
    }
    
    /// <summary>
    /// Subscribe to events for all cards in the hand
    /// </summary>
    private void SubscribeToCards()
    {
        foreach (Card card in handHolder.cards)
        {
            SubscribeToCard(card);
        }
    }
    
    /// <summary>
    /// Subscribe to events for a specific card
    /// </summary>
    private void SubscribeToCard(Card card)
    {
        // Only subscribe if the card has a CardDataComponent
        if (card.GetComponent<CardDataComponent>() != null)
        {
            card.BeginDragEvent.AddListener(OnCardBeginDrag);
            card.EndDragEvent.AddListener(OnCardEndDrag);
        }
    }
    
    /// <summary>
    /// Called when a card begins to be dragged
    /// </summary>
    private void OnCardBeginDrag(Card card)
    {
        // Show valid placement positions in the dungeon
        CardDataComponent dataComponent = card.GetComponent<CardDataComponent>();
        if (dataComponent != null)
        {
            dungeonManager.HighlightValidGridPositions(dataComponent.cardData);
        }
    }
    
    /// <summary>
    /// Called when a card is dropped
    /// </summary>
    private void OnCardEndDrag(Card card)
    {
        CardDataComponent dataComponent = card.GetComponent<CardDataComponent>();
        if (dataComponent == null) return;
        
        // Check if card was dropped on dungeon grid
        Vector2Int gridPos = dungeonManager.WorldToGridPosition(card.transform.position);
        if (dungeonManager.IsValidGridPosition(gridPos, dataComponent.cardData))
        {
            // Execute card effect
            dataComponent.cardData.OnCardPlayed(gridPos, gameManager);
            
            // Apply visual effect based on card type
            PlayCardEffect(dataComponent.cardData.cardType, gridPos);
            
            // Remove card from hand
            handHolder.cards.Remove(card);
            Destroy(card.transform.parent.gameObject);
            
            // Update the rest of the cards
            handHolder.UpdateCardPositions();
        }
        
        // Hide grid highlights
        dungeonManager.ClearHighlights();
    }
    
    /// <summary>
    /// Play visual effect based on card type
    /// </summary>
    private void PlayCardEffect(CardData.CardType cardType, Vector2Int gridPos)
    {
        Vector3 worldPos = dungeonManager.GridToWorldPosition(gridPos);
        
        // Different effects based on card type
        switch (cardType)
        {
            case CardData.CardType.Path:
                // Path creation effect
                StartCoroutine(PathCreationEffect(worldPos));
                break;
                
            case CardData.CardType.Combat:
                // Combat effect
                StartCoroutine(CombatEffect(worldPos));
                break;
                
            case CardData.CardType.Special:
                // Special effect
                StartCoroutine(SpecialEffect(worldPos));
                break;
        }
    }
    
    /// <summary>
    /// Visual effect for creating a path
    /// </summary>
    private IEnumerator PathCreationEffect(Vector3 position)
    {
        // Create a particle effect or animation
        // This would use DOTween or similar for consistency with your card animations
        
        // Example: Camera shake on path creation
        Camera.main.transform.DOShakePosition(0.2f, 0.1f, 10, 90);
        
        yield return null;
    }
    
    /// <summary>
    /// Visual effect for combat
    /// </summary>
    private IEnumerator CombatEffect(Vector3 position)
    {
        // Create a combat effect
        // This would use DOTween or similar for consistency
        
        // Example: Flash effect and camera shake
        Camera.main.transform.DOShakePosition(0.3f, 0.2f, 10, 90);
        
        yield return null;
    }
    
    /// <summary>
    /// Visual effect for special cards
    /// </summary>
    private IEnumerator SpecialEffect(Vector3 position)
    {
        // Create a special effect
        // This would use DOTween or similar for consistency
        
        // Example: Radial effect
        Camera.main.transform.DOShakePosition(0.4f, 0.3f, 10, 90);
        
        yield return null;
    }
    
    /// <summary>
    /// Add a new card to the player's hand
    /// </summary>
    public void AddCardToHand(CardData cardData)
    {
        // Create a card slot
        GameObject slotObj = Instantiate(handHolder.slotPrefab, handHolder.transform);
        
        // Find the card inside the slot
        Card card = slotObj.GetComponentInChildren<Card>();
        if (card != null)
        {
            // Add CardDataComponent
            CardDataComponent dataComponent = card.gameObject.AddComponent<CardDataComponent>();
            dataComponent.cardData = cardData;
            
            // Update visuals based on card type
            if (card.cardVisual != null)
            {
                UpdateCardVisual(card.cardVisual, cardData);
            }
            
            // Subscribe to the new card
            SubscribeToCard(card);
            
            // Add to list
            handHolder.cards.Add(card);
        }
    }
    
    /// <summary>
    /// Update card visual based on CardData
    /// </summary>
    private void UpdateCardVisual(CardVisual visual, CardData cardData)
    {
        // Update the card image
        if (visual.cardImage != null)
        {
            // Set sprite based on card type
            switch (cardData.cardType)
            {
                case CardData.CardType.Path:
                    // Set path card sprite
                    break;
                    
                case CardData.CardType.Combat:
                    // Set combat card sprite
                    break;
                    
                case CardData.CardType.Special:
                    // Set special card sprite
                    break;
            }
        }
    }
}