using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Net.Sockets;
using System.Text;
using System.IO;

namespace CarSimulator
{
    class centralcomputer
    {

        //public centralcomputer() { 
        public static int NUMBER_OF_BOTS = 5;
        public static Queue<Robot> robot = new Queue<Robot>(NUMBER_OF_BOTS);
        public static Queue<Truck> trucks = new Queue<Truck>();

        public static Truck[] TRUCK_AT_DOCK = new Truck[2]; //only 2 docks, 1 for restocking, 1 for deliverys out
                                                            //can be modified to include more than 1 dock for each
        public static bool EMPTY_RESTOCK_BAY;
        public static bool EMPTY_DELIVERY_BAY;
        public static int TOTAL_RESTOCK_TRUCKS;
        public static int TOTAL_DELIVERY_TRUCKS;

        public static bool SHUTDOWN_ALL_ROBOTS;
        public static bool FORCE_TRUCK_TO_LEAVE;

       // public static string[] order;
        public static Queue<Item> orderready = new Queue<Item>();           //not done 
        public static Queue<Item> orderoutfordelivery = new Queue<Item>();  // not done 



        public static Queue<Item> orderreceived = new Queue<Item>();
        public static Queue<Item> restock = new Queue<Item>();     // items need to be restock decided by manager

        public static int CHARGE_DURATION = 10000;
        public static int WAREHOUSE_X = 6;
        public static int WAREHOUSE_Y = 10;
        public static int side = 2;
        public static int shelf = 4;
        public static List<int[]> robotLocationTracker = new List<int[]>(2 * NUMBER_OF_BOTS);


        //create robots as indicated   could be done by manager 
        //@param number of robots
        void setrobotsnumber(int num)
        {
            robot = new Queue<Robot>(num);
        }
        //  could be done by manager 
        //@param number of truck 
        void settruckssnumber(int num)
        {
            trucks = new Queue<Truck>(num);
        }

        //  could be done by manager 
        //number of row 
        void setrow(int numx)
        {
            WAREHOUSE_X = numx;
        }
        //  could be done by manager 
        // number of column
        void setcolumn(int numy)
        {
            WAREHOUSE_Y = numy;
        }
        //  could be done by manager 
        // number of shelf 
        void setshelf(int numshelf)
        {
            shelf = numshelf;
        }

        /*
        // when user purchase item it invokes this function 
        //orderreceived consists of all item required by buyer 
        void order(string itemname, Warehouse warehouse)             // need to change to string 
        {


            orderreceived.Enqueue(warehouse.findItem(itemname));
        }
        */


        // alert manager if the number of an item is < 5
        //remind manager to add more
        //@param warehouse 
        public static void stockalert(Warehouse warehouse)   //  
        {
            List<Item> items = new List<Item>();
            items = warehouse.getstockinfo();      // a list show the stock of each item 
            for (int l = 0; l < items.Count; l++)
            {
                if (items[l].getitemnum() < 5)
                {
                    Console.WriteLine("The {0} is almost outof stock please add more to warehouse", items[l].getname());
                    Console.WriteLine("press 1 to add 3 more this item, press 0 to reject");   //this shoud be converted to interface version later
                    
                    /*
                    int agreement = Int32.Parse(Console.ReadLine());

                    if (agreement == 1)
                    {
                        int i = 0;
                        while (i < 3)
                        {
                            restock.Enqueue(items[l]);
                            i++;
                        }
                    }  */                                                                        // manager press "agree" button 


                }
            }
        }

        //@param list of items in warehouse 
        public static void printwarehousestock(List<Item> items)
        {
            for (int l = 0; l < items.Count; l++)
            {
                Console.WriteLine("Items in warehouse:");
                Console.WriteLine("{0} : {1}", items[l].getname(), items[l].getitemnum());
            }
        }





