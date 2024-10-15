using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class InterfaceManager : MonoBehaviour
{
    public TextMeshProUGUI resourceText;
    public TextMeshProUGUI progressText;

    private Dictionary<string, int> resourceAmounts;

    private string part1 = "<sprite name=\"";
    private string part2 = "\">";

    // Reference to the DialogueBox and the DialogueScript
    public GameObject dialogueBox;
    private DialogueScript dialogueScript;

    private bool isCrowInRange = false;  // Flag to check if crow is nearby

    private void Start()
    {
        // Initialize resourceAmounts dictionary
        resourceAmounts = new Dictionary<string, int>()
        {
            { "Wood", 0 },
            { "Stone", 0 },
            { "Coal", 0 },
            { "Metal", 0 },
            { "Fiber", 0 }
        };

        // Initialize the DialogueBox and its script
        dialogueScript = dialogueBox.GetComponent<DialogueScript>();
        dialogueBox.SetActive(false); // Make sure DialogueBox is initially inactive

        // Update resource amounts and display
        UpdateResourceAmounts();
        DisplayResources();
        DisplayStats();
        DisplayProgress();
    }

    private void Update()
    {
        // Optionally update the resources in real-time
        UpdateResourceAmounts();
        DisplayResources();
        DisplayStats();
        DisplayProgress();

        // Check for the "E" key press to interact with the crow
        if (isCrowInRange && Input.GetKeyDown(KeyCode.E))
        {
            if (!dialogueBox.activeInHierarchy)
            {
                // If the DialogueBox is not active, activate it and start dialogue
                dialogueBox.SetActive(true);
            }
        }
    }

    private void UpdateResourceAmounts()
    {
        // Retrieve the inventory instance
        Inventory inventory = Inventory.Instance;

        // Update the quantities of each resource from the inventory
        foreach (CollectableClass item in inventory.items)
        {
            if (resourceAmounts.ContainsKey(item.itemName))
            {
                resourceAmounts[item.itemName] = item.quantity;
            }
        }
    }

    private void DisplayResources()
    {
        // Clear the resourceText before displaying resources
        resourceText.text = "";

        // Loop through each resource and add it to the resourceText
        foreach (var resource in resourceAmounts)
        {
            resourceText.text += resource.Value + "x " + part1 + resource.Key + part2 + "   ";
        }
    }

    private void DisplayStats()
    {
        Inventory inventory = Inventory.Instance;
        // Clear the resourceText before displaying resources
    }

    private void DisplayProgress()
    {
        float currentProgress = ProgressTracker.Instance.GetProgress();
        currentProgress = currentProgress * 100;
        currentProgress = Mathf.Round(currentProgress);
        progressText.text = "Progress: " + currentProgress.ToString() + "%";
    }

    // Call this when a crow enters the trigger
    public void OnCrowEnter()
    {
        isCrowInRange = true;
    }

    // Call this when a crow exits the trigger
    public void OnCrowExit()
    {
        isCrowInRange = false;
    }



   

}
