using UnityEngine;

[System.Serializable, CreateAssetMenu]
public class Accessory : ScriptableObject
{
    public string accessoryName, description;
    public enum BodyPart { Hat, Shirt, Pants, Shoes, Shape, Face, Skin };
    public BodyPart bodyPart;
    public int shopIndex, itemID;
    public ItemCost itemCost;

}
