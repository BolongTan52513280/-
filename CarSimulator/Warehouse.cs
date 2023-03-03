using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;


// 4 dimension ware house  row;column;side;shelf
// all items is stored in the warehouse 
// item has been required by customers will be grabed by robot first and then put in the truck 
public class Warehouse
{
	public int col;
	public int row;
	public int side;
	public int shelf;
	public Item[,,,] spot;
	//public int numberoflocation;
	//public int sparelocation;
	

	// constructor 
	public Warehouse(int column1, int row1, int side1, int shelf1)
	{

		col = column1;
		row = row1;
		side = side1;
		shelf = shelf1;
		spot = new Item[column1, row1, side1, shelf1];
	}
	//@return return the number of items in the warehouse 
	public int numberofitem()
	{
		int num = 0;
		for (int i = 0; i < col; i++)
		{
			for (int j = 0; j < row; j++)
			{
				for (int k = 0; k < side; k++)
				{
					for (int z = 0; z < shelf; z++)
					{
						if (spot[i, j, k, z] != null)
							num++;

					}
				}
			}
		}
		return num;
	}


	// print out all the items in the warehouse 
	public void showItem()
	{
		int arduino = 0;
		int hpprime = 0;
		int multi = 0;
		int iclick = 0;
		int py = 0;
		int de1 = 0;
		for (int i = 0; i < col; i++)
		{
			for (int j = 0; j < row; j++)
			{
				for (int k = 0; k < side; k++)
				{
					for (int z = 0; z < shelf; z++)
					{
						if (spot[i, j, k, z] != null)
                        {
							String temp = spot[i, j, k, z].getIteminfo();

							if(string.Equals(temp, "Arduino"))
                            {
								arduino++;
                            }

							else if (string.Equals(temp, "Hp-prime"))
							{
								hpprime++;
							}
							else if (string.Equals(temp, "Multimeter"))
							{
								multi++;
							}
							else if (string.Equals(temp, "PYNQ-z1"))
							{
								py++;

							}
							else if (string.Equals(temp, "Icliker"))
							{
								iclick++;
							}
							else if (string.Equals(temp, "DE1-SoC"))
							{
								de1++;
							}

						}
						
						
						
							
							
					}
				}
			}
		}
		Console.WriteLine("Arduino: {0}", arduino);
		Console.WriteLine("Multimeter: {0}", multi);
		Console.WriteLine("iClicker: {0}", iclick);
		Console.WriteLine("DE1_SoC: {0}", de1);
		Console.WriteLine("PYNQ-z1: {0}", py);
		Console.WriteLine("HP-Prime: {0}", hpprime);
	}

	public void showItemManual()
	{

		for (int i = 0; i < col; i++)
		{
			for (int j = 0; j < row; j++)
			{
				for (int k = 0; k < side; k++)
				{
					for (int z = 0; z < shelf; z++)
					{
						if (spot[i, j, k, z] != null)
						{
							Console.WriteLine(spot[i, j, k, z].getname());

							

						}





					}
				}
			}
		}
		
	}

	// add item to a specific location 
	//@return true if added sucessfully, false otherwise 
	public Boolean additemtospecificlocation(Item item)
	{
		if (spot[item.getcolumn(), item.getrow(), item.getside(), item.getshelf()] == null)
		{
			spot[item.getcolumn(), item.getrow(), item.getside(), item.getshelf()] = item;
			Console.WriteLine("The itme is added to warehouse successfully");
			return true;
		}
		else
		{
			Console.WriteLine("The specific spot has been occupied please choose another location");
			return false;
		}
	}

	// check if the specific location in  warehosue has been occupied 
	// return true if the spot is free 
	public Boolean checkspecificshelf(int x, int y, int LoR, int shelf)
	{
		if (spot[x, y, LoR, shelf] == null)
		{
			Console.WriteLine("There is no item at entered location only adding item could be done");
			return true;
		}
		else
		{
			Console.WriteLine("This specific spot has been occupied only take item could be done");
			return false;
		}
	}

