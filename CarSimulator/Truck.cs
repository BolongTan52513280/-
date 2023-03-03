using System.Collections.Generic;
using System;

public class Truck
{
    private int max_weight;
    private int max_capacity;
    private int status;         //0 = waiting, 1 = docked, 2 = leaving
    private int total_items;
    private List<Item> cargo = new List<Item>();
    private int queue_location;
    private int truck_type;  //0 = restock, 1 = delivery
    private bool is_full;

    public Truck()
    {
        max_weight = 1000; //random place holder weight
        max_capacity = 500; //random place holder volume
        status = 0;
        total_items = 1;
        Item eggs = new Item(200, 100, "eggs","1");
        cargo.Add(eggs);
        queue_location = 0;
    }

    public Truck(int weight, int volume, int stat, int type)
    {
        max_weight = weight;
        max_capacity = volume;
        status = stat;
        //cargo = null;
        total_items = 0;
        truck_type = type;
        queue_location = 0;

    }

    //set cargo ignoring restrictions
    //@parama list of item in truck 

    public void setCargo(List<Item> new_cargo)
    {
        cargo = new_cargo;
        total_items = cargo.Count;
    }

    //set the truck weight limit
    //@parama the max weight of truck 
    public void setMaxWeight(int new_max)
    {
        max_weight = new_max;
    }

    //set the truck volume limit
    //@parama the max weight of truck 
    public void setMaxVolume(int new_max)
    {
        max_capacity = new_max;
    }

    //check cargo
    //@return cargo list 
    public List<Item> checkCargo()
    {
        return cargo;
    }

    //dock the truck if available spot and truck is at front of queue
    //@parama boolean to represnet if the truck is free
    public void dock(bool available)
    {
        if (available && status == 0 && queue_location == 0)
        {
            status = 1;
        }
        else
        {
            Console.WriteLine("failed to dock, check availability of dock or spot of truck");
        }
    }

    //check if docked
    //@return if the truck is docked 
    public bool isDocked()
    {
        if (status == 1)
        {
            return true;
        }
        return false;
    }

    //load a full list of items or unload the entire truck

    //load a list of items, only use this if truck is empty!!
    //@parama the list of items needed to be put into truck 
    public void loadManyItem(List<Item> new_items)
    {
        if (status == 1)
        {
            var old_items = cargo;
            cargo = new_items;

            if (checkWeight() > max_weight)
            {
                cargo = old_items;
                Console.WriteLine("items not loaded: shipment exceeds maximum weight capacity");
            }

            if (checkVolume() > max_capacity)
            {
                cargo = old_items;
                Console.WriteLine("items not loaded: shipment exceeds maximum space capacity");
            }

            total_items = cargo.Count;
        }
        else
        {
            Console.WriteLine("item not loaded: truck not docked");
        }
    }
    // unload all items on teh truck 
    //@return the list of itms has been unloaded 
    public List<Item> unloadAllItems()
    {
        if (status == 1)
        {
            var unloaded_items = cargo;
            foreach (var item in cargo)
            {
                cargo.Remove(item);
            }
            total_items = cargo.Count;
            return unloaded_items;
        }
        else
        {
            Console.WriteLine("item not unloaded: truck not docked");
        }

        return null;
    }

    //load item
    //@parama list of item should be loaded to the car 
    public void loadItem(Item new_item)
    {
        if (status == 1)
        {
            if (checkWeight() + new_item.getweight() > max_weight)
            {
                Console.WriteLine("item not loaded: exceeds maximum weight capacity");
                is_full = true;

            }
            else if (checkVolume() + new_item.getvolume() > max_capacity)
            {
                Console.WriteLine("item not loaded: exceeds maximum space capacity");
                is_full = true;
            }
            else
            {
                cargo.Add(new_item);
                total_items++;
            }

        }
        else
        {
            Console.WriteLine("item not loaded: truck not docked");
        }
    }

    //unload item
    //note if you remove an item from the middle of the list then all other items after it
    //move down by 1 so you need to keep track of where everything is
    //the alternative is to just always remove the last item or always remove the first item
    public Item unloadItem(int item_number)
    {
        if (status == 1)
        {
            if (item_number < total_items)
            {

                var removed_item = checkItem(item_number);
                cargo.RemoveAt(item_number);
                total_items--;
                Console.WriteLine("item " + removed_item.getname() + " removed from truck");
                return removed_item;
            }
            else
            {
                Console.WriteLine("no more items in truck");
                //Console.WriteLine("item not unloaded: item at location does not exist");
            }
        }
        else
        {
            Console.WriteLine("item not unloaded: truck not docked");
        }

        return null;
    }

    private Item checkItem(int item_number)
    {
        return cargo[item_number];
    }

    //check weight, returns weight of items in truck
    public int checkWeight()
    {
        int current_weight = 0;
        if (cargo.Count != 0)
        {
            foreach (var Item in cargo)
            {
                current_weight = current_weight + Item.getweight();
            }
        }
        return current_weight;
    }

    //check volume, returns volume of items in truck
    public int checkVolume()
    {
        int current_volume = 0;

        foreach (var Item in cargo)
        {
            current_volume = current_volume + Item.getvolume();
        }

        return current_volume;
    }

    //computer can tell truck to stop loading right now because it is out of time
    //return 1 for truck leaving
    public int finishLoading(bool outOfTime)
    {
        if (is_full)
        {
            return depart();
        }

        if (outOfTime)
        {
            return depart();
        }

        Console.WriteLine("no reason to leave yet");
        return 0;
    }

    //depart, 1 means it is leaving
    private int depart()
    {
        status = 2;
        queue_location = -1;    //signify truck is no longer in queue meaning it is gone
        Console.WriteLine("this truck is leaving, dock now available");
        return 1;
    }

    //check status of truck
    public int checkStatus()
    {
        if (status == 0)
        {
            Console.WriteLine("this truck is waiting");
        }
        else if (status == 1)
        {
            Console.WriteLine("this truck is docked");
        }
        else
        {
            Console.WriteLine("this truck has departed");
        }

        return status;
    }

    //get useful info about the truck
    public void getTruckInfo()
    {
        int remaining = max_weight - checkWeight();
        Console.WriteLine("Truck has a maximum weight load of " + max_weight + "Kg");
        Console.WriteLine("Truck can carry an additional" + remaining + "Kg");

        remaining = max_capacity - checkVolume();
        Console.WriteLine("Truck has a maximum volume of " + max_capacity + "m^3");
        Console.WriteLine("Truck an additional" + remaining + "m^3 of space remaining");

        Console.WriteLine("The truck currently has " + total_items + " items loaded");

        checkStatus();
        if (status == 0)
        {
            Console.WriteLine("currently in queue at spot " + checkQueue());
        }
    }

    //check queue location
    public int checkQueue()
    {
        return queue_location;
    }

    //move up queue
    public void moveUpQueue()
    {
        if (queue_location > 0)
        {
            queue_location--;
        }
        else
        {
            if (queue_location < 0)
            {
                Console.WriteLine("truck departed");
            }
            else
            {
                Console.WriteLine("already at front of queue");
            }
        }
    }

    //check if truck is full
    public bool checkFull()
    {
        return is_full;
    }

    public int getType()
    {
        return truck_type;
    }

}