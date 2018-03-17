﻿using System;
using UnityEngine;
using System.Collections.Generic;

namespace InitGuns
{
    class InitRandomGuns
    {
        static int clustering = 50;
        static int towncluster = 150;
        static System.Random rand = new System.Random();

        // Spaces Occupied By other Objects
        List<WeaponSpell> OccupiedSpaces = new List<WeaponSpell>();

        // The array of coordinates for each weapon
        List<WeaponSpell> SpawnedGuns = new List<WeaponSpell>();

        // An array of areas of interest to preferentially scatter guns around
        List<WeaponSpell> HotSpots = new List<WeaponSpell>();

        // An array of bytes to be initialized
        public byte[] pcktarray;

        // Constructor that creates an InitRandomGuns Object

        // Empty Initializtion from pulling from byte array
        public InitRandomGuns()
        {
        }

        public InitRandomGuns(int NoPlayers, List<Vector2> OccupiedCoordinates)
        {
            // Add each of the Occupied Coordinates to the local OccupiedSpaces List
            if (OccupiedCoordinates != null)
            {
                // foreach (var w in OccupiedCoordinates)
                // {
                //     OccupiedSpaces.Add(WeaponSpell((int) w.X,(int) w.Y));
                // }
            }

            // Add some dummy hotspots NOTE MUST BE AT LEAST clustering AWAY FROM EDGE IN EACH DIRECTION
            HotSpots.Add(new WeaponSpell(250, 250));
            HotSpots.Add(new WeaponSpell(800, 550));
            HotSpots.Add(new WeaponSpell(750, 750));
            HotSpots.Add(new WeaponSpell(900, 100));
            HotSpots.Add(new WeaponSpell(200, 900));

            WeaponSpell Weapon;
            int counter = 0;
            int interestTracker = 0;

            // Keep generating guns depending on number of players
            while (counter < numberOfWeapons(NoPlayers))
            {
                if (counter < numberOfWeapons(NoPlayers) / 4)
                {
                    Weapon = new WeaponSpell(rand.Next(500 - towncluster, 501 + towncluster),
                                             rand.Next(500 - towncluster, 501 + towncluster), true);

                }
                else
                {
                    if (rand.NextDouble() > 0.5)
                    {
                        Weapon = new WeaponSpell(rand.Next(0, 1001),
                            rand.Next(0, 1001), true);
                    }
                    else
                    {
                        // Choose a Hotspot at Random
                        interestTracker = rand.Next(0, HotSpots.Count);

                        int a = rand.Next(HotSpots[interestTracker].X - clustering, HotSpots[interestTracker].X + clustering);
                        int b = rand.Next(HotSpots[interestTracker].Z - clustering, HotSpots[interestTracker].Z + clustering);

                        Weapon = new WeaponSpell(a, b, true);
                    }
                }

                // Check if the generated coordinate is already occupied
                // if no add to coordinate array
                if (!OccupiedCheck(Weapon, OccupiedSpaces))
                {
                    counter++;
                    SpawnedGuns.Add(Weapon);
                    OccupiedSpaces.Add(Weapon);
                }
            }

            // Create a byte array with those coordinates
            getByteArray();
        }

        // Basic Coordinates Printed
        public void printCoordinates()
        {
            foreach (var w in SpawnedGuns)
            {
                Console.Write(w.BasicString() + ",");
            }
        }

        // Coordinates Printed with type and ID
        public void printExpandedCoordinates()
        {
            foreach (var w in SpawnedGuns)
            {
                Console.Write(w.ExtendedString() + ",");
            }
        }

        // Take a byteArray and fill spawnedGuns list from bytearray
        public void fromByteArrayToList(byte[] transmittedBytes)
        {
            int size = transmittedBytes.Length / 13;
            int count = 0;
            byte[] tempw = new byte[13];
            SpawnedGuns.Clear();

            for (int i = 0; i < size; i++)
            {
                Buffer.BlockCopy(transmittedBytes, count, tempw, 0, 13);
                WeaponSpell tempwpn = GetWeaponFromBytes(tempw);
                SpawnedGuns.Add(tempwpn);
                count += 13;
            }
        }

        // Output the list of weapons as a bytearray
        public void getByteArray()
        {
            pcktarray = new byte[13 * SpawnedGuns.Count];
            byte[] warray;
            int offset = 0;

            foreach (var w in SpawnedGuns)
            {
                warray = PutWeaponIntoBytes(w);
                Buffer.BlockCopy(warray, 0, pcktarray, offset, 13);
                offset += 13;
            }
        }

        public class WeaponSpell
        {
            public static int inc = 0;
            public int X { get; set; }
            public int Z { get; set; }
            public int ID { get; set; }
            public byte Type { get; set; }

            // Empty Constructor
            public WeaponSpell()
            {
            }

            // Plain Coordinate Constructor
            public WeaponSpell(int X, int Z)
            {
                this.X = X;
                this.Z = Z;
            }

