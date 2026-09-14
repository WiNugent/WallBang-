using UnityEngine;

public abstract class Weapon : ScriptableObject
{
    [SerializeField] protected string weaponName;
    [SerializeField] protected int damage;
    [SerializeField] protected float fireRate;
    [SerializeField] protected bool isHoldToShoot;
    [SerializeField] protected float maxPenetrationDepth;
    [SerializeField] protected float maxShootingDistance;

    public abstract void Use();

    public int GetDamageValue() { return damage; }

    public float GetFireRateValue() { return fireRate; }

    public bool GetIsHoldToShoot() { return isHoldToShoot; }

    public string GetWeaponName() { return weaponName; }

    public float GetMaxPenetrationDepth() { return maxPenetrationDepth; }
    
}