        private static Mutex mut = new Mutex(); //for locking locational movement of robots
        private static Mutex mut2 = new Mutex(); //for locking item retrieval of robots
        //initialize the warehouse, create robots and trucks
        //force the whole program keeping running and recieving string sent by interface 
        static void Main(string[] args)   //  this should be a loop to keep update warehouse and get info from interface ?????
                                          // see bottom of Main for answer
        {
            Warehouse depot = warehouseGenerator();   // house contians all the info we need it could be used as database 
                                                      // we only need to call method in warehouse to get info

            EMPTY_DELIVERY_BAY = true;  //initialize the loading bays to be empty
            EMPTY_RESTOCK_BAY = true;   //central computer can use these variables to check if there is a truck at the bay
            TOTAL_RESTOCK_TRUCKS = 0;
            TOTAL_DELIVERY_TRUCKS = 0; //used so that a delivery truck does not dock at wrong location

            SHUTDOWN_ALL_ROBOTS = false;
            FORCE_TRUCK_TO_LEAVE = false;

            /*
            //if this part is supposed to be able to be repeated executed, then put this at the bottom after robot creation
            List<Item> items = new List<Item>();
            items = depot.getstockinfo();
            printwarehousestock(items);   
            stockalert(depot);
            */



            //initialization of robots and info needed for robots
            int[][] robot_loc = new int[NUMBER_OF_BOTS][];
            int[][] instructions = new int[NUMBER_OF_BOTS][];

            Thread[] robots = new Thread[NUMBER_OF_BOTS];

            Queue<Item>[] order_list = new Queue<Item>[NUMBER_OF_BOTS];
            Queue<Item>[] restock_list = new Queue<Item>[NUMBER_OF_BOTS];

            //create robots    initialize threads 
            for (int i = 0; i < NUMBER_OF_BOTS; i++)
            {
                int x = i;
                robot_loc[x] = new int[2];
                instructions[x] = new int[7];
                order_list[x] = new Queue<Item>();
                restock_list[x] = new Queue<Item>();
                robots[x] = new Thread(() => robotOp(WAREHOUSE_X, WAREHOUSE_Y, x, depot, robot_loc[x], instructions[x], order_list[x], restock_list[x]));

                robots[x].Start();
            }



            //after robots are created, need to have central computer code running here as a permanant loop
            //make central computer loop here:
            //basically continuously runs and gets info from webserver
            //handles the info from website like orders and button presses from manager
            //include truckOp in the loop

            //initialize variables central computer will use
            bool program_is_running = true;
            List<Item> items = new List<Item>();
            List<Item> restock_orders = new List<Item>();
            List<Item> client_orders = new List<Item>();   //note: if we want multiple clients to connect at once,
                                                           //we will need to turn this into an array so we can
                                                           //keep track of each order made by the different clients
            bool need_restock_truck = false;
            bool need_delivery_truck = false;









            TcpListener listener = new TcpListener(System.Net.IPAddress.Any, 1302);
            listener.Start();






            string receivedString = "";










            //used to loop central computer main function
            do
            {






                ////////////////////////////////////////////////////////////////////////////////
                Console.WriteLine("Waiting for a connection.");
                TcpClient client = listener.AcceptTcpClient();
                Console.WriteLine("Client accepted.");
                NetworkStream stream = client.GetStream();
                StreamReader sr = new StreamReader(client.GetStream());
                StreamWriter sw = new StreamWriter(client.GetStream());
                try
                {
                    byte[] buffer = new byte[1024];
                    stream.Read(buffer, 0, buffer.Length);
                    int recv = 0;
                    foreach (byte b in buffer)
                    {
                        if (b != 0)
                        {
                            recv++;
                        }
                    }

                    string request = "--";
                    Console.WriteLine("received string is: " + request);
                    //receive the string flushed from the MVC side
                    request = Encoding.UTF8.GetString(buffer, 0, recv);
                    Console.WriteLine(request);
                    receivedString = request;
                    Console.WriteLine("received string is: " + request);

                    Console.WriteLine("request received");
                    sw.WriteLine("123456");
                    sw.Flush();
                }
                catch (Exception e)
                {
                    Console.WriteLine("Something went wrong.");
                    sw.WriteLine(e.ToString());
                }






                //////////////////////////////////////////////////////////////////////











                //items = depot.getstockinfo();           //send this info to website (I think), central computer output
                //printwarehousestock(items);

               

                //stockalert(depot);                      //send this info to website (I think), central computer output


                //need here some sort of way to get input from website
                //----------------------------------
                //
                //read database or read a file, whatever is decided
                //
                //----------------------------------
                //section above should provide all the inputs from website that is needed for next section
                //example: we need the list of items the customer is buying
                //cient_orders = info from website

                string orderORstock=""; // should be changed later 
                
                orderORstock = receivedString;



                Console.WriteLine("real received string is: " + orderORstock);

                string[] order_stock= orderORstock.Split(",");


                //Console.WriteLine("order_stock is: {0}", order_stock[0]);
                //Console.WriteLine("order_stock is: {0}", order_stock[1]);
                //Console.WriteLine("order_stock is: {0}", order_stock[2]);

                if (string.Equals(order_stock[0], "0"))
                {

                    need_delivery_truck = true;
                }
                else if(string.Equals(order_stock[0], "1"))
                {
                    need_restock_truck = true;
                }
                truckOp(need_restock_truck, need_delivery_truck);
                need_delivery_truck = false;
                need_restock_truck = false;



                if (string.Equals(order_stock[0], "0")) { //0 is order,1 is restock 



                    Console.WriteLine("//////////////////");



                    int i = 1;
                    while(i < order_stock.Length) {
                        //client_orders.Add(depot.findid(order_stock[i]));
                        if(depot.findid(order_stock[i]) == null)
                        {
                            Console.WriteLine("Item out of stock");
                        }
                        else
                        {
                            orderreceived.Enqueue(depot.findid(order_stock[i]));
                            Console.WriteLine("order_stock {0} is: {1}", i, order_stock[i]);
                            //Console.WriteLine("restock item is: {0}" + restock.Peek().getname());
                        }

                        i++;
                    }


                }
                
                


                
                if (string.Equals(order_stock[0], "1"))
                { //0 is order,1 is restock 

                    int i = 1;
                    while (i < order_stock.Length)
                    {

                        for (int w = 0; w < WAREHOUSE_X; w++)
                        {
                            for (int j = 0; j < WAREHOUSE_Y; j++)
                            {
                                for (int k = 0; k < side; k++)
                                {
                                    for (int z = 0; z < shelf; z++)
                                    {


                                   
                                        if (depot.spot[w, j, k, z] == null)
                                        {
                                            
                                            if (order_stock[i] == "1")
                                            {
                                                Item restocka = new Item(5, 1, "Arduino", "1");
                                                restocka.setlocation(w, j, k, z);
                                                restock_orders.Add(restocka);
                                                w = 100;
                                                j = 100;
                                                k = 100;
                                                z = 100;
                                            }
                                            else if (order_stock[i] == "2")
                                            {
                                                Item restocka = new Item(6, 1, "DE1-SoC", "2");
                                                restocka.setlocation(w, j, k, z);
                                                restock_orders.Add(restocka);
                                                w = 100;
                                                j = 100;
                                                k = 100;
                                                z = 100;
                                            }
                                            else if (order_stock[i] == "3")
                                            {
                                                Item restocka = new Item(7, 2, "Hp-prime", "3");
                                                restocka.setlocation(w, j, k, z);
                                                restock_orders.Add(restocka);
                                                w = 100;
                                                j = 100;
                                                k = 100;
                                                z = 100;
                                            }
                                            else if (order_stock[i] == "4")
                                            {
                                                Item restocka = new Item(8, 2, "Icliker", "4");
                                                restocka.setlocation(w, j, k, z);
                                                restock_orders.Add(restocka);
                                                w = 100;
                                                j = 100;
                                                k = 100;
                                                z = 100;
                                            }
                                            else if (order_stock[i] == "5")
                                            {
                                                Item restocka = new Item(9, 2, "Multimeter", "5");
                                                restocka.setlocation(w, j, k, z);
                                                restock_orders.Add(restocka);
                                                w = 100;
                                                j = 100;
                                                k = 100;
                                                z = 100;
                                            }
                                            else
                                            {
                                                Item restocka = new Item(10, 2, "PYNQ-z1", "6");
                                                restocka.setlocation(w, j, k, z);
                                                restock_orders.Add(restocka);
                                                w = 100;
                                                j = 100;
                                                k = 100;
                                                z = 100;

                                            }
                                            
                                            // change
                                            i++;
                                        }

                                    }
                                }
                            }
                        }
                       
                        
                    }
                }
                


                //send items to warehouse
                bool new_restock = true; // this needs to come from website when a manager wants to restock items, see input section above
                if (new_restock)
                {

                    need_restock_truck = true;


                    //new restock order comes in the form of a list of items (probably)
                    //loop until all new items are added to restock queue

                    while (restock_orders.Count != 0)
                    {
                        restock.Enqueue(restock_orders[0]);
                        restock_orders.RemoveAt(0);
                    }

                }


                //want items from warehouse
                bool new_order = true; // this needs to come from website when a new order is made, see input section above
                if (new_order)
                {

                    need_delivery_truck = true;



                    //new client orders comes in the form of a list of items (probably)
                    //loop until all new items are added to orderreceived queue

                    while (client_orders.Count != 0)
                    {
                        orderreceived.Enqueue(client_orders[0]);
                        client_orders.RemoveAt(0);
                    }

                }

                //can also have a force truck to leave section here but might be messy
                //maybe better to just not include

                //send info to the trucks and open bays
                //(might have slight synchronization problem with robots and departing trucks but ignore for now) 
               






                Thread.Sleep(1000);




                /*
                 int num = depot.numberofitem();
                 Console.WriteLine("There are {0} items in the warehouse", num);
                 int sumlocation = depot.overalllocation();
                 Console.WriteLine("There are {0} locations in the warehouse", sumlocation);
                 int locationleft = depot.locationleft();
                 Console.WriteLine("There are {0} locations left in the warehouse", locationleft);
                */

                /*
                List<Item> items_s = depot.getstockinfo();

                foreach (var item in items_s)
                {
                    Console.WriteLine("{0}: {1}", item.getname(), item.getitemnum());
                }
                */

                Console.WriteLine("all items in warehouse are:");
                depot.showItem();
                //depot.showItemManual();















            } while (program_is_running);



            //find spot to put this in loop above
            //manually turn off all the robots
            if (SHUTDOWN_ALL_ROBOTS)
            {
                for (int i = 0; i < NUMBER_OF_BOTS; i++)
                {
                    int x = i;

                    robots[x].Join();
                }
            }


        }


