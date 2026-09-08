using UnityEngine;
using UnityEditor;

[CreateAssetMenu(menuName = "New Gun")]
public class Gun : Weapon
{
    public override void Use()
    {
        Debug.Log("Bang!");
    }
}
