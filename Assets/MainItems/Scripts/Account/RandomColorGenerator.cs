using UnityEngine;

public class RandomColorGenerator : MonoBehaviour
{

    public static RandomColorGenerator instance;
    public string ChatColor;
    public static string GetRandomChatColor()
    {
        // Generate random values for RGB
        int r = Random.Range(0, 256);
        int g = Random.Range(0, 256);
        int b = Random.Range(0, 256);

        // Convert RGB to a hex string in the format "<#RRGGBB>"
        string hexColor = $"<#{r:X2}{g:X2}{b:X2}>";
        return hexColor;
    }

    void Start()
    {
        instance = this;
        // Example usage
        ChatColor = GetRandomChatColor();
        Debug.Log($"Generated Chat Color: {ChatColor}");
    }
}
