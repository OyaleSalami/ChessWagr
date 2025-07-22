using System.Collections.Generic;
using UnityEngine;

public class HighlightHelper : MonoBehaviour
{
    public GameObject highlightPrefabOld;
    public GameObject highlightPrefabNew;

    private List<GameObject> oldPositionHighlights;
    private List<GameObject> newPositionHighlights;

    private void Start()
    {
        oldPositionHighlights = new List<GameObject>();
        newPositionHighlights = new List<GameObject>();
    }

    private GameObject GetHighlightObject(List<GameObject> highlights, GameObject prefab)
    {
        GameObject go = highlights.Find(g => !g.activeSelf);

        if (go == null)
        {
            go = Instantiate(prefab);
            highlights.Add(go);
        }

        return go;
    }

    public void HighlightMove(Vector3 oldPosition, Vector3 newPosition)
    {
        // Highlight old position
        GameObject oldHighlight = GetHighlightObject(oldPositionHighlights, highlightPrefabOld);
        oldHighlight.SetActive(true);
        oldHighlight.transform.position = new Vector3(oldPosition.x, 0.0001f, oldPosition.z);

        // Highlight new position
        GameObject newHighlight = GetHighlightObject(newPositionHighlights, highlightPrefabNew);
        newHighlight.SetActive(true);
        newHighlight.transform.position = new Vector3(newPosition.x, 0.0001f, newPosition.z);
    }

    public void HideHighlights()
    {
        foreach (GameObject oldHighlight in oldPositionHighlights)
            oldHighlight.SetActive(false);

        foreach (GameObject newHighlight in newPositionHighlights)
            newHighlight.SetActive(false);
    }

    // Clean highlights after each team has played
    public void CleanTeamHighlights()
    {
        // Deactivate all old and new position highlights
        HideHighlights();
    }

    // Clean all highlights when the game ends
    public void CleanAllHighlights()
    {
        HideHighlights();

        // Optionally destroy the instantiated highlight objects if you want to reset everything at the end
        foreach (GameObject oldHighlight in oldPositionHighlights)
            Destroy(oldHighlight);

        foreach (GameObject newHighlight in newPositionHighlights)
            Destroy(newHighlight);

        oldPositionHighlights.Clear();
        newPositionHighlights.Clear();
    }
}