        //truck stuff
        //for simplicity, restocking trucks will be at 1,0 and delivery trucks will be at 2,0
        //TRUCK_AT_DOCK 0 = restock trucks, 1 = delivery trucks
        //@param the new item needed to be restock 
        //@param the new items ordered by users needed to be delievered 
        public static void truckOp(bool new_restock, bool new_delivery)
        {

            //input from central computer
            if (new_restock)
            {
                Truck new_Rtruck = new Truck(1000, 500, 0, 0);
                trucks.Enqueue(new_Rtruck);
                TOTAL_RESTOCK_TRUCKS++;
            }

            if (new_delivery)
            {
                Truck new_Dtruck = new Truck(1000, 500, 0, 1);
                trucks.Enqueue(new_Dtruck);
                TOTAL_DELIVERY_TRUCKS++;
            }

            //dock trucks if bay available
            if (trucks.Count != 0)
            {
                if (EMPTY_RESTOCK_BAY == true && TOTAL_RESTOCK_TRUCKS != 0)
                {
                    TRUCK_AT_DOCK[0] = trucks.Dequeue();
                    TRUCK_AT_DOCK[0].dock(true);
                    TOTAL_RESTOCK_TRUCKS--;
                    EMPTY_RESTOCK_BAY = false;
                }

                if (EMPTY_DELIVERY_BAY == true && TOTAL_DELIVERY_TRUCKS != 0)
                {
                    TRUCK_AT_DOCK[1] = trucks.Dequeue();
                    TRUCK_AT_DOCK[1].dock(true);
                    TOTAL_DELIVERY_TRUCKS--;
                    EMPTY_DELIVERY_BAY = false;
                }
            }

        }
        //let robot move items from shelf to truck or truck to shelf without colliding each other 
        //@param dimension of warehouse: row 
        //@param dimension of warehosue: column 
        //@param number of robot 
        //@param warehouse 
        //@param where is the robot at warehosue 
        //@param instructions from user 
        //@param all ordered items
        //@param all restock items
        public static void robotOp(int warehouse_x, int warehouse_y, int robotNumber, Warehouse warehouse, int[] robot_loc, int[] instructions, Queue<Item> orderlist, Queue<Item> restocklist)
        {
            //manual stop input from manager interface
            bool stop = false;
            Robot drone = new Robot();



            //add robot location to list immediately
            drone.setLocation(0, robotNumber, 0, 0);
            robotLocationTracker.Add(drone.giveLocation());

            //initial parameters robot needs
            drone.setWarehouseSize(warehouse_x, warehouse_y);

            //main loop
            //some sort of stop command if needed to shut robot off
            while (!SHUTDOWN_ALL_ROBOTS)
            {            // manually stop it   otheriwise keep running        !!!!!!!!!!!!

                //check if needs charging
                chargingOperation(drone, robot_loc, robotNumber);

                //main movement and loading/delivering logic
                //if drone arrived where it's supposed to be and in queue, give new instructions based on needs

                // always restock first 

                while (restock.Count != 0 && drone.checkArrive() && drone.checkIfQueued())
                // problem !!! everyrobot get their own orderlist or restock list 
                //could grab same thing twice !!!!!!! how to fix this ???? 
                {                                //this is only one robot should not use for loop to force it 
                                                 //grab all items in the orderlist 
                    int weight = 0;


                    while (weight < drone.getmaxweight()&& restock.Count != 0)
                    {
                        mut2.WaitOne();
                        if (restock.Count != 0)
                        {                                               // ^^^ problem above solved by locking the restock queue
                            weight = weight + restock.Peek().getweight();
                            restocklist.Enqueue(restock.Dequeue());
                        }
                        mut2.ReleaseMutex();
                    }
                    truckTospecificShelfOperation(drone, TRUCK_AT_DOCK[0], warehouse, instructions, robot_loc, robotNumber, restocklist);


                }


                while (restock.Count == 0 && orderreceived.Count != 0 && drone.checkArrive() && drone.checkIfQueued())
                {

                    int weight = 0;

                    while (weight < drone.getmaxweight()&& orderreceived.Count != 0)
                    {
                        
                        mut2.WaitOne();
                        if (orderreceived.Count != 0)
                        {
                            weight = weight + orderreceived.Peek().getweight();
                            orderlist.Enqueue(orderreceived.Dequeue());
                        }
                        mut2.ReleaseMutex();
                    }

                    shelfToTruckOperation(drone, TRUCK_AT_DOCK[1], warehouse, instructions, robot_loc, robotNumber, orderlist);


                }

                //operation finished return to start
                drone.setLocation(0, 0, 0, 0);
                drone.setStatus(2);
                robotPathfinding(drone, robot_loc, robotNumber);
                drone.setStatus(0);

                //Console.WriteLine("robot #{0} made it through one loop", robotNumber);

                //Console.WriteLine("");

                //warehouse.showItem();


            }


        }

