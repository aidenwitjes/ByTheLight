using UnityEngine;
using System.Collections.Generic;

// Simple inventory system for managing items
public class PlayerInventory : MonoBehaviour
{
    public static PlayerInventory Instance { get; private set; }

    [Header("Inventory Contents")]
    [SerializeField] private bool hasFlashlight = false;
    [SerializeField] private bool hasMatchbox = false;
    [SerializeField] private bool hasKey = false;
    // Add more items as needed

    [Header("Debug")]
    [SerializeField] private bool showDebugLog = true;

    // Events for UI updates (optional)
    public System.Action<string> OnItemAdded;
    public System.Action<string> OnItemRemoved;

    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Generic method to check if player has an item
    public bool HasItem(string itemName)
    {
        return itemName.ToLower() switch
        {
            "flashlight" => hasFlashlight,
            "matchbox" => hasMatchbox,
            "key" => hasKey,
            _ => false
        };
    }

    // Add an item to inventory
    public bool AddItem(string itemName)
    {
        bool wasAdded = false;

        switch (itemName.ToLower())
        {
            case "flashlight":
                if (!hasFlashlight)
                {
                    hasFlashlight = true;
                    wasAdded = true;
                }
                break;
            case "matchbox":
                if (!hasMatchbox)
                {
                    hasMatchbox = true;
                    wasAdded = true;
                }
                break;
            case "key":
                if (!hasKey)
                {
                    hasKey = true;
                    wasAdded = true;
                }
                break;
        }

        if (wasAdded)
        {
            if (showDebugLog)
                Debug.Log($"Added {itemName} to inventory");

            OnItemAdded?.Invoke(itemName);
        }
        else if (showDebugLog)
        {
            Debug.Log($"Already have {itemName}");
        }

        return wasAdded;
    }

    // Remove an item from inventory (for consumable items)
    public bool RemoveItem(string itemName)
    {
        bool wasRemoved = false;

        switch (itemName.ToLower())
        {
            case "flashlight":
                if (hasFlashlight)
                {
                    hasFlashlight = false;
                    wasRemoved = true;
                }
                break;
            case "matchbox":
                if (hasMatchbox)
                {
                    hasMatchbox = false;
                    wasRemoved = true;
                }
                break;
            case "key":
                if (hasKey)
                {
                    hasKey = false;
                    wasRemoved = true;
                }
                break;
        }

        if (wasRemoved)
        {
            if (showDebugLog)
                Debug.Log($"Removed {itemName} from inventory");

            OnItemRemoved?.Invoke(itemName);
        }

        return wasRemoved;
    }

    // Get all items player currently has
    public List<string> GetAllItems()
    {
        List<string> items = new List<string>();

        if (hasFlashlight) items.Add("Flashlight");
        if (hasMatchbox) items.Add("Matchbox");
        if (hasKey) items.Add("Key");

        return items;
    }

    // Debug method to show current inventory
    [ContextMenu("Show Inventory")]
    public void ShowInventory()
    {
        var items = GetAllItems();
        if (items.Count > 0)
        {
            Debug.Log("Current inventory: " + string.Join(", ", items));
        }
        else
        {
            Debug.Log("Inventory is empty");
        }
    }
}