using UnityEngine;

/// <summary>
/// Put this component on the Enter Wall. Enable Is Trigger on the wall's Collider,
/// or use a normal Collider with a Rigidbody on at least one of the colliding objects.
/// Assign the airplane object to avoid opening the popup for unrelated collisions.
/// </summary>
[RequireComponent(typeof(Collider))]
public class EnterGamePopupTrigger : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;
    [Tooltip("Optional. Assign the airplane GameObject to ignore collisions from other objects.")]
    [SerializeField] private GameObject airplane;

    private bool hasTriggered;

    private void Reset()
    {
        Collider wallCollider = GetComponent<Collider>();
        if (wallCollider != null)
        {
            wallCollider.isTrigger = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        HandleHit(other != null ? other.gameObject : null);
    }

    private void OnCollisionEnter(Collision collision)
    {
        HandleHit(collision != null ? collision.gameObject : null);
    }

    private void HandleHit(GameObject other)
    {
        if (hasTriggered || other == null)
        {
            return;
        }

        if (airplane != null && other != airplane && !other.transform.IsChildOf(airplane.transform))
        {
            return;
        }

        if (gameManager == null)
        {
            gameManager = FindObjectOfType<GameManager>();
        }

        if (gameManager == null)
        {
            Debug.LogError("EnterGamePopupTrigger: No GameManager found. Add GameManager to Main Camera and assign it here.", this);
            return;
        }

        hasTriggered = true;
        gameManager.ShowPopup();
    }
}
