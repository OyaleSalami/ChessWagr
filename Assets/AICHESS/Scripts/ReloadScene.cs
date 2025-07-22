using UnityEngine;
using UnityEngine.SceneManagement;
public class ReloadScene : MonoBehaviour
{
    public string SceneName = "";

    public void Reload()
    {
        SceneManager.LoadScene(SceneName);
    }
}
