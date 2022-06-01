using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data.SqlTypes;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;

namespace ConsoleGame
{
    class Program
    {
        static void Main(string[] args)
        {
            WorldRunner(); //Creates, starts and runs the game
        }
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        static void WorldRunner()
        {
            List<List<Dictionary<string, string>>> world = new List<List<Dictionary<string, string>>>(); //Creates new List<List<Dictionary<>>> for storing world information in a grid
            int roomWidth = 10, //Declaring room sizes
                roomLength = 20;
            WorldCreation(ref world, roomLength, roomWidth); //World Creation method
            Entity Player = PlayerCreation(); //Creates the player
            Entity Goblin = EnemyCreation(ref world, roomLength, roomWidth); //Creates a goblin that needs to be killed
            try
            {
                LoadGame(ref world, roomLength, roomWidth, Player, Goblin);
            }
            catch
            {
                Console.WriteLine("Save doesn't exists");
            }
            DrawMap(ref world, roomLength, roomWidth);
            Console.CursorVisible = false;  //Removes cursor visability in console
            GameOn(true, ref world, roomLength, roomWidth, Player, Goblin); //Runs the Game
        }
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        
        /// <summary>
        /// Creates a new tile based world with room height and length, can store any number off dictionary inputs on each tile
        /// </summary>
        /// <param name="world"></param>
        /// <param name="roomLength"></param>
        /// <param name="roomWidth"></param>
        static void WorldCreation(ref List<List<Dictionary<string, string>>> world, int roomLength, int roomWidth)
        {

            for (int x = 0; x < roomLength; x++)
            {
                var worldX = new List<Dictionary<string, string>>();
                for (int y = 0; y < roomWidth; y++)
                {
                    var worldY = new Dictionary<string, string>();
                    if (x == 0 || x == roomLength - 1 || y == 0 || y == roomWidth - 1) //Creates the world as empty and moveable except on the edges
                    {
                        worldY.Add("Type", "Wall");      //Unmoveable Tile
                        worldY.Add("Entity", "Occupied");
                        worldX.Add(worldY);
                    }
                    else
                    {

                        worldY.Add("Type", "Grass");    //Moveable Tile
                        worldY.Add("Entity", "None");
                        worldX.Add(worldY);
                    }
                }
                world.Add(worldX);

                //for (int y = 0; y < roomWidth; y++)
                //{
                //    Console.WriteLine($"{x}:{y} Type:" + world[x][y]["Type"]);
                //    Console.WriteLine($"{x}:{y} Entity:" + world[x][y]["Entity"]);
                //}
            }
        }
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        
        /// <summary>
        /// Creates the player character
        /// </summary>
        /// <returns></returns>
        static Entity PlayerCreation()
        {
            Entity Player = new Entity(); //Creates new player entity
            int playerClass;  //Creates int for player class
            Console.WriteLine("Your class choices are:");
            Console.WriteLine("1) Warrior");
            Console.WriteLine("2) Rogue");
            Console.Write("What class do you want to play as? (Enter the class number): ");
            string pClass = Console.ReadLine();
            while (true)
            {
                try
                {
                    playerClass = Convert.ToInt32(pClass); //Ensures player class is a digit
                    break;
                }
                catch
                {
                    Console.WriteLine("Please input a valid class number");
                    pClass = Console.ReadLine();
                    continue;
                }
            }

            while (true) {

                if (playerClass == 1) //If input is 1, class set to warrior
                {
                    Player.Class = ClassID.Warrior;
                    Player.EXP = 0;
                    Player.Health = Convert.ToInt32(ClassHealth.Warrior);
                    Player.Damage = Convert.ToInt32(ClassDamage.Warrior);
                    break;
                }
                else if (playerClass == 2) //If input is 2, player class is set to Rogue
                {
                    Player.Class = ClassID.Rogue;
                    Player.EXP = 0;
                    Player.Health = Convert.ToInt32(ClassHealth.Rogue);
                    Player.Damage = Convert.ToInt32(ClassDamage.Rogue);
                    break;
                }
                else
                {
                    Console.WriteLine("Defaulted to Warrior: number invalid"); //Otherwise class is set to warrior by default
                    Player.Class = ClassID.Warrior;
                    Player.EXP = 0;
                    Player.Health = Convert.ToInt32(ClassHealth.Warrior);
                    Player.Damage = Convert.ToInt32(ClassDamage.Warrior);
                    break;
                }
            }


            Console.WriteLine($"Class: {Player.Class}");        //Displays class info
            Console.WriteLine($"Health: {Player.Health}");
            Console.WriteLine($"Damage: {Player.Damage}");


            return Player;

        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        ///
        /// <summary>
        /// Creates a new entity named Goblin
        /// </summary>
        /// <param name="world"></param>
        /// <param name="roomLength"></param>
        /// <param name="roomWidth"></param>
        /// <returns>Entity</returns>
        static Entity EnemyCreation(ref List<List<Dictionary<string, string>>> world, int roomLength, int roomWidth)
        {
            Entity Goblin = new Entity(4, EnemyHealth.Goblin, EnemyDamage.Goblin, 5, 5); //Creates new entity named goblin
            world[Goblin.X][Goblin.Y]["Entity"] = "Goblin"; //Assigns goblin a spot in the world based on it's X n Y

            return Goblin;

        }
            
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        
        /// <summary>
        /// While loop that keeps the game going until player hits ESC key
        /// </summary>
        /// <param name="GameStarted"></param>
        /// <param name="world"></param>
        /// <param name="roomLength"></param>
        /// <param name="roomWidth"></param>
        /// <param name="Player"></param>
        /// <param name="Goblin"></param>
        static void GameOn(bool GameStarted, ref List<List<Dictionary<string, string>>> world, int roomLength, int roomWidth, Entity Player, Entity Goblin)
        {

            while (GameStarted) //Keeps player input available
            {
                int playerMoveX = 0, 
                    playerMoveY = 0;

                if (Console.KeyAvailable)
                {
                    ConsoleKeyInfo KeyPressed = Console.ReadKey(true); // Switch case for input keys
                    switch (KeyPressed.Key)
                    {
                        case ConsoleKey.LeftArrow: //Left arrow key moves left
                            {
                                playerMoveX -= 1;

                                break;
                            }
                        case ConsoleKey.RightArrow: //Right arrow key moves right
                            {
                                playerMoveX += 1;
                                break;
                            }
                        case ConsoleKey.UpArrow: //Up arrow key moves up
                            {
                                playerMoveY -= 1;
                                break;
                            }
                        case ConsoleKey.DownArrow: //Down arrow key moves down
                            {
                                playerMoveY += 1;
                                break;
                            }
                        case ConsoleKey.Spacebar: //Spacebar respawns the goblin
                            {
                                RespawnGoblin(ref world,ref Goblin, roomLength, roomWidth);
                                break;
                            }
                        case ConsoleKey.Escape: //ESC key exits the game
                            {
                                /* Exit game*/
                                SaveGame(ref world, roomLength, roomWidth);
                                GameStarted = false; 
                                break;
                            }
                        default:
                            continue; //Other keys don't do anything

                    }

                    //Updates all world entities after player input
                    EntityUpdator(ref world, roomLength, roomWidth, playerMoveX, playerMoveY, ref Player, ref Goblin, ref GameStarted);
                    if (Player.Health <= 0)
                    {
                        Console.WriteLine("You Died!");
                    }
                    //Displays player health/damage
                    Console.WriteLine($"Class: {Player.Class} EXP: {Player.EXP}");
                    Console.WriteLine($"Health: {Player.Health} Damage: {Player.Damage}");
                }
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        
        /// <summary>
        /// Updates all entities on the map 
        /// </summary>
        /// <param name="world"></param>
        /// <param name="roomLength"></param>
        /// <param name="roomWidth"></param>
        /// <param name="playerMoveX"></param>
        /// <param name="playerMoveY"></param>
        /// <param name="Player"></param>
        /// <param name="Goblin"></param>
        static void EntityUpdator(ref List<List<Dictionary<string, string>>> world,int roomLength, int roomWidth, int playerMoveX, int playerMoveY, ref Entity Player, ref Entity Goblin,ref bool GameStarted )
        {

            if (world[Player.X + playerMoveX][Player.Y + playerMoveY]["Type"] == "Wall") //If player input is towards a wall, displays you cannot move that direction
            {
                DrawMap(ref world, roomLength, roomWidth);
                Console.WriteLine("You cannot move that direction");

            }
            else if (world[Player.X + playerMoveX][Player.Y + playerMoveY]["Entity"] == "Goblin") //If player input is towards Goblin, player attacks goblin, then goblin attacks player
            {
                Goblin.Health -= Player.Damage;
                if (Goblin.Health <= 0)
                {
                    world[Goblin.X][Goblin.Y]["Entity"] = "None"; // if Goblin dies, then removes entity
                    Player.EXP += Goblin.EXP;
                    Console.Beep(500, 100);
                    Console.Beep(350, 75);
                    Console.Beep(200, 50);
                    DrawMap(ref world, roomLength, roomWidth);
                }
                else
                {
                    Player.Health -= Goblin.Damage;
                    if (Player.Health <= 0)
                    {
                        world[Player.X][Player.Y]["Entity"] = "None";
                        DrawMap(ref world, roomLength, roomWidth);
                        Console.WriteLine("You died!");
                        Console.Beep(500, 100);
                        Console.Beep(350, 75);
                        Console.Beep(200, 50);
                        Console.Beep(350, 75);
                        Console.Beep(350, 75);
                        GameStarted = false;
                    }
                    DrawMap(ref world, roomLength, roomWidth);
                }

            }
            else if (world[Player.X + playerMoveX][Player.Y + playerMoveY]["Entity"] == "None") //If spot is empty, player moves
            {
                world[Player.X][Player.Y]["Entity"] = "None";
                Player.X += playerMoveX;
                Player.Y += playerMoveY;
                world[Player.X][Player.Y]["Entity"] = "Player";
                DrawMap(ref world, roomLength, roomWidth);

            }

        }
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Draws the map when given a world
        /// </summary>
        /// <param name="world"></param>
        /// <param name="roomLength"></param>
        /// <param name="roomWidth"></param>
        static void DrawMap(ref List<List<Dictionary<string, string>>> world, int roomLength, int roomWidth)
        {
            Console.Clear(); //Clears Console
            Console.WriteLine();
            for (int y = 0; y < roomWidth; y++)
            {
                for (int x = 0; x < roomLength; x++)
                {
                    if(world[x][y]["Type"] == "Wall") //If tile is wall, print gray square on green
                    {
                        DrawPixel("■",ConsoleColor.Green, ConsoleColor.DarkGray);
                    }
                    else if(world[x][y]["Entity"] == "Player") //If tile is player, print yellow p on green
                    {
                        DrawPixel("P",ConsoleColor.Green, ConsoleColor.Yellow);
                    }
                    else if(world[x][y]["Entity"] == "Goblin") //If tile is goblin, print darkblue G on green
                    {
                        DrawPixel("G",ConsoleColor.Green, ConsoleColor.DarkBlue);
                    }
                    else if (world[x][y]["Type"] == "Grass") //If tile is Grass, print green square with no forground
                    {
                        DrawPixel(" ",ConsoleColor.Green, ConsoleColor.Black);
                    }
                }
                if (y == 2)
                {
                    Console.Write($"    Arrow Keys to Move"); //Displays Controls to the side of the frame
                }
                else if (y == 3)
                {
                    Console.Write($"    Space to spawn a new goblin");
                }
                else if (y == 4)
                {
                    Console.Write($"    ESC to end the game and prompt Save");
                }
                Console.WriteLine();
            }
        }
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Draws a single pixel based on world world information
        /// </summary>
        /// <param name="cha"></param>
        /// <param name="col1"></param>
        /// <param name="col2"></param>
        static void DrawPixel(string cha, ConsoleColor col1, ConsoleColor col2)
        {
            Console.BackgroundColor = col1; //Sets colors, displays input characters, then resets colors to normal
            Console.ForegroundColor = col2;
            Console.Write($"{cha}");
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.White;
        }
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Respawns the goblin enemy when space is hit
        /// </summary>
        /// <param name="world"></param>
        /// <param name="Goblin"></param>
        /// <param name="roomLength"></param>
        /// <param name="roomWidth"></param>
        static void RespawnGoblin(ref List<List<Dictionary<string, string>>> world,ref Entity Goblin, int roomLength, int roomWidth)
        {
            world[Goblin.X][Goblin.Y]["Entity"] = "None"; //Respawns the goblin in a new random square in the room
            Goblin.Health = Convert.ToInt32(EnemyHealth.Goblin);
            var rand = new Random();
            Goblin.X = rand.Next(1, roomLength -2);
            Goblin.Y = rand.Next(1, roomWidth -2);
            world[Goblin.X][Goblin.Y]["Entity"] = "Goblin";
            DrawMap(ref world, roomLength, roomWidth);
        }
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Saves the game with player given file name
        /// </summary>
        /// <param name="world"></param>
        /// <param name="roomLength"></param>
        /// <param name="roomWidth"></param>
        static void SaveGame(ref List<List<Dictionary<string, string>>> world, int roomLength, int roomWidth)
        {
            Console.Write("Do you want to save the game? [Y/N]: "); //Promts the user if they want to save
            string save = Console.ReadLine();
            while (true) { 
                if (save == "Y")
                {
                    using (StreamWriter file = new StreamWriter("save.txt")) //Creates a file named "save.txt" with all tile data, only saving position, not Entity information
                    {
                        for (int x = 0; x < roomLength; x++) 
                        {
                            for (int y = 0; y < roomWidth; y++)
                            {
                                file.WriteLine(world[x][y]["Type"]);
                                file.WriteLine(world[x][y]["Entity"]);
                            }
                        }
                    }

                    break;
                }
                else if (save == "N") //Else, do nothing
                {
                    break;
                }
                else
                {
                    Console.Write("Please input Y or N: "); //Keeps loopn while input is not "Y" or "N"
                    save = Console.ReadLine();
                    continue;
                }
            }
        }
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Loads game from "save.txt" file if it exists
        /// </summary>
        /// <param name="world"></param>
        /// <param name="roomLength"></param>
        /// <param name="roomWidth"></param>
        /// <param name="Player"></param>
        /// <param name="Goblin"></param>
        static void LoadGame(ref List<List<Dictionary<string, string>>> world, int roomLength, int roomWidth, Entity Player, Entity Goblin)
        {
            for (int x = 0; x < roomLength; x++)
            {
                for (int y = 0; y < roomWidth; y++)
                {
                    world[x][y]["Type"] = File.ReadLines("save.txt").Skip((2 * y) + (roomWidth*2*x)).Take(1).First();   //Replaces all default world data with saved data
                    world[x][y]["Entity"] = File.ReadLines("save.txt").Skip((2 * y) + ((roomWidth*2*x)) +1).Take(1).First();

                }
            }
            for (int x = 0; x < roomLength; x++)
            {
                for (int y = 0; y < roomWidth; y++)
                {
                    
                    if (world[x][y]["Entity"] == "Player") //If player, update player.coordinate info to match loaded file
                    {
                        world[Player.X][Player.Y]["Entity"] = "None";
                        Player.X = x;
                        Player.Y = y;
                        world[Player.X][Player.Y]["Entity"] = "Player";

                    }
                    else if (world[x][y]["Entity"] == "Goblin") //If goblin, update goblin.coordinate info to match loaded file
                    {
                        world[Goblin.X][Goblin.Y]["Entity"] = "None";
                        Goblin.X = x;
                        Goblin.Y = y;
                        world[x][y]["Entity"] = "Goblin";
                    }
                    //Console.WriteLine($"{x}:{y} Type:" + world[x][y]["Type"]);
                    //Console.WriteLine($"{x}:{y} Entity:" + world[x][y]["Entity"]);

                }
            }
        }
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    }

    public enum ClassID {None = 0, Warrior = 1, Rogue = 2} //Class IDs
    public enum ClassDamage {Warrior = 3, Rogue = 5} // Class Damages
    public enum ClassHealth {Warrior = 15, Rogue = 10} // Class Health
    public enum EnemyDamage {Goblin = 3} // Enemy Damage
    public enum EnemyHealth {Goblin = 8} // Enemy Health
    public class Entity
    {
        public ClassID Class { get; set; }
        public int EXP { get; set; }
        public dynamic Health { get; set; }
        public dynamic Damage { get; set; }
        public int X { get; set; }
        public int Y { get; set; }

        public Entity() //Blank struct
        {
            Class = ClassID.None;
            EXP = 0;
            Health = 1;
            Damage = 1;
            X = 1;
            Y = 1;
        }

        public Entity(ClassID eClass, int eEXP, ClassHealth eHealth, ClassDamage eDamage, int eX, int eY) //Default struct for a player instance
        {
            Class = eClass;
            EXP = eEXP;
            Health = eHealth;
            Damage = eDamage;
            X = eX;
            Y = eY;
        }

        public Entity(int eEXP, EnemyHealth eHealth, EnemyDamage eDamage, int eX, int eY) //Default struct for enemy instance
        {
            Class = ClassID.None;
            EXP = eEXP;
            Health = eHealth;
            Damage = eDamage;
            X = eX;
            Y = eY;
        }

    }
}
