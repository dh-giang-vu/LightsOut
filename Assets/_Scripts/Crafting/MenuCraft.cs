using UnityEngine;

public class MenuCraft : MonoBehaviour
{
    // Serialized fields for material requirements, configurable per GameObject
    [SerializeField] private GameObject objectToMove;
    [SerializeField] private int requiredWood = 0;
    [SerializeField] private int requiredCoal = 0;
    [SerializeField] private int requiredStone = 0;
    [SerializeField] private int requiredMetal = 0;
    [SerializeField] private int requiredFiber = 0;

    [SerializeField] private LightClass itemToCraft; // This is now of type LightClass (ScriptableObject)
    private float itemPlaceDistance = 30.0f;

    private Inventory inventory;  // Reference to the Inventory singleton
    private bool isPlacingItem = false;
    private GameObject instantiatedItem = null;
    private LayerMask instantiatedItemLayerMask;

    // Material handling variables
    [SerializeField] private Material craftingMaterial; // The material to apply while crafting
    private Material originalMaterial; // Store the original material

    // Cloud effect when placing items
    [SerializeField] private ParticleSystem cloudParticleSystem;
    [SerializeField] private Vector3 defaultCloudSize = new Vector3(1.0f, 1.0f, 1.0f);
    [SerializeField] private float cloudScaling = 0.25f;

    // Reference to the CraftUIMove script to control the UI movement
    [SerializeField] private CraftUIMove craftUIMove;

    private void Start()
    {
        // Get the singleton instance of the Inventory
        inventory = Inventory.Instance;

        // Ensure the inventory instance is valid
        if (inventory == null)
        {
            Debug.LogError("Inventory singleton instance is null. Ensure Inventory is instantiated.");
        }

        // Make sure itemToCraft is assigned
        if (itemToCraft == null)
        {
            Debug.LogError("No LightClass item assigned for crafting!");
        }

        instantiatedItemLayerMask = LayerMask.NameToLayer("Default");

        // Ensure the CraftUIMove script is assigned
        if (craftUIMove == null)
        {
            Debug.LogError("CraftUIMove reference is missing. Assign the script in the inspector.");
        }
    }

    // Method to handle crafting the item and starting placement
    public void CraftAndPlaceItem()
    {
        // Check if enough materials exist for crafting
        if (HasRequiredMaterials())
        {
            UseRequiredMaterials(); // Consume the materials
            StartPlacingItem();      // Start placing the item

            // Call ToggleObjectPosition from CraftUIMove to move the UI element
            craftUIMove.ToggleObjectPosition();
        }
        else
        {
            Debug.LogWarning("Not enough materials to craft the item.");
        }
    }

    // Check if the inventory has the required materials
    private bool HasRequiredMaterials()
    {
        return inventory.woodAmount >= requiredWood &&
               inventory.stoneAmount >= requiredStone &&
               inventory.coalAmount >= requiredCoal &&
               inventory.metalAmount >= requiredMetal &&
               inventory.fiberAmount >= requiredFiber;
    }

    // Consume the required materials from the inventory
    private void UseRequiredMaterials()
    {
        // Find and reduce the quantities in the inventory
        CollectableClass wood = inventory.items.Find(item => item.itemName == "Wood");
        CollectableClass stone = inventory.items.Find(item => item.itemName == "Stone");
        CollectableClass coal = inventory.items.Find(item => item.itemName == "Coal");
        CollectableClass metal = inventory.items.Find(item => item.itemName == "Metal");
        CollectableClass fiber = inventory.items.Find(item => item.itemName == "Fiber");

        if (wood != null) wood.quantity -= requiredWood;
        if (stone != null) stone.quantity -= requiredStone;
        if (coal != null) coal.quantity -= requiredCoal;
        if (metal != null) metal.quantity -= requiredMetal;
        if (fiber != null) fiber.quantity -= requiredFiber;

        // Update material counts in the Inventory
        inventory.UpdateMaterialCounts();
    }

    // Method to start placing the crafted item
    private void StartPlacingItem()
    {
        // Ensure the item to craft has a model assigned
        if (itemToCraft != null && itemToCraft.model != null)
        {
            isPlacingItem = true;

            // Change the material of the instantiated item
            if (instantiatedItem == null)
            {
                instantiatedItem = Instantiate(itemToCraft.model, Vector3.zero, Quaternion.identity); // Instantiate at a temporary position
                originalMaterial = instantiatedItem.GetComponent<Renderer>().material; // Store original material
                instantiatedItem.GetComponent<Renderer>().material = craftingMaterial; // Apply new material
                instantiatedItemLayerMask = instantiatedItem.layer;
                instantiatedItem.layer = LayerMask.NameToLayer("NoCollision");
            }
        }
        else
        {
            Debug.LogError("LightClass item or its model is not assigned!");
        }
    }

    private void Update()
    {
        // Check if player is placing an item
        if (isPlacingItem && itemToCraft != null && itemToCraft.model != null)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition); // Use the main camera
            Debug.DrawRay(ray.origin, ray.direction * 10, Color.yellow);

            if (Physics.Raycast(ray, out RaycastHit hit, itemPlaceDistance, LayerMask.GetMask("TerrainLayer")))
            {
                Vector3 placePosition = hit.point;

                // If the item hasn't been instantiated yet, instantiate it at the hit point
                if (instantiatedItem == null)
                {
                    Debug.LogWarning("Item not instantiated");
                }
                else
                {
                    instantiatedItem.transform.position = placePosition;  // Update its position as the player moves the cursor
                }
            }

            // If the player clicks the mouse button, finalize placement
            if (Input.GetKeyDown(KeyCode.Mouse0) && instantiatedItem != null)
            {
                StopPlacingItem();
            }
        }
    }

    // Method to stop placing the item
    private void StopPlacingItem()
    {
        isPlacingItem = false;
        instantiatedItem.layer = instantiatedItemLayerMask;

        // Revert back to the original material
        if (instantiatedItem != null)
        {
            instantiatedItem.GetComponent<Renderer>().material = originalMaterial;
        }

        // Play cloud particle system, size adjusted to item being placed
        if (instantiatedItem.TryGetComponent<Renderer>(out var instantiatedItemRenderer))
        {
            Vector3 size = instantiatedItemRenderer.bounds.size;
            cloudParticleSystem.transform.localScale = size * cloudScaling;
        }
        else
        {
            cloudParticleSystem.transform.localScale = defaultCloudSize;
        }
        cloudParticleSystem.transform.position = instantiatedItem.transform.position;
        cloudParticleSystem.Play();

        instantiatedItem = null;  // Clear the reference so no further updates happen
    }
}
