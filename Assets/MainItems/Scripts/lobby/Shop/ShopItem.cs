using System.Collections.Generic; // Add this for List
using UnityEngine;
using TMPro;

public class ShopItem : MonoBehaviour
{
    public string itemName;
    public float price;
    public GameObject SelectedIcon;
    public GameObject material1;
    public GameObject material2;
    public TMP_Text nameText;
    public TMP_Text priceText;
    public string SavedData;

    private bool isPurchased = false;
    private bool isSelected = false;

    [SerializeField] private Material materialForTeam1; // Material for team 1
    [SerializeField] private Material materialForTeam2; // Material for team 2
    private PiecesCreator piecesCreator; // Reference to the PiecesCreator

    private Motherboard motherboard; // Reference to the Motherboard

    private static List<ShopItem> allShopItems = new List<ShopItem>(); // Static list to track all shop items

    private void Start()
    {
        nameText.text = itemName;
        priceText.text = price.ToString("F2") + " Credits";
        SelectedIcon.SetActive(false);

        // Find the Motherboard in the scene
        motherboard = FindObjectOfType<Motherboard>();
        piecesCreator = FindObjectOfType<PiecesCreator>();

        // Add this item to the static list
        allShopItems.Add(this);
    }

    private void OnDestroy()
    {
        // Remove this item from the static list when destroyed
        allShopItems.Remove(this);
    }

    private void SetMaterial(GameObject materialObject, Material newMaterial)
    {
        if (materialObject != null && newMaterial != null)
        {
            Renderer renderer = materialObject.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material = newMaterial;
            }
        }
    }

    public void BuyItem()
    {
        if(motherboard.balance >= price)
        {
            if (!isPurchased)
            {
                isPurchased = true;
                priceText.text = "Select";
                // Notify the Motherboard that this item is purchased
                if (motherboard != null)
                {
                    motherboard.ItemPurchased(this);
                    motherboard.SAVESHOPDATA();
                    motherboard.balance -= price;
                    motherboard.UpdateBalanceFunction();
                }
            }
            else
            {
                SelectItem();
                motherboard.SAVESHOPDATA();
            }
        }
        else
        {
            motherboard.ShowNotification("Non enough credit");
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
                // Notify the PiecesCreator to set the team materials
                if (piecesCreator != null)
                {
                    piecesCreator.SetTeamMaterials(new Material[] { materialForTeam1, materialForTeam2 });
                    motherboard.SAVESHOPDATA();
                }

                isSelected = true;
                SelectedIcon.SetActive(true);
            }
        }
    }

    // Method to deselect this item
    public void DeselectItem()
    {
        isSelected = false;
        SelectedIcon.SetActive(false);
    }

    // New method to check if this item is selected
    public bool IsSelected()
    {
        return isSelected;
    }

    public string SaveItemData()
    {
        SavedData = $"{itemName},{price},{isPurchased},{isSelected}";
        return SavedData;
    }

    public void LoadItemData(string data)
    {
        string[] values = data.Split(',');

        itemName = values[0];
        price = float.Parse(values[1]);
        isPurchased = bool.Parse(values[2]);
        isSelected = bool.Parse(values[3]);

        nameText.text = itemName;
        priceText.text = isPurchased ? "Select" : "<size=35>NGN <size=50>" + price.ToString("F2");
        SelectedIcon.SetActive(isSelected);  // Set the visual state of the selected icon

        // If the item is selected, notify the PiecesCreator to set the team materials
        if (isSelected)
        {
            if (piecesCreator != null)
            {
                piecesCreator.SetTeamMaterials(new Material[] { materialForTeam1, materialForTeam2 });
            }
        }
    }

}
