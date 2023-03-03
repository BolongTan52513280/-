using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace CarSimulator
{


    public class main
    {
        public static int NUMBER_OF_BOTS = 1;
        public static List<int[]> robotLocationTracker = new List<int[]>(NUMBER_OF_BOTS);
        public static int CHARGE_DURATION = 10000;
        public static int WAREHOUSE_X = 5;
        public static int WAREHOUSE_Y = 7;



        private static Mutex mut = new Mutex();

        static void Main(string[] args)
        {
            Warehouse depot = warehouseGenerator();


            //this is multithreaded... idk if it works but it looks like it so far
            int[][] robot_loc = new int[NUMBER_OF_BOTS][];     //int[][,,,] robot_loc????
            int[][] instructions = new int[NUMBER_OF_BOTS][];    //!!!

            Thread[] robots = new Thread[NUMBER_OF_BOTS];
            for (int i = 0; i < NUMBER_OF_BOTS; i++)
            {
                int x = i;
                robot_loc[x] = new int[2];
                instructions[x] = new int[7];
                robots[x] = new Thread(() => robotOp(WAREHOUSE_X, WAREHOUSE_Y, x, depot, robot_loc[x], instructions[x]));
                robots[x].Start();
            }



            // robotOp(WAREHOUSE_X, WAREHOUSE_Y, 0, depot);

        }




        public static void robotOp(int warehouse_x, int warehouse_y, int robotNumber, Warehouse warehouse, int[] robot_loc, int[] instructions)
        {
            //manual stop input from manager interface
            bool stop = false;
            Robot drone = new Robot();

            //add robot location to list immediately
            drone.setLocation(0, robotNumber, 0, 0);
            robotLocationTracker.Add(drone.giveLocation());

            //need to figure out truck stuff, prob make a truck function and embed into robots
            //no, need to make truck stuff and send info to bots
            //instead of truck stuff here, should just be info from global variables.. hmmge
            Truck[] truckAtDoc = new Truck[3];

            //temp truck testing stuff
            for (int i = 0; i < 3; i++)
            {
                truckAtDoc[i] = new Truck();
                truckAtDoc[i].dock(true);
            }
            int truck_loc = 2;  //????





            //initial parameters robot needs
            drone.setWarehouseSize(warehouse_x, warehouse_y);

            //main loop
            //some sort of stop command if needed to shut robot off
            while (!stop)
            {
                //check if needs charging
                chargingOperation(drone, robot_loc, robotNumber);

                //main movement and loading/delivering logic
                //if drone arrived where it's supposed to be and in queue, give new instructions
                if (drone.checkArrive() && drone.checkIfQueued())
                {
                    Boolean stopasking = false; //used to check if there is an item at desired location
                    while (!stopasking)
                    {
                        mut.WaitOne();
                        getInput(robotNumber, instructions);
                        stopasking = warehouse.checkspecificshelf(instructions[0], instructions[1], instructions[2], instructions[3]);
                        if (instructions[4] == 2 && !stopasking)
                        {
                            stopasking = true;
                        }
                        if (instructions[4] == 1 && stopasking)
                        {
                            stopasking = true;
                        }
                        mut.ReleaseMutex();
                        drone.setStatus(2);
                    }
                }


                //instruction 1 = going to take items from the truck and store on the shelf
                //instruction 2 = going to take items from the shelf and store on the truck
                if (instructions[4] == 1)
                {
                    //instructions[5] - 1 = dock # 
                    truckTospecificShelfOperation(drone, truckAtDoc[truck_loc], warehouse, instructions, robot_loc, robotNumber);

                }
                else if (instructions[4] == 2)
                {
                    shelfToTruckOperation(drone, truckAtDoc[truck_loc], warehouse, instructions, robot_loc, robotNumber);

                }
                else
                {
                    Console.WriteLine("error, neither operation chosen");
                }

                //operation finished return to start
                drone.setLocation(0, 0, 0, 0);
                drone.setStatus(2);
                robotPathfinding(drone, robot_loc, robotNumber);
                drone.setStatus(0);

                Console.WriteLine("robot #{0} made it through one loop", robotNumber);

                Console.WriteLine("");

                warehouse.showItem();

            }


        }


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



        //NOT FULLY DONE ! ! ! CHECK FUNCTION FOR WHAT NEEDS TO BE CHANGED ! ! !
        //instructions for going from the truck to the shelves
        public static void truckTospecificShelfOperation(Robot currentBot, Truck currentTruck, Warehouse warehouse, int[] ins, int[] loc, int robotID)
        {
            //truck location
            currentBot.setLocation(ins[5], ins[6], 0, 0); // meet with truck first 
            currentBot.setStatus(2);    // moving in the warehouse
            robotPathfinding(currentBot, loc, robotID);
            //now at the truck
            currentBot.setStatus(1);

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
            //to make the robot go to multiple locations, we would just update the instructions with the new shelf locations <<<<<<------------------------------------ NOT DONE
            //and have the path that was originally set keep track of all the locations the bot is supposed to go
            bool done = false;

            do
            {
                currentBot.setLocation(ins[0], ins[1], ins[2], ins[3]);
                currentBot.setStatus(2);
                robotPathfinding(currentBot, loc, robotID);

                //at the shelf now
                currentBot.setStatus(3);


                //possible problems of shelf being full but I will ignore that for now...
                //don't need to lock because robots will never be in the same spot... hopefully
                warehouse.addItemToLocation(ins[0], ins[1], ins[2], ins[3], currentBot.unloadItem(0, robotID));


                //some sort of logic to determine if need to go to more shelves <<<<<<------------------------------------ NOT DONE
                done = true; //temporary until logic is implemented

            } while (!done);

        }

        //operation for moving items from shelf and loading on to truck 
        //prob need to include a force leave into parameters and a return value for if truck leaves <<<<<<------------------------------------ NOT DONE
        public static void shelfToTruckOperation(Robot currentBot, Truck currentTruck, Warehouse warehouse, int[] ins, int[] loc, int robotID)
        {

            //go to shelf 
            //to make the robot go to multiple locations, we would just update the instructions with the new shelf locations <<<<<<------------------------------------ NOT DONE
            //and have the path that was originally set keep track of all the locations the bot is supposed to go
            bool done = false;

            do
            {
                //shelf location
                currentBot.setLocation(ins[0], ins[1], ins[2], ins[3]);
                currentBot.setStatus(2);
                robotPathfinding(currentBot, loc, robotID);
                //now at the shelf
                currentBot.setStatus(3);



                //prob need to lock the warehouse shelves just to be safe.. but technically no robot will access the same shelf at the same time
                Item retrievedItem = warehouse.removeItemFromLocation(ins[0], ins[1], ins[2], ins[3]);
                if (retrievedItem != null)
                {
                    currentBot.loadItem(retrievedItem);
                }
                else
                {
                    Console.WriteLine("no item at location, moving on");
                }


                //some sort of logic to determine if need to go to more shelves <<<<<<------------------------------------ NOT DONE
                //some func update location probably needed to go to new shelves

                done = true; //temporary until logic is implemented

            } while (!done && !currentBot.isFull());

            //go to truck
            //truck location
            currentBot.setLocation(ins[5], ins[6], 0, 0);
            currentBot.setStatus(2);
            robotPathfinding(currentBot, loc, robotID);
            //now at the truck
            currentBot.setStatus(1);

            int check = 0;

            //should be variable from bool array to force trucks to leave sent from manager UI
            bool leave = false;// <<<<<<------------------------------------ NOT DONE

            //unloading bot into truck
            do
            {
                check = unloadRobotToTruck(currentBot, currentTruck, leave,robotID);
            } while (check == 0);

            //check value might be useful for central cpu, can return it if needed
            if (check == 1)
            {
                Console.WriteLine("robot is empty, robot moving");
            }
            //should include something for this about the  truck leaving for truck function <<<<<<------------------------------------ NOT DONE
            else if (check == 2)
            {
                Console.WriteLine("truck is full, leaving dock");
            }
            else
            {
                Console.WriteLine("finished loading but unsure why...? ERROR DETECTED");
            }

        }



        //update the location of the robot
        public static void updateLocationList(int[] location, int spot) //spot is robot id 
        {
            //NEED TO LOCK THIS ! ! !
            mut.WaitOne();
            robotLocationTracker.RemoveAt(spot);        //!!!!
            robotLocationTracker.Insert(spot, location);
            mut.ReleaseMutex();
        }

        //this is temporarily getting console input
        //need to change this to get input from the UI
        //instrutions 0 = shelf x, 1 = shelf y, 2 = left or right, 3 = shelf,
        //4 = type of instruction, 5 = truck x, 6 = truck y
        public static void getInput(int robotID, int[] instructions)
        {
            Console.WriteLine("Give commands to robot #{0}", robotID);
            Console.WriteLine("Enter the commands for the robot:");
            Console.WriteLine("1. Unload truck -> Shelf      2. Unload shelf -> truck");
            instructions[4] = Int32.Parse(Console.ReadLine());

            //truck y will always be 0
            Console.WriteLine("Enter the truck dock location");
            Console.WriteLine("X axis: ");
            instructions[5] = Int32.Parse(Console.ReadLine());
            instructions[6] = 0;

            Console.WriteLine("Enter the a specific location in warehouse");
            Console.WriteLine("X should be <{0}", WAREHOUSE_X);
            Console.WriteLine("X axis: ");
            instructions[0] = Int32.Parse(Console.ReadLine());
            Console.WriteLine("Y should be <{0}", WAREHOUSE_Y);
            Console.WriteLine("Y axis: ");
            instructions[1] = Int32.Parse(Console.ReadLine());
            Console.WriteLine("enter 0 for Left    enter 1 for Right");
            instructions[2] = Int32.Parse(Console.ReadLine());
            Console.WriteLine("shelf should be <4 for now ");
            Console.WriteLine("Shelf #: ");
            instructions[3] = Int32.Parse(Console.ReadLine());
        }

        //robot auto pathing and updating info
        public static void robotPathfinding(Robot currentBot, int[] robot_location, int robotID)
        {
            do
            {
                //get the robots current location, update it's position, and get the new updated postions of other robots
                robot_location = currentBot.giveLocation();
                updateLocationList(robot_location, robotID);
                currentBot.getRobotLocations(robotLocationTracker);   //?????
                mut.WaitOne();
                currentBot.pathFinding();      // output at destination
                mut.ReleaseMutex();
                Console.WriteLine("robot is at location x: {0}, location y: {1} now", robot_location[0], robot_location[1]);
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

        public static Warehouse warehouseGenerator()
        {
            Warehouse warehouse = new Warehouse(WAREHOUSE_X, WAREHOUSE_Y, 2, 4);//the number here is the real number of row....
                                                                                // Location loc = new Location(3, 3, 3, 3);
           

            Random rnd = new Random();

            Item item1 = new Item(5, 1, "Arduino", "1");
            item1.setlocation(0, 0, 0, 0);
            warehouse.spot[0, 0, 0, 0] = item1;

            Item item2 = new Item(5, 1, "Arduino", "1");
            item2.setlocation(1, 0, 0, 0);
            warehouse.spot[1, 0, 0, 0] = item1;

            Item item3 = new Item(5, 1, "Arduino", "1");
            item3.setlocation(2, 0, 0, 0);
            warehouse.spot[2, 0, 0, 0] = item1;

            Item item4 = new Item(5, 1, "Arduino", "1");
            item4.setlocation(3, 0, 0, 0);
            warehouse.spot[3, 0, 0, 0] = item1;

            Item item5 = new Item(5, 1, "Arduino", "1");
            item5.setlocation(4, 0, 0, 0);
            warehouse.spot[4, 0, 0, 0] = item5;

            // 10 de1
            Item item6 = new Item(6, 1, "DE1-SoC", "2");
            item6.setlocation(0, 1, 0, 0);
            warehouse.spot[0, 1, 0, 0] = item6;

            Item item7 = new Item(6, 1, "DE1-SoC", "2");
            item7.setlocation(1, 1, 0, 0);
            warehouse.spot[1, 1, 0, 0] = item7;

            Item item8 = new Item(6, 1, "DE1-SoC", "2");
            item8.setlocation(2, 1, 0, 0);
            warehouse.spot[2, 1, 0, 0] = item8;

            Item item9 = new Item(6, 1, "DE1-SoC", "2");
            item9.setlocation(3, 1, 0, 0);
            warehouse.spot[3, 1, 0, 0] = item9;

            Item item10 = new Item(6, 1, "DE1-SoC", "2");
            item10.setlocation(4, 1, 0, 0);
            warehouse.spot[4, 1, 0, 0] = item10;

            Item item11 = new Item(6, 1, "DE1-SoC", "2");
            item11.setlocation(5, 1, 0, 0);
            warehouse.spot[5, 1, 0, 0] = item11;

            Item item12 = new Item(6, 1, "DE1-SoC", "2");
            item12.setlocation(6, 1, 0, 0);
            warehouse.spot[6, 1, 0, 0] = item12;

            Item item13 = new Item(6, 1, "DE1-SoC", "2");
            item13.setlocation(7, 1, 0, 0);
            warehouse.spot[7, 1, 0, 0] = item13;

            Item item14 = new Item(6, 1, "DE1-SoC", "2");
            item14.setlocation(8, 1, 0, 0);
            warehouse.spot[8, 1, 0, 0] = item14;

            Item item15 = new Item(6, 1, "DE1-SoC", "2");
            item15.setlocation(9, 1, 0, 0);
            warehouse.spot[9, 1, 0, 0] = item15;

            //8 hp 

            Item item16 = new Item(7, 2, "Hp-prime", "3");
            item16.setlocation(0, 2, 0, 0);
            warehouse.spot[0, 2, 0, 0] = item16;

            Item item17 = new Item(7, 2, "Hp-prime", "3");
            item17.setlocation(1, 2, 0, 0);
            warehouse.spot[1, 2, 0, 0] = item17;

            Item item18 = new Item(7, 2, "Hp-prime", "3");
            item18.setlocation(2, 2, 0, 0);
            warehouse.spot[2, 2, 0, 0] = item18;

            Item item19 = new Item(7, 2, "Hp-prime", "3");
            item19.setlocation(3, 2, 0, 0);
            warehouse.spot[3, 2, 0, 0] = item19;

            Item item20 = new Item(7, 2, "Hp-prime", "3");
            item20.setlocation(4, 2, 0, 0);
            warehouse.spot[4, 2, 0, 0] = item20;

            Item item21 = new Item(7, 2, "Hp-prime", "3");
            item21.setlocation(5, 2, 0, 0);
            warehouse.spot[5, 2, 0, 0] = item21;

            Item item22 = new Item(7, 2, "Hp-prime", "3");
            item22.setlocation(6, 2, 0, 0);
            warehouse.spot[6, 2, 0, 0] = item22;

            Item item23 = new Item(7, 2, "Hp-prime", "3");
            item23.setlocation(7, 2, 0, 0);
            warehouse.spot[7, 2, 0, 0] = item23;


            //icliker 4

            Item item24 = new Item(8, 2, "Icliker", "4");
            item24.setlocation(0, 3, 0, 0);
            warehouse.spot[0, 3, 0, 0] = item24;

            Item item25 = new Item(8, 2, "Hp-prime", "4");
            item25.setlocation(1, 3, 0, 0);
            warehouse.spot[1, 3, 0, 0] = item25;

            Item item26 = new Item(8, 2, "Hp-prime", "4");
            item26.setlocation(2, 3, 0, 0);
            warehouse.spot[2, 3, 0, 0] = item26;

            Item item27 = new Item(8, 2, "Hp-prime", "4");
            item27.setlocation(3, 3, 0, 0);
            warehouse.spot[3, 3, 0, 0] = item27;

            // multiplier   2

            Item item28 = new Item(9, 2, "Multiplier", "5");
            item28.setlocation(0, 4, 0, 0);
            warehouse.spot[0, 4, 0, 0] = item28;

            Item item29 = new Item(9, 2, "Multiplier", "5");
            item29.setlocation(1, 4, 0, 0);
            warehouse.spot[1, 4, 0, 0] = item29;

            //PYNQz1

            Item item30 = new Item(10, 2, "PYNQ-z1", "6");
            item30.setlocation(0, 5, 0, 0);
            warehouse.spot[0, 5, 0, 0] = item30;

            Item item31 = new Item(10, 2, "PYNQ-z1", "6");
            item30.setlocation(1, 5, 0, 0);
            warehouse.spot[1, 5, 0, 0] = item30;

            Item item32 = new Item(10, 2, "PYNQ-z1", "6");
            item32.setlocation(2, 5, 0, 0);
            warehouse.spot[2, 5, 0, 0] = item32;

            Item item33 = new Item(10, 2, "PYNQ-z1", "6");
            item33.setlocation(3, 5, 0, 0);
            warehouse.spot[3, 5, 0, 0] = item33;

            Item item34 = new Item(10, 2, "PYNQ-z1", "6");
            item34.setlocation(4, 5, 0, 0);
            warehouse.spot[4, 5, 0, 0] = item34;


            warehouse.showItem();
            int num = warehouse.numberofitem();
            Console.WriteLine("There are {0} items in the warehouse", num);
            int sumlocation = warehouse.overalllocation();
            Console.WriteLine("There are {0} locations in the warehouse", sumlocation);
            int locationleft = warehouse.locationleft();
            Console.WriteLine("There are {0} locations left in the warehouse", locationleft);


            return warehouse;

        }

    }
}