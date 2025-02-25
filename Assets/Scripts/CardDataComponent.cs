using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Attaches to a Card to provide gameplay functionality
/// </summary>
public class CardDataComponent : MonoBehaviour
{
    public CardData cardData;

    [Header("UI References")]
    public TextMeshProUGUI cardNameText;
    public TextMeshProUGUI cardDescriptionText;
    public Image cardArtImage;

    private void Start()
    {
        // Find UI elements if not set
        if (cardNameText == null)
        {
            cardNameText = GetComponentInChildren<TextMeshProUGUI>();
        }

        if (cardArtImage == null)
        {
            cardArtImage = transform.Find("CardVisual/TiltParent/Sprite")?.GetComponent<Image>();
        }

        // Update UI with card data
        UpdateCardUI();
    }

    /// <summary>
    /// Update card UI elements based on card data
    /// </summary>
    public void UpdateCardUI()
    {
        if (cardData == null) return;

        if (cardNameText != null)
        {
            cardNameText.text = cardData.cardName;
        }

        if (cardDescriptionText != null)
        {
            cardDescriptionText.text = cardData.description;
        }

        if (cardArtImage != null && cardData.cardArt != null)
        {
            cardArtImage.sprite = cardData.cardArt;
        }

        // Set color or other visual properties based on card type
        if (cardArtImage != null)
        {
            Color cardColor = Color.white;

            switch (cardData.cardType)
            {
                case CardData.CardType.Path:
                    cardColor = new Color(0.2f, 0.6f, 1f);
                    break;
                case CardData.CardType.Combat:
                    cardColor = new Color(1f, 0.4f, 0.4f);
                    break;
                case CardData.CardType.Special:
                    cardColor = new Color(0.8f, 0.4f, 0.8f);
                    break;
            }

            // Tint the card slightly based on type
            cardArtImage.color = cardColor;
        }
    }

    /// <summary>
    /// Set new card data and update UI
    /// </summary>
    public void SetCardData(CardData data)
    {
        cardData = data;
        UpdateCardUI();
    }
}