        //charge robots when robot is run out of battery 
        //@param robot 
        //@param the location of current robot 
        //@param the ID of robot 

        public static void chargingOperation(Robot currentBot, int[] loc, int robotID)
        {
            currentBot.checkIfNeedCharge();
            //status 4 = needs to charge
            if (currentBot.returnStatus() == 4)
            {
                //pathfind our way to the exit for charging

                currentBot.setLocation(0, 0, 0, 0);
                robotPathfinding(currentBot, loc, robotID);

                //charge and let other robots know we are out of the warehouse
                currentBot.chargeRobot();
                updateLocationList(loc, robotID);
                Thread.Sleep(CHARGE_DURATION);         //sleep for as long as the robot needs to charge

                //keep trying to re enter the warehouse until in
                do
                {
                    currentBot.reEnterWarehouse();
                } while (!currentBot.checkArrive());

            }
        }


        //instructions for going from the truck to the shelves
        //@param robot 
        //@param truck 
        //@param warehosue 
        //@param instruction from interfaace 
        //@param the location of robot 
        //@param Id of robbot 
        //@param the items needed to be restocked 
        public static void truckTospecificShelfOperation(Robot currentBot, Truck currentTruck, Warehouse warehouse, int[] ins, int[] loc, int robotID, Queue<Item> restock)
        {
            //truck location
            currentBot.setLocation(2, 0, 0, 0); // force robot and truck meet at(1,0) simplify problem  could be changed //change this
            currentBot.setStatus(2);    // moving in the warehouse
            robotPathfinding(currentBot, loc, robotID);
            //now at the truck
            currentBot.setStatus(1);


            // convert queue restock to list 
            Item[] restock2 = new Item[restock.Count];
            restock.CopyTo(restock2, 0);
            List<Item> restock1 = new List<Item>();
            for (int i = 0; i < restock.Count; i++)
            {
                restock1.Add(restock2[i]);
            }

            currentTruck.loadManyItem(restock1); // put restock on truck 


            //loading process
            //check if truck is empty or robot is full
            int check = 0;
            do
            {
                check = unloadTruckToRobot(currentBot, currentTruck);
            } while (check == 0);

            //check value might be useful for central cpu, can return it if needed
            if (check == 1)
            {
                Console.WriteLine("truck is empty, robot {0} moving", robotID);
                currentTruck.finishLoading(true);
                EMPTY_RESTOCK_BAY = true;
            }
            else if (check == 2)
            {
                Console.WriteLine("robot {0} is full, leaving dock", robotID);
            }
            else
            {
                Console.WriteLine("finished loading but unsure why...? ERROR DETECTED");
            }


            //go to shelf 
            //to make the robot go to multiple locations, we would just update the instructions with the new shelf locations 
            //and have the path that was originally set keep track of all the locations the bot is supposed to go


            while (restock.Count != 0)
            {
                Item justonshelf = warehouse.Additem(restock.Dequeue());
                currentBot.setLocation(justonshelf.getcolumn(), justonshelf.getrow(), justonshelf.getside(), justonshelf.getshelf());    // put item at any vacant spot in warehosue 
                currentBot.setStatus(2);
                robotPathfinding(currentBot, loc, robotID);

                //at the shelf now
                currentBot.setStatus(3);
                currentBot.unloadItem(0, robotID);

                // add item to any location as long as it is free

            }

        }

