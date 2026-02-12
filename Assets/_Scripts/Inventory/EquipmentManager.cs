using UnityEngine;

public class EquipmentManager : MonoBehaviour
{
    public static EquipmentManager Instance { get; private set; }

    [Header("Equipped Items")]
    [SerializeField] private ItemDefinition equippedWeapon;
    [SerializeField] private ItemDefinition equippedArmor;

    public ItemDefinition EquippedWeapon => equippedWeapon;
    public ItemDefinition EquippedArmor => equippedArmor;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void EquipWeapon(ItemDefinition weapon)
    {
        if (weapon == null)
        {
            Debug.LogWarning("Tried to equip a null weapon.");
            return;
        }

        equippedWeapon = weapon;
        Debug.Log("Equipped weapon: " + weapon.itemName);
    }

    public void UnequipWeapon()
    {
        if (equippedWeapon == null) return;

        Debug.Log("Unequipped weapon: " + equippedWeapon.itemName);
        equippedWeapon = null;
    }

    public void EquipArmor(ItemDefinition armor)
    {
        if (armor == null)
        {
            Debug.LogWarning("Tried to equip a null armor.");
            return;
        }

        equippedArmor = armor;
        Debug.Log("Equipped armor: " + armor.itemName);
    }

    public void UnequipArmor()
    {
        if (equippedArmor == null) return;

        Debug.Log("Unequipped armor: " + equippedArmor.itemName);
        equippedArmor = null;
    }

    public bool IsEquipped(ItemDefinition item)
    {
        if (item == null) return false;
        return item == equippedWeapon || item == equippedArmor;
    }

    /// <summary>Used by save/load to restore equipped weapon without triggering normal equip flow.</summary>
    public void SetEquippedWeapon(ItemDefinition weapon)
    {
        equippedWeapon = weapon;
        if (weapon != null)
            Debug.Log("Restored equipped weapon: " + weapon.itemName);
    }

    /// <summary>Used by save/load to restore equipped armor without triggering normal equip flow.</summary>
    public void SetEquippedArmor(ItemDefinition armor)
    {
        equippedArmor = armor;
        if (armor != null)
            Debug.Log("Restored equipped armor: " + armor.itemName);
    }
}

