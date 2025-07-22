using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class Mainloading : MonoBehaviour
{
    public Slider Loader;
    public string sceneNameToLoad;

    [Header("UI Elements")]
    [SerializeField] private Image loadingImage;  // Assign your loading image in the inspector

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Make sure the loading image is initially hidden
        loadingImage.gameObject.SetActive(false);
        LoadSceneWithImage(); 
    }

    public void LoadSceneWithImage()
    {
        // Start the loading process
        StartCoroutine(LoadScene());
    }

    private IEnumerator LoadScene()
    {
        // Show the loading image
        loadingImage.gameObject.SetActive(true);
        Loader.value = 0;

        // Start loading the scene asynchronously
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneNameToLoad);

        // Wait until the scene is fully loaded
        while (!asyncLoad.isDone)
        {
            Loader.maxValue = asyncLoad.progress;
            float progressmain = Mathf.Clamp01(asyncLoad.progress / .9f);
            Debug.Log(progressmain);

            Loader.value = progressmain * 100;

            // Optionally, you can add a loading progress bar logic here
            yield return null;  // Continue waiting until the next frame
        }

        // Hide the loading image once the scene is loaded
        loadingImage.gameObject.SetActive(false);
    }
}
