using System;
using System.Collections.Generic;
using System.Text;


public class Robot
{
    private int maximum_weight;
    private int battery_capacity;
    private int current_battery;

    //how fast robot runs out of battery
    private int drain;
    private int minimum_battery_threshold; //charge upon reaching this or lower

    private int status; //0 = in queue, 1 = at loading dock, 2 = moving through warehouse, 3 = at destination, 4 = charging

    //used for collision detection
    private int wait;
    private int limit;

    //element 0 = x, 1 = y;
    private int[] current_location = new int[2];
    private int[] warehouse_dimensions = new int[2];
    private int[] destination = new int[4];

    private List<int[]> other_robot_locations;

    private List<Item> cargo = new List<Item>();
    bool is_full;
    int total_items;

    public Robot()
    {
        maximum_weight = 200;
        battery_capacity = 100;
        current_battery = 100;

        drain = 1;
        minimum_battery_threshold = 10;

        status = 0;
        wait = 0;
        limit = 10;

        current_location[0] = 0;
        current_location[1] = 0;

        is_full = false;
        total_items = 0;
    }

    public Robot(int weight, int battery_cap, int current_bat, int bat_drain, int bat_threshold, int state, int wait_limit, int x_loc, int y_loc)
    {
        maximum_weight = weight;
        battery_capacity = battery_cap;
        current_battery = current_bat;

        drain = bat_drain;
        minimum_battery_threshold = bat_threshold;

        status = state;
        wait = 0;
        limit = wait_limit;

        current_location[0] = x_loc;
        current_location[1] = y_loc;

        is_full = false;
        total_items = 0;
    }

    public void setRobotStats(int weight, int battery_cap, int current_bat, int bat_drain, int bat_threshold, int state, int wait_limit, int x_loc, int y_loc)
    {
        maximum_weight = weight;
        battery_capacity = battery_cap;
        current_battery = current_bat;

        drain = bat_drain;
        minimum_battery_threshold = bat_threshold;

        status = state;
        limit = wait_limit;

        current_location[0] = x_loc;
        current_location[1] = y_loc;
    }




    //ROBOT PATHFINDING LOGIC

    //assuming warehouse coordinate structure is like this
    //increasing x = right, increasing y = up
    //_______________
    //|3            |
    //|2| | | | | | |
    //|1| | | | | | |
    //|0 1 2 3 4 5 6|
    // --LOAD--------
    //example warehouse structure with 1 and 2 as loading zones
    //I assume (0,0) is a door for robots to exit to charging stations

    //BASIC MOVEMENT ALGORITHM
    //the algorithm I use will always have robots move right through x first
    //when at the correct column, it will go up in y, stopping at target location
    //after preforming whatever task at the shelf, it will move up to the top row and return going left
    //using the left most column as a reset/queue, location (0,0) also contains a door to leave/enter

    //EXAMPLE
    //want to go to fifth row, 2nd shelf, left side
    //robot goes right along 0,0 until 5,0
    //robot goes up along 5,0 to 5,2
    //does whatever operation it supposed to do
    //moves up to 5,3 and then left from 5,3 to 0,3
    //finally, it moves back to 0,0 ready to interact with the loading area

    //COLLISION DETECTION
    //if it encounters another robot it the way, it waits until the other bot moves
    //it also detects if another robot is about to move into the spot it wants to move
    //giving right of way to robots in the main movement lanes at the top and bottom (robots moving left and right)
    //I assume robots can not move through shelves so they must move at either the top or bottom

    //ADVANCED MOVEMENT (IE MULTIPLE SHELVES TO GO TO)
    //robots will use the top lane for moving left and bottom lane for moving right (primary movement lane)
    //robots will go up and down lanes to get to their destinations
    //if a robot is blocking their way, they will try to move to the closest primary movement lane so they don't stop traffic
    //it will then attempt again to get to their destination