        //operation for moving items from shelf and loading on to truck 
        //prob need to include a force leave into parameters and a return value for if truck leaves <<<<<<------------------------------------ NOT DONE
        //@param robot 
        //@param truck 
        //@param warehosue 
        //@param instruction from interfaace 
        //@param the location of robot 
        //@param Id of robbot 
        //@param the items needed to be restocked n value for if truck leaves <<<<<<------------------------------------ NOT DONE
        public static void shelfToTruckOperation(Robot currentBot, Truck currentTruck, Warehouse warehouse, int[] ins, int[] loc, int robotID, Queue<Item> itemlist)
        {

            //go to shelf 
            //to make the robot go to multiple locations, we would just update the instructions with the new shelf locations
            //and have the path that was originally set keep track of all the locations the bot is supposed to go

            while (itemlist.Count != 0)   // robot went back to 0,0 until shopping list is done
            {
                //shelf location
                Item item1 = itemlist.Dequeue();                  //？？？？？
                currentBot.setLocation(item1.getrow(), item1.getcolumn(), item1.getside(), item1.getshelf());
                currentBot.setStatus(2);
                robotPathfinding(currentBot, loc, robotID);
                //now at the shelf
                currentBot.setStatus(3);



                //prob need to lock the warehouse shelves just to be safe.. but technically no robot will access the same shelf at the same time
                Item retrievedItem = warehouse.removeItemFromLocation(item1.getrow(), item1.getcolumn(), item1.getside(), item1.getshelf());
                if (retrievedItem != null)
                {
                    currentBot.loadItem(retrievedItem);
                }
                else
                {
                    Console.WriteLine("no item at location, moving on");
                }

            }

            //go to truck
            //truck location
            currentBot.setLocation(2, 0, 0, 0); // force robot and truck meet at (2,0) simplify problem 
            currentBot.setStatus(2);
            robotPathfinding(currentBot, loc, robotID);
            //now at the truck
            currentBot.setStatus(1);

            int check = 0;

            //should be variable from bool array to force trucks to leave sent from manager UI
            //included now, use global FORCE_TRUCK_TO_LEAVE

            //unloading bot into truck
            do
            {
                check = unloadRobotToTruck(currentBot, currentTruck, FORCE_TRUCK_TO_LEAVE, robotID);
            } while (check == 0);

            //check value might be useful for central cpu, can return it if needed
            if (check == 1)
            {
                Console.WriteLine("robot {0} is empty, robot moving", robotID);
            }
            //should include something for this about the  truck leaving for truck function <<<<<<------------------------------------ NOT DONE
            else if (check == 2)
            {
                Console.WriteLine("truck is full, leaving dock");
                EMPTY_DELIVERY_BAY = true;
            }
            else
            {
                Console.WriteLine("finished loading but unsure why...? ERROR DETECTED");
            }

        }



