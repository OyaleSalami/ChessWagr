using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using System.Collections;

public class HelloPhpRequest : MonoBehaviour
{
    public TMP_Text responseText; // TextMeshPro component to display the PHP response
    private string phpApiUrl = "http://connectboy.isellibuys.com/hello_api.php"; // URL to your PHP file

    public void LStart()
    {
        StartCoroutine(GetText());
    }

    IEnumerator GetText()
    {
        using (UnityWebRequest www = UnityWebRequest.Get("http://connectboy.isellibuys.com/hello_api.php"))
        {
            yield return www.Send();

            if (www.isNetworkError || www.isHttpError)
            {
                Debug.Log(www.error);
            }
            else
            {
                // Show results as text
                Debug.Log(www.downloadHandler.text);

                // Or retrieve results as binary data
                byte[] results = www.downloadHandler.data;
            }
        }
    }
}
