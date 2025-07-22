using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HighlightManager : MonoBehaviour
{
    public GameObject oldPositionPrefab; // Prefab for highlighting the old position
    public GameObject newPositionPrefab; // Prefab for highlighting the new position

    private GameObject oldHighlightInstance;
    private GameObject newHighlightInstance;

    public void HighlightMove(Vector3 oldPosition, Vector3 newPosition)
    {
        // Clear previous highlights
        ClearHighlights();

        // Instantiate highlight at the old position
        oldHighlightInstance = Instantiate(oldPositionPrefab, oldPosition, Quaternion.identity);
        oldHighlightInstance.transform.position += new Vector3(0, 0.01f, 0); // Slightly raise to avoid z-fighting

        // Instantiate highlight at the new position
        newHighlightInstance = Instantiate(newPositionPrefab, newPosition, Quaternion.identity);
        newHighlightInstance.transform.position += new Vector3(0, 0.01f, 0); // Slightly raise to avoid z-fighting
    }

    private void ClearHighlights()
    {
        // Destroy previous highlights
        if (oldHighlightInstance != null)
        {
            Destroy(oldHighlightInstance);
        }

        if (newHighlightInstance != null)
        {
            Destroy(newHighlightInstance);
        }
    }
}