        //update the location of the robot
        //@param the loacation of robot 
        //@param the ID of robot 
        public static void updateLocationList(int[] location, int spot) //spot is robot id 
        {
            //NEED TO LOCK THIS ! ! !
            //mut.WaitOne();
            robotLocationTracker.RemoveAt(spot);        //!!!!
            robotLocationTracker.Insert(spot, location);
           // mut.ReleaseMutex();
        }

        //this is temporarily getting console input for testing
        //need to change this to get input from the UI
        //instrutions 0 = shelf x, 1 = shelf y, 2 = left or right, 3 = shelf,
        //4 = type of instruction, 5 = truck x, 6 = truck y
        public static void getInput(int robotID, int[] instructions)
        {
            Console.WriteLine("Give commands to robot #{0}", robotID);
            Console.WriteLine("Enter the commands for the robot:");
            Console.WriteLine("1. Unload truck -> Shelf      2. Unload shelf -> truck");
            instructions[4] = Int32.Parse(Console.ReadLine());
        }

        //robot auto pathing and updating info
        public static void robotPathfinding(Robot currentBot, int[] robot_location, int robotID)
        {
            do
            {
                //get the robots current location, update it's position, and get the new updated postions of other robots
                robot_location = currentBot.giveLocation();
                mut.WaitOne();
                updateLocationList(robot_location, robotID);
                currentBot.getRobotLocations(robotLocationTracker);   //????
                currentBot.pathFinding();      // output at destination
                mut.ReleaseMutex();
                //Console.WriteLine("robot is at location x: {0}, location y: {1} now", robot_location[0], robot_location[1]);
            } while (!currentBot.checkArrive());
        }

