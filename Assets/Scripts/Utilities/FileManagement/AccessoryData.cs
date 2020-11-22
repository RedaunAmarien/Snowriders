[System.Serializable]
public class AccessoryData {

	public string name, description;
	public enum BodyPart {Hat, Shirt, Pants, Shoes, Shape, Face, Skin};
	public BodyPart bodyPart;
    public int shopIndex, itemID;
	public ItemCost itemCost;

}
