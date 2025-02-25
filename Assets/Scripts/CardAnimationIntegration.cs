using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

/// <summary>
/// Connects the card animation system with dungeon gameplay
/// </summary>
public class CardAnimationIntegration : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameplayCardController cardController;
    [SerializeField] private DungeonManager dungeonManager;
    [SerializeField] private VisualCardsHandler visualHandler;

    [Header("Card Animation Parameters")]
    [SerializeField] private float placementPunchAmount = 0.3f;
    [SerializeField] private int placementVibrato = 10;
    [SerializeField] private float placementDuration = 0.3f;

    [Header("Path Effect Parameters")]
    [SerializeField] private float pathCreationDelay = 0.1f;
    [SerializeField] private float pathCreationDuration = 0.3f;

    [Header("Grid Highlight Parameters")]
    [SerializeField] private float highlightScaleEffect = 1.2f;
    [SerializeField] private float highlightDuration = 0.2f;

    private void Start()
    {
        // Find references if not set
        if (cardController == null)
            cardController = FindObjectOfType<GameplayCardController>();

        if (dungeonManager == null)
            dungeonManager = FindObjectOfType<DungeonManager>();

        if (visualHandler == null)
            visualHandler = FindObjectOfType<VisualCardsHandler>();
    }

    /// <summary>
    /// Apply a placement animation to a card visual
    /// </summary>
    public void PlayCardPlacementAnimation(Card card, Vector2Int gridPosition)
    {
        if (card.cardVisual == null) return;

        // Create a sequence for the placement animation
        Sequence placementSequence = DOTween.Sequence();

        // Get the world position
        Vector3 worldPosition = dungeonManager.GridToWorldPosition(gridPosition);

        // Animate the card to the target position
        placementSequence.Append(card.cardVisual.transform.DOMove(worldPosition, 0.3f).SetEase(Ease.OutBack));

        // Add a punch effect
        placementSequence.Join(card.cardVisual.transform.DOPunchScale(Vector3.one * 0.2f, 0.3f, placementVibrato, 1));

        // Add a shake effect
        placementSequence.Join(card.cardVisual.transform.DOShakeRotation(0.3f, 30, placementVibrato, 90));

        // Fade out at the end
        placementSequence.Append(card.cardVisual.transform.DOScale(0, 0.2f).SetEase(Ease.InBack));

        // Cleanup
        placementSequence.OnComplete(() =>
        {
            Destroy(card.cardVisual.gameObject);
        });
    }

    /// <summary>
    /// Add animations to path creation
    /// </summary>
    public void AnimatePathCreation(Vector2Int position, PathCardData.PathDirection[] directions)
    {
        // Get the cell at the position
        GridCell cell = dungeonManager.GetCell(position);
        if (cell == null) return;

        // Animate the cell
        cell.transform.DOPunchScale(Vector3.one * 0.2f, 0.3f, 10, 1);

        // Create sequence for path animations
        Sequence pathSequence = DOTween.Sequence();

        // Delay between each direction
        float delay = 0f;

        // Animate each path direction
        foreach (PathCardData.PathDirection direction in directions)
        {
            Vector2Int targetPos = GetAdjacentPosition(position, direction);
            GridCell targetCell = dungeonManager.GetCell(targetPos);

            if (targetCell != null)
            {
                // Add delay for sequential animation
                delay += pathCreationDelay;

                // Animate the path creation with delay
                pathSequence.InsertCallback(delay, () =>
                {
                    targetCell.transform.DOScale(0, 0).SetEase(Ease.OutBack);
                    targetCell.transform.DOScale(1, pathCreationDuration).SetEase(Ease.OutBack);
                    targetCell.transform.DOPunchRotation(Vector3.forward * 30, pathCreationDuration, 10, 1);
                });
            }
        }

        // Camera shake effect at the end
        pathSequence.InsertCallback(delay + pathCreationDelay, () =>
        {
            Camera.main.transform.DOShakePosition(0.2f, 0.1f, 10, 90);
        });
    }

    /// <summary>
    /// Animate highlights for valid grid positions
    /// </summary>
    public void AnimateGridHighlights(List<Vector2Int> validPositions)
    {
        foreach (Vector2Int pos in validPositions)
        {
            GridCell cell = dungeonManager.GetCell(pos);
            if (cell != null)
            {
                // Scale up and down for highlight effect
                cell.transform.DOScale(highlightScaleEffect, highlightDuration)
                    .SetLoops(2, LoopType.Yoyo)
                    .SetEase(Ease.InOutSine);
            }
        }
    }

    /// <summary>
    /// Get the adjacent position based on direction
    /// </summary>
    private Vector2Int GetAdjacentPosition(Vector2Int position, PathCardData.PathDirection direction)
    {
        switch (direction)
        {
            case PathCardData.PathDirection.North:
                return position + Vector2Int.up;
            case PathCardData.PathDirection.East:
                return position + Vector2Int.right;
            case PathCardData.PathDirection.South:
                return position + Vector2Int.down;
            case PathCardData.PathDirection.West:
                return position + Vector2Int.left;
            default:
                return position;
        }
    }

    /// <summary>
    /// Convert from Unity's Vector2Int direction to PathDirection enum
    /// </summary>
    public PathCardData.PathDirection VectorToDirection(Vector2Int direction)
    {
        if (direction == Vector2Int.up)
            return PathCardData.PathDirection.North;
        else if (direction == Vector2Int.right)
            return PathCardData.PathDirection.East;
        else if (direction == Vector2Int.down)
            return PathCardData.PathDirection.South;
        else if (direction == Vector2Int.left)
            return PathCardData.PathDirection.West;
        else
            return PathCardData.PathDirection.North; // Default
    }

    /// <summary>
    /// Add ripple effect to a position
    /// </summary>
    public void CreateRippleEffect(Vector2Int position)
    {
        Vector3 worldPos = dungeonManager.GridToWorldPosition(position);

        // Create a visual ripple effect
        GameObject ripple = new GameObject("Ripple");
        ripple.transform.position = worldPos;

        // Add a sprite renderer
        SpriteRenderer renderer = ripple.AddComponent<SpriteRenderer>();
        renderer.sprite = Resources.Load<Sprite>("Circle"); // Simple circle sprite
        renderer.color = new Color(1f, 1f, 1f, 0.5f);

        // Start small and transparent
        ripple.transform.localScale = Vector3.zero;

        // Grow and fade out
        ripple.transform.DOScale(3f, 1f).SetEase(Ease.OutQuad);
        renderer.DOFade(0f, 1f).SetEase(Ease.OutQuad);

        // Destroy when done
        Destroy(ripple, 1.1f);
    }
}