using UnityEngine;
using UnityEngine.EventSystems; // Required for IDragHandler and IEndDragHandler
using System.Collections;

public class SwipeOutNotification : MonoBehaviour, IDragHandler, IEndDragHandler
{
    public float swipeThreshold = 100f;  // Minimum swipe distance to trigger the swipe out
    public float swipeSpeed = 5f;  // Speed of the swipe
    public float autoDismissTime = 3f;  // Time in seconds before it auto-dismisses
    private RectTransform rectTransform;  // The RectTransform of the UI element
    private Vector3 startPosition;  // The initial position of the notification
    private bool isSwiping = false;  // Flag to check if it's currently swiping
    private Vector3 swipeDirection;  // Direction of the swipe
    private bool isDismissed = false;  // To track if the notification has been dismissed

    private void Start()
    {
        rectTransform = GetComponent<RectTransform>();

        // Start the notification off-screen (e.g., to the top center)
        float startY = Screen.height + rectTransform.rect.height / 2;  // Position above the screen
        rectTransform.anchoredPosition = new Vector3(0, startY, 0);  // Centered at the top
        startPosition = rectTransform.anchoredPosition;  // Store the starting position

        // Optionally start the auto-dismiss coroutine
        StartCoroutine(AutoDismissCoroutine());
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isSwiping && !isDismissed)
        {
            // Update the object's position based on the drag
            Vector3 newPosition = rectTransform.anchoredPosition + eventData.delta;
            rectTransform.anchoredPosition = newPosition;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!isSwiping && !isDismissed)
        {
            // Determine swipe direction
            swipeDirection = (eventData.position - rectTransform.anchoredPosition).normalized; // Ensure it's normalized
            float swipeDistance = Vector3.Distance(eventData.position, rectTransform.anchoredPosition);

            if (swipeDistance > swipeThreshold)
            {
                // Start swiping the notification out of the screen
                StartCoroutine(SwipeOutCoroutine());
            }
            else
            {
                // Return the notification to its original position if not swiped out
                StartCoroutine(ReturnToStartCoroutine());
            }
        }
    }

    private IEnumerator SwipeOutCoroutine()
    {
        isSwiping = true;
        isDismissed = true; // Mark as dismissed

        // Calculate the target position by moving it completely off the screen
        Vector3 targetPosition = new Vector3(
            swipeDirection.x > 0 ? Screen.width : -Screen.width, // Move right or left
            rectTransform.anchoredPosition.y, // Keep the Y position the same
            0 // Z position is not needed for 2D UI
        );

        // Move the notification out of the screen with a smooth animation
        while (Vector3.Distance(rectTransform.anchoredPosition, targetPosition) > 0.1f)
        {
            rectTransform.anchoredPosition = Vector3.MoveTowards(rectTransform.anchoredPosition, targetPosition, swipeSpeed * Time.deltaTime);
            yield return null;
        }

        // Ensure the object is completely out of the screen
        rectTransform.anchoredPosition = targetPosition;
        gameObject.SetActive(false);  // Optionally deactivate the notification
        isSwiping = false;
    }

    private IEnumerator ReturnToStartCoroutine()
    {
        isSwiping = true;

        // Return the notification to its original position with a smooth animation
        while (Vector3.Distance(rectTransform.anchoredPosition, startPosition) > 0.1f)
        {
            rectTransform.anchoredPosition = Vector3.MoveTowards(rectTransform.anchoredPosition, startPosition, swipeSpeed * Time.deltaTime);
            yield return null;
        }

        // Ensure the object is back to its original position
        rectTransform.anchoredPosition = startPosition;

        isSwiping = false;
    }

    private IEnumerator AutoDismissCoroutine()
    {
        yield return new WaitForSeconds(autoDismissTime);
        if (!isSwiping && !isDismissed)
        {
            StartCoroutine(SwipeOutCoroutine());  // Automatically swipe out
        }
    }
}