            // Coordinate Constructor with Types
            public WeaponSpell(int X, int Z, bool flag)
            {
                this.X = X;
                this.Z = Z;
                this.Type = GenGunType();
                this.ID = inc;
                inc++;
            }

            // Check if two sets of coordinates match
            public bool CoordinateMatch(WeaponSpell C)
            {
                if (this.X == C.X && this.Z == C.Z)
                {
                    return true;
                }
                return false;
            }

            // Basic String output
            public string BasicString()
            {
                return "(" + X + "," + Z + ")";
            }

            // Extended String output with weapon/spell type
            public string ExtendedString()
            {
                return "(ID:" + ID + " Type:" + Type + "," + X + "," + Z + ")";
            }

            // Randomly Generate a type of gun
            private byte GenGunType()
            {
                // Randomly seeded values
                double seed1 = rand.NextDouble();
                double seed2 = rand.NextDouble();

                // Gun Spawn 35% chance
                if (seed1 > 0.65)
                {
                    if (seed2 > 0.5)
                    {
                        //type 1 -- 50% chance
                        return 1;
                    }
                    else if (seed2 > 0.15)
                    {
                        //type = 2 -- 35% chance
                        return 2;
                    }
                    else
                    {
                        //type = 3 -- 15% chance
                        return 3;
                    }
                }

                // Spell Spawn 65% chance -- 10% chance per spell
                else
                {
                    if (seed2 > 0.9)
                    {
                        return 4;
                    }
                    else if (seed2 > 0.8)
                    {
                        return 5;
                    }
                    else if (seed2 > 0.7)
                    {
                        return 6;
                    }
                    else if (seed2 > 0.6)
                    {
                        return 7;
                    }
                    else if (seed2 > 0.5)
                    {
                        return 8;
                    }
                    else if (seed2 > 0.4)
                    {
                        return 9;
                    }
                    else if (seed2 > 0.3)
                    {
                        return 10;
                    }
                    else if (seed2 > 0.2)
                    {
                        return 11;
                    }
                    else if (seed2 > 0.1)
                    {
                        return 12;
                    }
                    else
                    {
                        return 13;
                    }
                }
            }
        }

        // Check if a certain set of spaces is occupied
        public static bool OccupiedCheck(WeaponSpell genC, List<WeaponSpell> Occupied)
        {
            //TODO if coordinates occupied in terrain check return false
            return Occupied.Contains(genC);
        }

        // Takes a Weapon Object and puts it into byte array format
        public static byte[] PutWeaponIntoBytes(WeaponSpell Weapon)
        {
            byte[] wpn = new byte[13];

            byte[] type = new byte[sizeof(byte)];
            type[0] = Weapon.Type;
            Buffer.BlockCopy(type, 0, wpn, 0, 1);

            byte[] ID = BitConverter.GetBytes(Weapon.ID);
            Buffer.BlockCopy(ID, 0, wpn, 1, sizeof(int));

            byte[] X = BitConverter.GetBytes(Weapon.X);
            Buffer.BlockCopy(X, 0, wpn, 5, sizeof(int));

            byte[] Z = BitConverter.GetBytes(Weapon.Z);
            Buffer.BlockCopy(Z, 0, wpn, 9, sizeof(int));

            return wpn;
        }

        // Gets a weapon type from a bytearray format weapon
        public WeaponSpell GetWeaponFromBytes(byte[] weaponinbytes)
        {
            WeaponSpell Weapon = new WeaponSpell();

            Weapon.Type = weaponinbytes[0];

            byte[] ID = new byte[4];
            Weapon.ID = BitConverter.ToInt32(weaponinbytes, 1);

            byte[] X = new byte[4];
            Weapon.X = BitConverter.ToInt32(weaponinbytes, 5);

            byte[] Z = new byte[4];
            Weapon.Z = BitConverter.ToInt32(weaponinbytes, 9);

            return Weapon;
        }

        // Generates a number of guns depending on the number of players
        public static int numberOfWeapons(int players)
        {
            return players * 4 * 2;
        }


        /*
        // Example Usage is below
        public static void Main(string[] args)
        {
            // Create a bunch of spawned gun coordinates for 10 players
            // second coordinate should be a list of occupied spaces might need slight reworking
            InitRandomGuns guns = new InitRandomGuns(10,null);
            guns.printExpandedCoordinates();

            Console.WriteLine("");
            Console.WriteLine("");
            Console.WriteLine("");
            Console.WriteLine("");

            // Create a second empty InitRandomGuns object
            InitRandomGuns guns2 = new InitRandomGuns();

            // Grab and process a byte array and put into coordinate list
            // in this case I am directly grabbing first objects packet array
            guns2.fromByteArrayToList(guns.pcktarray);
            guns2.printExpandedCoordinates();


            Console.Write("");
        }
        */

    }
}