    //I am not sure if this above algorithm is completely fail proof as there might be scenario depending on warehouse size
    //that two robots end up perfectly blocking each other everytime they try to get to the desired location
    //one fix would be to just have all robots loop through 0,0 after each delivery but that makes the robots really slow
    //plus they now have to wait for other robots to load and deliver which means even slower
    //the fail safe I added is if a robot is unable to get to its desired destination for too long, it will reset and go through 0,0


    //I thought of other pathfinding methods but I believe this is the most
    //straightforward and reliable methods with minimal possibility of collisions
    public void pathFinding()
    {
        int right = 1;
        int left = 2;
        int up = 3;
        int down = 4;


        //if too many total waits, try reseting
        if (wait > limit)
        {
            var delivery = destination;
            destination[0] = 0;
            destination[1] = 0;

            //made it back to start, reset wait and try to deliver again
            if (current_location[0] == 0 && current_location[1] == 0)
            {
                destination = delivery;
                wait = 0;
            }
        }


        //moving right logic
        if (current_location[0] < destination[0])
        {
            //if there is shelf in the way try moving down to main right moving lane 

            //commenting out all the move lines, no longer debugging
            // Console.WriteLine("trying to move right...");
            if (checkShelfCollision())
            {
                //Console.WriteLine("shelf in the way, trying to move down");
                //check to see if a robot is in the way
                if (checkForRobots(down))
                {
                    //Console.WriteLine("robot in the way, waiting...");
                    checkWait();
                }
                else
                {
                    //Console.WriteLine("moving down");
                    moveDown();
                }
            }
            else
            {
                if (checkForRobots(right))
                {
                    // Console.WriteLine("robot in the way, waiting...");
                    checkWait();
                }
                else
                {
                    //Console.WriteLine("moving right");
                    moveRight();
                }

            }
        }

        //moving left logic
        else if (current_location[0] > destination[0])
        {
            //Console.WriteLine("trying to move left...");
            //if there is shelf in the way try moving up to main left moving lane 
            if (checkShelfCollision())
            {
                //Console.WriteLine("shelf in the way, trying to move up");
                //check to see if a robot is in the way
                if (checkForRobots(up))
                {
                    // Console.WriteLine("robot in the way, waiting...");
                    checkWait();
                }
                else
                {
                    //Console.WriteLine("moving up");
                    moveUp();
                }
            }
            else
            {
                if (checkForRobots(left))
                {
                    //Console.WriteLine("robot in the way, waiting...");
                    checkWait();
                }
                else
                {
                    //Console.WriteLine("moving left");
                    moveLeft();
                }

            }
        }

        //moving up
        else if (current_location[1] < destination[1])
        {
            //check to see if a robot is in the way
            if (checkForRobots(up))
            {
                //Console.WriteLine("robot in the way, waiting...");
                wait++;
            }
            else
            {
                // Console.WriteLine("moving up");
                moveUp();
            }
        }

        //moving down
        else if (current_location[1] > destination[1])
        {
            //check to see if a robot is in the way
            if (checkForRobots(down))
            {
                // Console.WriteLine("robot in the way, waiting...");
                wait++;
            }
            else
            {
                //Console.WriteLine("moving down");
                moveDown();
            }
        }
        else
        {
            //Console.WriteLine("at destination");
        }

    }

    //move right (x + 1)
    private void moveRight()
    {
        if (current_location[0] + 1 <= warehouse_dimensions[0])
        {
            current_location[0]++;
            current_battery -= drain;
        }
        else
        {
            Console.WriteLine("can not go right! Exceeds warehouse boundaries");
        }
    }

    //move left (x - 1)
    private void moveLeft()
    {
        if (current_location[0] - 1 >= 0)
        {
            current_location[0]--;
            current_battery -= drain;
        }
        else
        {
            Console.WriteLine("can not go left! Exceeds warehouse boundaries");
        }
    }