	// add item to a specific locaton 
	//@return true if added successfully 
	public Boolean addItemToLocation(int x, int y, int LoR, int shelf, Item newItem)
	{
		if (spot[x, y, LoR, shelf] == null)
		{
			spot[x, y, LoR, shelf] = newItem;
			Console.WriteLine("The item is added to the warehouse successfully");
			return true;
		}
		else
		{
			Console.WriteLine("This specific spot has been occupied please choose another location");
			return false;
		}
	}

	// remove item to a specific locaton 
	//@return true if removed successfully 
	public Item removeItemFromLocation(int x, int y, int LoR, int shelf)
	{
		Item removedItem = spot[x, y, LoR, shelf];
		spot[x, y, LoR, shelf] = null;
		if (removedItem != null)
		{
			Console.WriteLine("item " + removedItem.getname() + " removed from shelf");
		}
		return removedItem;

	}

	// add item to any spare location 
	// return where is it added
	public Item Additem(Item item)
	{
		if (locationleft() != 0)
		{
			for (int i = 0; i < col; i++)
			{
				for (int j = 0; j < row; j++)
				{
					for (int k = 0; k < side; k++)
					{
						for (int z = 0; z < shelf; z++)
						{
							if (spot[i, j, k, z] != null)
							{
								spot[i, j, k, z] = item;
								item.setlocation(i, j, k, z);
								return item;
							}

						}
					}
				}
			}
		}


		Console.WriteLine("warehouse is already full");
		return null;


	}

	// return the number of the location can store item in the warehouse 
	public int overalllocation()
	{
		return col * row * side * shelf;
	}

	//return the number of location can still accept new item
	public int locationleft()
	{
		return overalllocation() - numberofitem();
	}
	// find spcific item in ware hosue 
	//@return the specified item 
	public Item findItem(string name)
	{
		for (int i = 0; i < col; i++)
		{
			for (int j = 0; j < row; j++)
			{
				for (int k = 0; k < side; k++)
				{
					for (int z = 0; z < shelf; z++)
					{

						if (spot[i, j, k, z] != null)
						{
							if (string.Equals(spot[i, j, k, z].getname(), name))
							{

								return spot[i, j, k, z];
							}
						}

					}
				}
			}
		}
		Console.WriteLine("Item is not found");
		return null;
	}

	// find spcific item in ware hosue 
	//@return the specified item 
	public Item findid(string id)
	{
		for (int i = 0; i < col; i++)
		{
			for (int j = 0; j < row; j++)
			{
				for (int k = 0; k < side; k++)
				{
					for (int z = 0; z < shelf; z++)
					{

						if (spot[i, j, k, z] != null)
						{
							if (string.Equals(spot[i, j, k, z].getid(), id))
							{

								return spot[i, j, k, z];
							}
						}

					}
				}
			}
		}
		Console.WriteLine("Item is not found");
		return null;
	}

	public Item[,,,] getspot()
	{
		return spot;
	}


	// get the list of all items in warehouse 
	//@return a list of item stored in warehouse    contians info about number of a same item stored in warehouse
	public List<Item> getstockinfo()
	{

	  List<Item> items = new List<Item>();
	    int sameitem = 0;


		for (int l = 0; l < items.Count; l++)
		{
			
				items[l].setnum(0);

		}

		for (int i = 0; i < col; i++)
		{
			for (int j = 0; j < row; j++)
			{
				for (int k = 0; k < side; k++)
				{
					for (int z = 0; z < shelf; z++)
					{

						if (spot[i, j, k, z] != null)
						{
							for (int l = 0; l < items.Count; l++)
							{
								if (string.Equals(spot[i, j, k, z].getname(), items[l].getname()))
								{
									
									items[l].increaseitemnum();
									sameitem = 1;
								}

							}
							if (sameitem == 0)
							{
								items.Add(spot[i, j, k, z]);
							}
						}


						sameitem = 0;

					}
				}
			}
		}
		

		for (int l = 0; l < items.Count; l++)  // the first time recognize items we did not add 1 
		{
			items[l].increaseitemnum();
		}

		return items;
	}

	
}