        //0 = successful load, 1 = failed IE truck empty, 2 = robot full
        public static int unloadTruckToRobot(Robot currentBot, Truck currentTruck)
        {
            Item unloaded = currentTruck.unloadItem(0);  //unload one item each time 
                                                         // output "item ....removed from truck“
            if (unloaded != null)
            {
                //try to load if there is actually an item
                currentBot.loadItem(unloaded);
                //if the bot is full, load back on to truck and let system know robot is full
                if (currentBot.isFull())
                {
                    currentTruck.loadItem(unloaded);
                    return 2;
                }
                return 0;
            }
            return 1;
        }

        //0 = successful load, 1 = failed IE robot empty, 2 = truck full
        public static int unloadRobotToTruck(Robot currentBot, Truck currentTruck, bool forceLeave, int robotID)
        {
            Item unloaded = currentBot.unloadItem(0, robotID);

            if (unloaded != null)
            {
                //try to load if there is actually an item
                currentTruck.loadItem(unloaded);
                //if the truck is full, load back on to bot and let system know truck is full
                if (currentTruck.finishLoading(forceLeave) == 1)
                {
                    currentBot.loadItem(unloaded);
                    return 2;
                }
                return 0;
            }

            return 1;
        }



        // initialize the warehosue and put items into warehouse 
        public static Warehouse warehouseGenerator()
        {
            Warehouse warehouse = new Warehouse(WAREHOUSE_X, WAREHOUSE_Y, 2, shelf);//the number here is the real number of row....
            

            // 
            /*// Location loc = new Location(3, 3, 3, 3);
            Item item1 = new Item(1, 1, "yeezy");
            item1.setlocation(1, 1, 1, 1);
            warehouse.spot[1, 1, 1, 1] = item1;
            //item1.getIteminfo();
            //warehouse.spot[1, 1, 1, 1].getIteminfo();
            */
            Random rnd = new Random();     //col = 10;  y = 10; 
            
            
            for (int i = 0; i < 5; i++)
            {

                
                Boolean done = false;
                while (!done)
                {
                    int column = rnd.Next(0, 9);
                    int row = rnd.Next(0, 5);
                    int side = rnd.Next(0, 1);
                    int shelf = rnd.Next(0, 3);   //col is x   row is y 
                    int weight = rnd.Next(0, 5);
                    int volume = rnd.Next(0, 5);
                    while (warehouse.spot[row, column,  side, shelf] == null && !done)
                    {
                        Item item2 = new Item(5, 1, "Arduino", "1");
                        item2.setlocation(row, column, side, shelf);
                        warehouse.spot[row, column, side, shelf] = item2;
                        done = true;

                    }
                }

            }

            
            for (int i = 0; i < 10; i++)
            {
                Boolean done = false;

                while (!done)
                {
                    int column = rnd.Next(0, 9);
                    int row = rnd.Next(0, 5);
                    int side = rnd.Next(0, 1);
                    int shelf = rnd.Next(0, 3);
                    int weight = rnd.Next(0, 5);
                    int volume = rnd.Next(0, 5);
                    while (warehouse.spot[row, column, side, shelf] == null && !done)
                    {
                        Item item6 = new Item(6, 1, "DE1-SoC", "2");
                        item6.setlocation(row, column, side, shelf);
                        warehouse.spot[row, column, side, shelf] = item6;
                        done = true;

                    }
                }

            }
            
         
            for (int i = 0; i < 8; i++)
            {
                Boolean done = false;
                while (!done)
                {
                    int column = rnd.Next(0, 9);
                    int row = rnd.Next(0, 5);
                    int side = rnd.Next(0, 1);
                    int shelf = rnd.Next(0, 3);
                    int weight = rnd.Next(0, 5);
                    int volume = rnd.Next(0, 5);
                    while (warehouse.spot[row, column, side, shelf] == null && !done)
                    {
                        Item item16 = new Item(7, 2, "Hp-prime", "3");
                        item16.setlocation(row, column, side, shelf);
                        warehouse.spot[row, column, side, shelf] = item16;
                        done = true;

                    }
                }

            }

         
         for (int i = 0; i < 4; i++)
         {
             Boolean done = false;
             while (!done)
             {
                    int column = rnd.Next(0, 9);
                    int row = rnd.Next(0, 5);
                    int side = rnd.Next(0, 1);
                    int shelf = rnd.Next(0, 3);
                    int weight = rnd.Next(0, 5);
                    int volume = rnd.Next(0, 5);
                    while (warehouse.spot[row, column, side, shelf] == null && !done)
                    {
                     Item item24 = new Item(8, 2, "Icliker", "4");
                     item24.setlocation(row, column, side, shelf);
                     warehouse.spot[row, column, side, shelf] = item24;
                     done = true;

                 }
             }

         }
         
            for (int i = 0; i < 5; i++)
            {
                Boolean done = false;
                while (!done)
                {
                    int column = rnd.Next(0, 9);
                    int row = rnd.Next(0, 5);
                    int side = rnd.Next(0, 1);
                    int shelf = rnd.Next(0, 3);
                    int weight = rnd.Next(0, 5);
                    int volume = rnd.Next(0, 5);
                    while (warehouse.spot[row, column, side, shelf] == null && !done)
                    {
                        Item item31 = new Item(10, 2, "PYNQ-z1", "6");
                        item31.setlocation(row, column, side, shelf);
                        warehouse.spot[row, column, side, shelf] = item31;
                        done = true;

                    }
                }

            }


          
         for (int i = 0; i < 2; i++)
         {
             Boolean done = false;
             while (!done)
             {
                    int column = rnd.Next(0, 9);
                    int row = rnd.Next(0, 5);
                    int side = rnd.Next(0, 1);
                    int shelf = rnd.Next(0, 3);
                    int weight = rnd.Next(0, 5);
                    int volume = rnd.Next(0, 5);
                    while (warehouse.spot[row, column, side, shelf] == null && !done)
                    {
                     Item item28 = new Item(9, 2, "Multimeter", "5");
                     item28.setlocation(row, column, side, shelf);
                     warehouse.spot[row, column, side, shelf] = item28;
                     done = true;

                 }
             }

         }


            Console.WriteLine("all items in warehouse");
            warehouse.showItem();
            int num = warehouse.numberofitem();
            Console.WriteLine("There are {0} items in the warehouse", num);
            int sumlocation = warehouse.overalllocation();
            Console.WriteLine("There are {0} locations in the warehouse", sumlocation);
            int locationleft = warehouse.locationleft();
            Console.WriteLine("There are {0} locations left in the warehouse", locationleft);

            List<Item> items = warehouse.getstockinfo();

            foreach(var item in items)
            {
                Console.WriteLine("{0}: {1}", item.getname(), item.getitemnum());
            }


            return warehouse;

        }

    }
}