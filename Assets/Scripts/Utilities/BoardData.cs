[System.Serializable]
public class BoardData {

	public string name, description;
	public int level, speed, turn, jump;
	public ItemCost boardCost;
	public string specialAbility;

}

[System.Serializable]
public class ItemCost {
	public int coins, bronzeTickets, silverTickets, goldTickets;
}
