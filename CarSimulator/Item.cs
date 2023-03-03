using System;

public class Item
{

	private int weight;
	private int volume;
	private string name;
	private int column;
	private int row;
	private int side;
	private int shelf;
	private int numberofthisitem;    // to record how many this item is stored in warehouse 
	private string id;
	public Item(int wei, int volume1, string str, string str1 )
	{
		weight = wei;
		volume = volume1;
		name = str;
		id = str1;

	}
	public void setlocation(int row1, int column1, int side1, int shelf1)
	{
		row = row1;
		column = column1;
		side = side1;
		shelf = shelf1;
	}

	public string getIteminfo()
	{
		//Console.WriteLine("{0}", name);
		return name;
		//Console.WriteLine("weight: {0} kg and volume: {1} m^3", weight, volume);
		//Console.WriteLine("location is at row :{0}, column:{1}, side:{2}, shelf:{3}", row, column, side, shelf);

	}

	//@return the row of item
	public int getrow()
	{
		return row;
	}
	//@return the column of item
	public int getcolumn()
	{
		return column;
	}

	//@return the side of item
	public int getside()
	{
		return side;
	}
	//@return the shelf of item
	public int getshelf()
	{
		return shelf;
	}
	//@return the name of item
	public string getname()
	{
		return name;
	}

	//@return the id of item
	public string getid()
	{
		return id;
	}

	//@return the weight of item
	public int getweight()
	{
		return weight;
	}

	//@return the volume of item
	public int getvolume()
	{
		return volume;
	}

	//increase the number of this item 
	public void increaseitemnum()
	{
		numberofthisitem++;
	}

	// decrease the number of this item
	public void decreaseitemnum()
	{
		numberofthisitem--;
	}

	//@return the number of this item
	public int getitemnum()
	{
		return numberofthisitem;
	}
	// set item number
	public void setnum(int num)
    {
		numberofthisitem = num;
    }
}