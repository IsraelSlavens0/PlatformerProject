using System.Collections.Generic;
using UnityEngine;

public class TCSystem : MonoBehaviour
{
    // --- Player data class inside the shop script ---
    public class PlayerData
    {
        public int Coins; // Current number of coins the player has
        public Inventory Inventory; // Player's inventory for holding items
        public SkillManager SkillManager; // Player's skill manager

        public PlayerData(int startingCoins)
        {
            Coins = startingCoins;
            Inventory = new Inventory();
            SkillManager = new SkillManager();
        }

        // Attempt to buy an item; deduct coins and add to inventory if successful
        public bool BuyItem(Item item)
        {
            if (Coins >= item.Price)
            {
                Coins -= item.Price;
                Inventory.AddItem(item.Type);
                Debug.Log($"Bought {item.Type} for {item.Price} coins. Coins left: {Coins}");
                return true;
            }
            Debug.Log("Not enough coins.");
            return false;
        }

        // Attempt to upgrade a skill; deduct coins and upgrade skill if successful
        public bool BuySkill(SkillType skillType)
        {
            int price = SkillManager.GetSkillPrice(skillType);
            if (Coins >= price)
            {
                Coins -= price;
                SkillManager.UpgradeSkill(skillType);
                Debug.Log($"Upgraded {skillType} skill for {price} coins. Coins left: {Coins}");
                return true;
            }
            Debug.Log("Not enough coins.");
            return false;
        }
    }

    // --- Inventory ---
    public class Inventory
    {
        // Dictionary to store item quantities by type
        private Dictionary<ItemType, int> items = new Dictionary<ItemType, int>();

        // Add item to inventory or increase quantity
        public void AddItem(ItemType itemType)
        {
            if (!items.ContainsKey(itemType))
                items[itemType] = 0;
            items[itemType]++;
            Debug.Log($"Added {itemType} to inventory. Total: {items[itemType]}");
        }

        // Check if item is available in inventory
        public bool HasItem(ItemType itemType)
        {
            return items.ContainsKey(itemType) && items[itemType] > 0;
        }

        // Use one unit of an item if available
        public bool UseItem(ItemType itemType)
        {
            if (HasItem(itemType))
            {
                items[itemType]--;
                Debug.Log($"Used one {itemType}. Remaining: {items[itemType]}");
                return true;
            }
            Debug.Log($"No {itemType} left to use!");
            return false;
        }
    }

    // --- Skill Manager ---
    public class SkillManager
    {
        // Dictionary of player skills by type
        private Dictionary<SkillType, Skill> skills = new Dictionary<SkillType, Skill>();

        public SkillManager()
        {
            // Initialize each skill with a starting price
            skills[SkillType.Combat] = new Skill(SkillType.Combat, 5);
            skills[SkillType.MagicRange] = new Skill(SkillType.MagicRange, 5);
            skills[SkillType.ManaStorage] = new Skill(SkillType.ManaStorage, 5);
            skills[SkillType.HealthAmount] = new Skill(SkillType.HealthAmount, 5);
        }

        // Get the price to upgrade a specific skill
        public int GetSkillPrice(SkillType skillType)
        {
            return skills[skillType].Price;
        }

        // Get current level of a specific skill
        public int GetSkillLevel(SkillType skillType)
        {
            return skills[skillType].Level;
        }

        // Upgrade the skill and apply its effects
        public void UpgradeSkill(SkillType skillType)
        {
            skills[skillType].Upgrade();
            Debug.Log($"{skillType} skill upgraded to level {skills[skillType].Level}");
            ApplySkillEffect(skillType);
        }

        // Simulate applying the skill's effect (would update actual player stats in real game)
        private void ApplySkillEffect(SkillType skillType)
        {
            switch (skillType)
            {
                case SkillType.Combat:
                    Debug.Log("Combat skill upgraded: attack speed, damage, sweeping edge distance improved.");
                    break;
                case SkillType.MagicRange:
                    Debug.Log("Magic range skill upgraded: increased range and damage.");
                    break;
                case SkillType.ManaStorage:
                    Debug.Log("Mana storage skill upgraded: increased max mana.");
                    break;
                case SkillType.HealthAmount:
                    Debug.Log("Health amount skill upgraded: increased max health.");
                    break;
            }
        }
    }

    // --- Data Models ---

    // Types of items available in the shop
    public enum ItemType
    {
        HealthPotion,
        ManaPotion,
        XPCluster
    }

    // Class representing an item with a type and a price
    public class Item
    {
        public ItemType Type;
        public int Price;

        public Item(ItemType type, int price)
        {
            Type = type;
            Price = price;
        }
    }

    // Types of skills the player can upgrade
    public enum SkillType
    {
        Combat,
        MagicRange,
        ManaStorage,
        HealthAmount
    }

    // Class representing a skill with level and price
    public class Skill
    {
        public SkillType Type;
        public int Level;
        public int Price;

        public Skill(SkillType type, int startingPrice)
        {
            Type = type;
            Level = 0;
            Price = startingPrice;
        }

        // Increase skill level and raise the price for the next upgrade
        public void Upgrade()
        {
            Level++;
            Price += 1; // Increment price per upgrade
        }
    }

    // --- Shop System variables ---

    private PlayerData playerData; // Holds player coins, inventory, and skills

    // Define purchasable items with set prices
    private Item healthPotion = new Item(ItemType.HealthPotion, 3);
    private Item manaPotion = new Item(ItemType.ManaPotion, 4);
    private Item xpCluster = new Item(ItemType.XPCluster, 5);

    private bool playerInRange = false; // Is player near the shop?

    private void Start()
    {
        // Initialize player with 20 coins at start
        playerData = new PlayerData(20);
    }

    private void Update()
    {
        if (playerInRange)
        {
            // Item purchase inputs
            if (Input.GetKeyDown(KeyCode.Alpha1))
                playerData.BuyItem(healthPotion);
            else if (Input.GetKeyDown(KeyCode.Alpha2))
                playerData.BuyItem(manaPotion);
            else if (Input.GetKeyDown(KeyCode.Alpha3))
                playerData.BuyItem(xpCluster);

            // Skill upgrade inputs
            else if (Input.GetKeyDown(KeyCode.C))
                playerData.BuySkill(SkillType.Combat);
            else if (Input.GetKeyDown(KeyCode.M))
                playerData.BuySkill(SkillType.MagicRange);
            else if (Input.GetKeyDown(KeyCode.N))
                playerData.BuySkill(SkillType.ManaStorage);
            else if (Input.GetKeyDown(KeyCode.H))
                playerData.BuySkill(SkillType.HealthAmount);
        }
    }

    // Detect player entering the shop area
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            Debug.Log("Entered Shop! Press 1,2,3 for items; C,M,N,H for skills.");
            Debug.Log($"You have {playerData.Coins} coins.");
        }
    }

    // Detect player leaving the shop area
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            Debug.Log("Left Shop.");
        }
    }
}