    //move up (y + 1)
    private void moveUp()
    {
        if (current_location[1] + 1 <= warehouse_dimensions[1])
        {
            current_location[1]++;
            current_battery -= drain;
        }
        else
        {
            Console.WriteLine("can not go up! Exceeds warehouse boundaries");
        }
    }

    //move down (y - 1)
    private void moveDown()
    {
        if (current_location[1] - 1 >= 0)
        {
            current_location[1]--;
            current_battery -= drain;
        }
        else
        {
            Console.WriteLine("can not go down! Exceeds warehouse boundaries");
        }
    }

    //self check if we are where we are supposed to be
    private void checkWait()
    {
        //if we're not where we want to be and not in queue, we are waiting
        if (!(current_location[0] == destination[0] && current_location[1] == destination[1]) && !checkIfQueued())
        {
            wait++;
        }
    }

    //check space (for other robots)
    //direction: 1 = right, 2 left, 3 = up, 4 = down
    //return true if robot is in the way
    //@param the direction robot should move to 
    //@return true if successful 
    private bool checkForRobots(int direction)
    {
        foreach (var location in other_robot_locations)
        {
            //ignore self location in the list, only do directional checks on other robots
            if (!(location[0] == current_location[0] && location[1] == current_location[1]))
            {
                if (direction == 1)
                {
                    //if going right = a spot a robot is in, robot in the way
                    if (current_location[0] + 1 == location[0] && current_location[1] == location[1])
                    {
                        return true;
                    }
                }

                if (direction == 2)
                {
                    //if going left = a spot a robot is in, robot in the way
                    if (current_location[0] - 1 == location[0] && current_location[1] == location[1])
                    {
                        return true;
                    }
                }

                if (direction == 3)
                {
                    //if going up = a spot a robot is in, robot in the way
                    if (current_location[1] + 1 == location[1] && current_location[0] == location[0]
                        //second condition for moving up and down, need to check for oncoming traffic
                        //moving up = spot a robot is about to be in, robot in the way
                        || (current_location[1] + 1 == location[1] && current_location[0] == location[0] - 1)
                        //move up into a robot moving down also = collide
                        || (current_location[1] + 1 == location[1] - 1 && current_location[0] == location[0]))
                    {
                        return true;
                    }
                }

                if (direction == 4)
                {
                    //if going down = a spot a robot is in, robot in the way
                    if (current_location[1] - 1 == location[1] && current_location[0] == location[0]
                        //second condition for moving up and down, need to check for oncoming traffic
                        //moving down = spot a robot is about to be in, robot in the way
                        || (current_location[1] - 1 == location[1] && current_location[0] == location[0] + 1)
                        || (current_location[1] - 1 == location[1] + 1 && current_location[0] == location[0]))
                    {
                        return true;
                    }
                }
            }

        }

        return false;
    }

    ///only used for moving left or right
    //@return true if move successfully 
    private bool checkShelfCollision()
    {
        //if we're in bottom or top lane, no shelves to collide with
        if (current_location[1] == 0 || current_location[1] == warehouse_dimensions[1])
        {
            return false;
        }

        return true;
    }

    //check if queued
    //@return true if the robot is queued 
    public bool checkIfQueued()
    {

        if (status == 0)
        {
            return true;
        }
        return false;
    }

    //need to constantly update this to see where other robots are
    public void getRobotLocations(List<int[]> locations)
    {
        other_robot_locations = locations;
    }

    //check if queued
    //@return true if the robot is queued 
    public int[] giveLocation()
    {
        return current_location;
    }

    //tell robot size of warehouse
    public void setWarehouseSize(int x, int y)
    {
        warehouse_dimensions[0] = x;
        warehouse_dimensions[1] = y;
    }

    //give directions to the shelf
    public void setLocation(int x, int y, int side, int shelf)
    {
        destination[0] = x;
        destination[1] = y;
        destination[2] = side;
        destination[3] = shelf;
    }

