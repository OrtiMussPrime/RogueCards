using System;
using UnityEngine;
using DG.Tweening;

/// <summary>
/// Controls the dice visuals and animation
/// </summary>
public class DiceController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private MeshRenderer meshRenderer;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Transform shakeParent;
    [SerializeField] private Transform visualTransform;

    [Header("Visual Settings")]
    [SerializeField] private Material[] diceMaterials; // Different dice materials for different side counts
    [SerializeField] private Transform[] facesParent; // Parent for each face visual
    [SerializeField] private Sprite[] diceFaces; // Sprites for each face

    [Header("Animation Settings")]
    [SerializeField] private float jumpHeight = 2f;
    [SerializeField] private int jumpCount = 3;
    [SerializeField] private float rollDuration = 1.5f;
    [SerializeField] private float punchAmount = 20f;
    [SerializeField] private int vibrato = 10;
    [SerializeField] private float shakeIntensity = 0.3f;

    private int sides = 6; // Default to 6-sided die
    private bool isRolling = false;
    private int rollResult = 0;
    private Action<int> rollCallback;

    private void Start()
    {
        // Initialize references if not set
        if (meshRenderer == null)
            meshRenderer = GetComponent<MeshRenderer>();

        if (rb == null)
            rb = GetComponent<Rigidbody>();

        if (visualTransform == null)
            visualTransform = transform;

        if (shakeParent == null)
            shakeParent = transform;
    }

    /// <summary>
    /// Set the number of sides for the die
    /// </summary>
    public void SetDiceSides(int diceSides)
    {
        sides = Mathf.Max(2, diceSides);

        // Change mesh/material based on number of sides
        if (meshRenderer != null && diceMaterials.Length > 0)
        {
            // Choose appropriate material
            int materialIndex = Mathf.Clamp(sides / 2 - 1, 0, diceMaterials.Length - 1);
            meshRenderer.material = diceMaterials[materialIndex];
        }
    }

    /// <summary>
    /// Get the roll result
    /// </summary>
    public int GetRollResult()
    {
        if (rollResult == 0)
        {
            // Generate a random result if one hasn't been set
            rollResult = UnityEngine.Random.Range(1, sides + 1);
        }
        return rollResult;
    }

    /// <summary>
    /// Roll the dice with animation
    /// </summary>
    public void Roll(Action<int> onComplete = null)
    {
        if (isRolling) return;

        isRolling = true;
        rollCallback = onComplete;

        // Reset result
        rollResult = 0;

        // Create a sequence for the roll animation
        Sequence rollSequence = DOTween.Sequence();

        // Jump animation
        rollSequence.Append(transform.DOJump(
            transform.position + Vector3.up * jumpHeight,
            jumpHeight,
            jumpCount,
            rollDuration
        ).SetEase(Ease.OutQuad));

        // Rotation during jump
        rollSequence.Join(transform.DORotate(
            new Vector3(
                UnityEngine.Random.Range(0, 360 * 2),
                UnityEngine.Random.Range(0, 360 * 2),
                UnityEngine.Random.Range(0, 360 * 2)
            ),
            rollDuration,
            RotateMode.FastBeyond360
        ).SetEase(Ease.OutQuad));

        // Shake on landing
        rollSequence.AppendCallback(() =>
        {
            // Generate result
            rollResult = UnityEngine.Random.Range(1, sides + 1);

            // Shake effect
            shakeParent.DOShakePosition(0.3f, shakeIntensity, vibrato, 90);
            shakeParent.DOPunchRotation(Vector3.forward * punchAmount, 0.3f, vibrato, 1);

            // Show the result
            ShowResult(rollResult);
        });

        // Wait for result to be visible
        rollSequence.AppendInterval(0.5f);

        // Complete
        rollSequence.OnComplete(() =>
        {
            isRolling = false;
            rollCallback?.Invoke(rollResult);
        });
    }

    /// <summary>
    /// Show the roll result visually
    /// </summary>
    private void ShowResult(int result)
    {
        // Display the correct face
        if (facesParent != null && facesParent.Length > 0)
        {
            // Hide all faces
            for (int i = 0; i < facesParent.Length; i++)
            {
                if (facesParent[i] != null)
                    facesParent[i].gameObject.SetActive(false);
            }

            // Show the result face
            int faceIndex = Mathf.Clamp(result - 1, 0, facesParent.Length - 1);
            if (faceIndex < facesParent.Length && facesParent[faceIndex] != null)
            {
                facesParent[faceIndex].gameObject.SetActive(true);
            }
        }
    }

    /// <summary>
    /// Simulate physics-based dice roll (for more advanced implementations)
    /// </summary>
    public void PhysicsRoll()
    {
        if (rb != null)
        {
            isRolling = true;

            // Apply random force and torque
            rb.AddForce(UnityEngine.Random.onUnitSphere * 5f, ForceMode.Impulse);
            rb.AddTorque(UnityEngine.Random.onUnitSphere * 5f, ForceMode.Impulse);

            // Start checking which face is up
            StartCoroutine(CheckDiceResult());
        }
    }

    /// <summary>
    /// Coroutine to check which face is up after a physics roll
    /// </summary>
    private System.Collections.IEnumerator CheckDiceResult()
    {
        // Wait for dice to settle
        yield return new WaitForSeconds(2.0f);

        // Check which face is up (implementation would depend on specific dice model)
        // For a simple implementation, we'll just use random
        rollResult = UnityEngine.Random.Range(1, sides + 1);
        isRolling = false;

        // Show the result
        ShowResult(rollResult);

        // Invoke callback
        rollCallback?.Invoke(rollResult);
    }
}