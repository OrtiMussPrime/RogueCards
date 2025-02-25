using System.Collections;
using UnityEngine;
using DG.Tweening;

/// <summary>
/// Extension methods for the HorizontalCardHolder class
/// </summary>
public static class HorizontalCardHolderExtension
{
    /// <summary>
    /// Updates the positions of cards in the hand when a card is removed
    /// </summary>
    public static void UpdateCardPositions(this HorizontalCardHolder holder)
    {
        // Update the layout
        holder.GetComponent<UnityEngine.UI.HorizontalLayoutGroup>().CalculateLayoutInputHorizontal();
        holder.GetComponent<UnityEngine.UI.HorizontalLayoutGroup>().SetLayoutHorizontal();

        // Also notify each card visual to update its index
        foreach (Card card in holder.cards)
        {
            if (card.cardVisual != null)
            {
                card.cardVisual.UpdateIndex(holder.transform.childCount);
            }
        }
    }
}

/// <summary>
/// Adds the UpdateCardPositions method directly to HorizontalCardHolder
/// </summary>
public partial class HorizontalCardHolder
{
    /// <summary>
    /// Updates the positions of cards in the hand when a card is removed
    /// </summary>
    public void UpdateCardPositions()
    {
        // Update the layout
        GetComponent<UnityEngine.UI.HorizontalLayoutGroup>().CalculateLayoutInputHorizontal();
        GetComponent<UnityEngine.UI.HorizontalLayoutGroup>().SetLayoutHorizontal();

        // Also notify each card visual to update its index
        foreach (Card card in cards)
        {
            if (card.cardVisual != null)
            {
                card.cardVisual.UpdateIndex(transform.childCount);
            }
        }
    }
}