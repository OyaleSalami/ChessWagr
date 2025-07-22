using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AvatarShopItem : MonoBehaviour
{

    public static AvatarShopItem instance;
    public string avatarName; // Name of the avatar
    public float price; // Price of the avatar
    public GameObject selectedIcon; // Icon to indicate selected state
    public Image avatarImage; // Image of the avatar
    public TMP_Text nameText; // Text for the avatar's name
    public TMP_Text priceText; // Text for the avatar's price
    public string savedData; // Data to save the avatar state

    private bool isPurchased = false; // Indicates if the avatar is purchased
    private bool isSelected = false; // Indicates if the avatar is selected

    private static List<AvatarShopItem> allShopItems = new List<AvatarShopItem>(); // Static list to track all avatar shop items

    private Motherboard motherboard; // Reference to the Motherboard script
    public string AvatarFindName = "demoavataraName"; // Base name for finding the avatar
    public string Fullpath;

    private void Start()
    {
        instance = this;

        // Initialize UI text
        nameText.text = avatarName;
        priceText.text = price.ToString("F2") + " Credits";
        selectedIcon.SetActive(false);

        // Find the Motherboard in the scene
        motherboard = FindObjectOfType<Motherboard>();

        // Add this item to the static list
        allShopItems.Add(this);
        LoadAvatarImage();
    }

    private void OnDestroy()
    {
        // Remove this item from the static list when destroyed
        allShopItems.Remove(this);
    }

    private void LoadAvatarImage()
    {
        if (!string.IsNullOrEmpty(AvatarFindName))
        {
            // Combine the base folder and the avatar name to construct the full path
            string fullPath = $"photo/{AvatarFindName}";
            Sprite loadedSprite = Resources.Load<Sprite>(fullPath);

            Fullpath = fullPath; // Save the full path for later use

            if (loadedSprite != null)
            {
                avatarImage.sprite = loadedSprite; // Assign the loaded sprite to the Image component
                Debug.Log($"Avatar image successfully loaded from: Resources/{fullPath}");
            }
            else
            {
                Debug.LogWarning($"Avatar image not found at Resources/{fullPath}");
            }
        }
        else
        {
            Debug.LogWarning("AvatarFindName is not set. Unable to load avatar image.");
        }
    }


    public void BuyItem()
    {
        if (motherboard.balance >= price)
        {
            if (!isPurchased)
            {
                isPurchased = true;
                priceText.text = "Select";

                // Deduct price from balance and update
                motherboard.balance -= price;
                motherboard.UpdateBalanceFunction();

                // Save purchase state
                motherboard.SAVESHOPDATAAvatar();
            }
            else
            {
                SelectItem();
            }
        }
        else
        {
            motherboard.ShowNotification("Not enough credits");
        }
    }

    public void SelectItem()
    {
        if (isPurchased)
        {
            // Deselect all other items
            foreach (var item in allShopItems)
            {
                if (item != this && item.IsSelected())
                {
                    item.DeselectItem();
                }
            }

            if (!isSelected)
            {
                isSelected = true;
                selectedIcon.SetActive(true);

                Motherboard motherboard = FindObjectOfType<Motherboard>();
            if (motherboard != null)
            {
                motherboard.UpdateAvatarProfile(avatarImage.sprite, avatarName, Fullpath);
                Motherboard.instance.UploadImage();
            }
            else
            {
                Debug.LogWarning("Motherboard instance not found. Cannot update AvatarProfile.");
            }
                // Save selection state
                motherboard.SAVESHOPDATAAvatar();
            }
        }
    }

    public void DeselectItem()
    {
        isSelected = false;
        selectedIcon.SetActive(false);
    }

    public bool IsSelected()
    {
        return isSelected;
    }

    public string SaveItemData()
    {
        savedData = $"{avatarName},{price},{isPurchased},{isSelected}";
        return savedData;
    }

    public void LoadItemData(string data)
    {
        string[] values = data.Split(',');

        avatarName = values[0];
        price = float.Parse(values[1]);
        isPurchased = bool.Parse(values[2]);
        isSelected = bool.Parse(values[3]);

        nameText.text = avatarName;
        priceText.text = isPurchased ? "Select" : price.ToString("F2") + " Credits";
        selectedIcon.SetActive(isSelected);

        // If this item is selected, update the AvatarProfile in the Motherboard
        if (isSelected)
        {
            Motherboard motherboard = FindObjectOfType<Motherboard>();
            if (motherboard != null)
            {
                motherboard.UpdateAvatarProfile(avatarImage.sprite, avatarName, Fullpath);
            }
            else
            {
                Debug.LogWarning("Motherboard instance not found. Cannot update AvatarProfile.");
            }
        }
    }


}
