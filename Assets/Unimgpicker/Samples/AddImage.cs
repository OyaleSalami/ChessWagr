using Kakera;
using System;
using System.Collections;
using System.Runtime.InteropServices;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Android;

// WebGLで使うときはIPointerDownHandlerを継承する必要がある点に注意
public class AddImage : MonoBehaviour, IPointerDownHandler
{
    public static AddImage instance;
    [SerializeField] private Unimgpicker imagePicker;
    [SerializeField] private Image ssImage;
    public Texture2D texture;
    public Sprite texture2;
    public string Location;

    private void Start()
    {
        if (!ssImage)
        {
            ssImage = gameObject.GetComponent<Image>();
        }

        instance = this;
    }

#if !UNITY_WEBGL
    private void Awake()
    {
        imagePicker.Completed += path => StartCoroutine(LoadImage(path, ssImage));
    }

    public void OnPressShowPicker()
    {
        if (!Permission.HasUserAuthorizedPermission(Permission.ExternalStorageRead))
        {
            Permission.RequestUserPermission(Permission.ExternalStorageRead);
        }
        else
        {
            imagePicker.Show("Select Image", "unimgpicker", 256);//1024→512に変更
        }

        if (!Permission.HasUserAuthorizedPermission(Permission.ExternalStorageWrite))
        {
            Permission.RequestUserPermission(Permission.ExternalStorageWrite);
        }
        else
        {
            imagePicker.Show("Select Image", "unimgpicker", 16);//1024→512に変更
        }
    }

    private IEnumerator LoadImage(string path, Image output)
    {
        string url = "file://" + path;
        Location = path;

        WWW www = new WWW(url);
        yield return www;
        Motherboard.instance.ShowNotification("Click the save button to complete the change...");
        texture = www.texture;
        // まずリサイズ
        int _CompressRate = TextureCompressionRate.TextureCompressionRatio(texture.width, texture.height);
        TextureScale.Bilinear(texture, texture.width / _CompressRate, texture.height / _CompressRate);
        // 次に圧縮(縦長・横長すぎると使えない場合があるようです。) -> https://forum.unity.com/threads/strange-error-message-miplevel-m_mipcount.441907/
        //texture.Compress(false);
        // Spriteに変換して使用する
        texture2 = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
        output.overrideSprite = texture2;
    }

    // WebGLを使わない場合はこの関数(OnPointerDown)は不要
    public void OnPointerDown(PointerEventData eventData) { }

#elif UNITY_WEBGL
    [DllImport("__Internal")]
    private static extern void UploadFile(string gameObjectName, string methodName, string filter, bool multiple);

    public void OnPointerDown(PointerEventData eventData)
    {
        UploadFile(gameObject.name, "OnFileUpload", ".png, .PNG, .jpg, .jpeg", false);
    }

    // Called from browser
    public void OnFileUpload(string url)
    {
        StartCoroutine(OutputRoutine(url));
    }


    private IEnumerator OutputRoutine(string url)
    {
        var www = new WWW(url);
        yield return www;

        texture = www.texture;
        // まずリサイズ
        int _CompressRate = TextureCompressionRate.TextureCompressionRatio(texture.width, texture.height);
        TextureScale.Bilinear(texture, texture.width / _CompressRate, texture.height / _CompressRate);
        // 次に圧縮(縦長・横長すぎると使えない場合があるようです。) -> https://forum.unity.com/threads/strange-error-message-miplevel-m_mipcount.441907/
        //texture.Compress(false);
        // Spriteに変換して使用する
        texture2 = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
        ssImage.overrideSprite = texture2;
    }
#endif
}


public static class TextureCompressionRate
{
    /// <summary>
    /// Textureが500x500に収まるようにリサイズします
    /// </summary>
    public static int TextureCompressionRatio(int width, int height)
    {
        if (width >= height)
        {
            if (width / 500 > 0) return (width / 500);
            else return 1;
        }
        else if (width < height)
        {
            if (height / 500 > 0) return (height / 500);
            else return 1;
        }
        else return 1;
    }
}