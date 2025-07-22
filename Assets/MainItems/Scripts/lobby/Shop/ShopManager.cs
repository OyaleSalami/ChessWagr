using UnityEngine;
using Firebase.Database;
using Firebase.Auth;
using System.Collections.Generic;
using System.Threading.Tasks;

public class ShopManager : MonoBehaviour
{
    public List<ShopItem> shopItems = new List<ShopItem>(); // List of all shop items
    public FirebaseAuth auth;
    public DatabaseReference DBreference;

    private ShopItem currentlySelectedItem; // To track the currently selected item
    private string saveKey = "SHOPID";      // Firebase child key for shop data

    void Start()
    {
        // Initialize Firebase Auth and DatabaseReference
        auth = FirebaseAuth.DefaultInstance;
        DBreference = FirebaseDatabase.DefaultInstance.RootReference;
    }

    // Method to save the shop data to Firebase
    public async Task SaveShopData()
    {
        FirebaseUser user = auth.CurrentUser;
        if (user != null)
        {
            List<string> savedData = new List<string>();

            foreach (var item in shopItems)
            {
                savedData.Add(item.SaveItemData());
            }

            string combinedData = string.Join(";", savedData);  // Combine all item data with semicolons
            Task DBTask = DBreference.Child("users").Child(user.UserId).Child(saveKey).SetValueAsync(combinedData);

            await DBTask;

            if (DBTask.IsCompleted)
            {
                Debug.Log("Shop data saved to Firebase!");
            }
            else
            {
                Debug.LogError("Failed to save shop data to Firebase.");
            }
        }
        else
        {
            Debug.LogError("No user is logged in. Cannot save shop data.");
        }
    }

    // Method to load the shop data from Firebase
    public async Task LoadShopData()
    {
        FirebaseUser user = auth.CurrentUser;
        if (user != null)
        {
            Task<DataSnapshot> DBTask = DBreference.Child("users").Child(user.UserId).Child(saveKey).GetValueAsync();

            await DBTask;

            if (DBTask.IsCompleted)
            {
                DataSnapshot snapshot = DBTask.Result;

                if (snapshot.Exists)
                {
                    string combinedData = snapshot.Value.ToString();
                    string[] loadedData = combinedData.Split(';');

                    for (int i = 0; i < shopItems.Count && i < loadedData.Length; i++)
                    {
                        shopItems[i].LoadItemData(loadedData[i]);

                        // Check if the item was previously selected
                        if (shopItems[i].IsSelected())
                        {
                            currentlySelectedItem = shopItems[i];
                        }
                    }

                    Debug.Log("Shop data loaded from Firebase!");
                }
                else
                {
                    Debug.LogWarning("No saved shop data found in Firebase.");
                }
            }
            else
            {
                Debug.LogError("Failed to load shop data from Firebase.");
            }
        }
        else
        {
            Debug.LogError("No user is logged in. Cannot load shop data.");
        }
    }

    // Method to clear saved data in Firebase
    public async Task ClearShopData()
    {
        FirebaseUser user = auth.CurrentUser;
        if (user != null)
        {
            Task DBTask = DBreference.Child("users").Child(user.UserId).Child(saveKey).RemoveValueAsync();
            await DBTask;

            if (DBTask.IsCompleted)
            {
                Debug.Log("Shop data cleared in Firebase.");
            }
            else
            {
                Debug.LogError("Failed to clear shop data in Firebase.");
            }
        }
        else
        {
            Debug.LogError("No user is logged in. Cannot clear shop data.");
        }
    }

    // Method to handle selecting an item, ensuring only one is selected at a time
    public void SelectItem(ShopItem itemToSelect)
    {
        // Deselect the currently selected item, if there is one
        if (currentlySelectedItem != null && currentlySelectedItem != itemToSelect)
        {
            currentlySelectedItem.DeselectItem(); // Call the Deselect method in ShopItem class
        }

        // Set the new item as selected
        currentlySelectedItem = itemToSelect;
    }
}
