using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TCSystem : MonoBehaviour
{
    // --- Player data class inside the shop script ---
    public class PlayerData
    {
        public int Coins;
        public Inventory Inventory;
        public SkillManager SkillManager;

        public PlayerData(int startingCoins)
        {
            Coins = startingCoins;
            Inventory = new Inventory();
            SkillManager = new SkillManager();
        }

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
        private Dictionary<ItemType, int> items = new Dictionary<ItemType, int>();

        public void AddItem(ItemType itemType)
        {
            if (!items.ContainsKey(itemType))
                items[itemType] = 0;
            items[itemType]++;
            Debug.Log($"Added {itemType} to inventory. Total: {items[itemType]}");
        }

        public bool HasItem(ItemType itemType)
        {
            return items.ContainsKey(itemType) && items[itemType] > 0;
        }

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
        private Dictionary<SkillType, Skill> skills = new Dictionary<SkillType, Skill>();

        public SkillManager()
        {
            skills[SkillType.Combat] = new Skill(SkillType.Combat, 5);
            skills[SkillType.MagicRange] = new Skill(SkillType.MagicRange, 5);
            skills[SkillType.ManaStorage] = new Skill(SkillType.ManaStorage, 5);
            skills[SkillType.HealthAmount] = new Skill(SkillType.HealthAmount, 5);
        }

        public int GetSkillPrice(SkillType skillType)
        {
            return skills[skillType].Price;
        }

        public int GetSkillLevel(SkillType skillType)
        {
            return skills[skillType].Level;
        }

        public void UpgradeSkill(SkillType skillType)
        {
            skills[skillType].Upgrade();
            Debug.Log($"{skillType} skill upgraded to level {skills[skillType].Level}");
            ApplySkillEffect(skillType);
        }

        private void ApplySkillEffect(SkillType skillType)
        {
            // Here you would apply real effects to player stats (e.g. damage, mana)
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
    public enum ItemType
    {
        HealthPotion,
        ManaPotion,
        XPCluster
    }

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

    public enum SkillType
    {
        Combat,
        MagicRange,
        ManaStorage,
        HealthAmount
    }

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

        public void Upgrade()
        {
            Level++;
            Price += 1; // Increment price per upgrade
        }
    }

    // --- Shop System variables ---

    private PlayerData playerData;

    private Item healthPotion = new Item(ItemType.HealthPotion, 3);
    private Item manaPotion = new Item(ItemType.ManaPotion, 4);
    private Item xpCluster = new Item(ItemType.XPCluster, 5);

    private bool playerInRange = false;

    private void Start()
    {
        // For demo, player starts with 20 coins
        playerData = new PlayerData(20);
    }

    private void Update()
    {
        if (playerInRange)
        {
            // Buy items
            if (Input.GetKeyDown(KeyCode.Alpha1))
                playerData.BuyItem(healthPotion);
            else if (Input.GetKeyDown(KeyCode.Alpha2))
                playerData.BuyItem(manaPotion);
            else if (Input.GetKeyDown(KeyCode.Alpha3))
                playerData.BuyItem(xpCluster);

            // Buy skills
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

    // Detect player entering the shop trigger zone
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            Debug.Log("Entered Shop! Press 1,2,3 for items; C,M,N,H for skills.");
            Debug.Log($"You have {playerData.Coins} coins.");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            Debug.Log("Left Shop.");
        }
    }
}