    public int[] getShelf()
    {
        int[] shelfInfo = { destination[3], destination[4] };
        return shelfInfo;
    }


    //copied from truck class -----------------------------------------------------

    //load item
    public void loadItem(Item new_item)
    {
        //only load if we are at the trucks or shelves
        if (status == 1 || status == 3)
        {
            if (checkWeight() + new_item.getweight() > maximum_weight)
            {
                Console.WriteLine("item not loaded: exceeds maximum weight capacity");
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
            Console.WriteLine("item not loaded: not at dock or proper shelf");
        }
    }

    //unload item
    //note if you remove an item from the middle of the list then all other items after it
    //move down by 1 so you need to keep track of where everything is
    //the alternative is to just always remove the last item or always remove the first item
    public Item unloadItem(int item_number, int robotID)
    {
        if (status == 1 || status == 3)
        {
            if (item_number < total_items)
            {
                var removed_item = checkItem(item_number);
                cargo.RemoveAt(item_number);
                total_items--;
                is_full = false;
                Console.WriteLine("item " + removed_item.getname() + " removed from robot {0}", robotID);
                return removed_item;
            }
            else
            {
                Console.WriteLine("no more items to unload from robot {0}", robotID);
                //Console.WriteLine("item not unloaded: item at location does not exist");
            }
        }
        else
        {
            Console.WriteLine("item not unloaded: not at dock or proper shelf");
        }

        return null;
    }

    //@return the specific ith  item in the cargo list 
    private Item checkItem(int item_number)
    {
        return cargo[item_number];
    }

    //check weight, returns weight of items robot carrying
    public int checkWeight()
    {
        int current_weight = 0;

        foreach (var Item in cargo)
        {
            current_weight = current_weight + Item.getweight();
        }

        return current_weight;
    }

    public int getmaxweight()
    {
        return maximum_weight;
    }

    //end of truck class code -------------------------------------------------

    //check full
    public bool isFull()
    {
        return is_full;
    }

    //check if at destination
    public bool checkArrive()
    {
        if (current_location[0] == destination[0] && current_location[1] == destination[1])
        {
            return true;
        }

        return false;
    }

    public void setStatus(int state)
    {
        status = state;
    }

    public int returnStatus()
    {
        /*
        if (status == 0)
        {
            Console.WriteLine("currently in queue");
        }
        else if (status == 1)
        {
            Console.WriteLine("currently at loading docks");
        }
        else if (status == 2)
        {
            Console.WriteLine("currently moving to destination");
        }
        else if (status == 3)
        {
            Console.WriteLine("currently at target shelf location");
        }
        else
        {
            Console.WriteLine("currently charging");
        }
        */
        return status;
    }

    //while in queue: if the battery can not handle the expected route cost, go charge
    public void checkIfNeedCharge()
    {
        //if we're just waiting in queue and our next order is undoable, go charge
        if (status == 0)
        {
            //set to less than min threshold in case robot thinks it can do
            //delivery but takes more battery than expected due to blocks from other robots

            //our route is the perimeter based on our desired destination + the y parameter of the warehouse
            //double each value for both sides and subtract by 4 for the redundancy in the corners
            if (current_battery - drain * (destination[0] * 2 + warehouse_dimensions[1] * 2 - 4) < minimum_battery_threshold)
            {
                status = 4;
                destination[0] = 0;
                destination[1] = 0;
            }
        }
    }


    //get current battery value
    public int checkBattery()
    {
        return current_battery;
    }


    //need charging station to max
    //wait for a bit before sending this robot back 
    //at the moment this instantly charged the robot
    public void chargeRobot()
    {
        current_location[0] = 0;
        current_location[1] = -1;
        destination[0] = 0;
        destination[1] = 0;

        current_battery = battery_capacity;

    }

    public void reEnterWarehouse()
    {
        //3 is move up
        //if no robots in the way
        if (!checkForRobots(3))
        {
            moveUp();
            status = 0;
        }
    }








}