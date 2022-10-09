using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System;
using Random = System.Random;

namespace MAPGEN
{
    public class TileMapGen
    {
        // Start is called before the first frame update
        public static int[,] GenerateArray(int width, int height, bool empty)
        {
            int[,] map = new int[width, height];
            Random rnd = new Random();
            int holes = rnd.Next(3, 5);
            int xR = 0;
            bool[] empArr = new bool[width];
            int yoffset = 0;
            for (int h = 0; h < holes; h++)
            {
                int dist = rnd.Next(4, 8);
                int offset = rnd.Next(5, 15);
                xR += offset;
                for(int i = xR;i< xR + dist; i++)
                {
                    if(i<width)
                        empArr[i] = true;
                }
                xR += offset;
            }
            
            foreach(bool a in empArr)
                Debug.Log("bool " + a);

            for (int x = 0; x < map.GetUpperBound(0); x++)
            {
                for (int y = 0; y < map.GetUpperBound(1); y++)
                {

                    //Debug.Log(xR + ", " + offset);
                    if(x-1 > 0 && x + 1 < width)
                    {
                        if (empArr[x-1] && empArr[x + 1])
                        {
                            yoffset = rnd.Next(-2, 5);
                        }
                    }
                    
                    if (empArr[x])
                    {
                        map[x, y] = 0;
                    }
                    else if(y>height/2 + yoffset)
                    {

                        map[x, y] = 0;
                    }
                    else
                    {
                        map[x, y] = 1;
                    }
                        
                    
                    
                    
                    
                }
            }
            return map;
        }

        public static void RenderMap(int[,] map, Tilemap tilemap, TileBase tile)
        {
            //Clear the map (ensures we dont overlap)
            tilemap.ClearAllTiles();
            int width = map.GetUpperBound(0);
            int height = map.GetUpperBound(1);
            //Loop through the width of the map
            for (int x = 0; x < width; x++)
            {
                //Loop through the height of the map
                for (int y = 0; y < height; y++)
                {
                    // 1 = tile, 0 = no tile
                    if (map[x, y] == 1)
                    {
                        //Debug.Log(y +" "+ height);
                        
                        tilemap.SetTile(new Vector3Int(x , y - (height), 0), tile);
                    }
                }
            }
        }

        public static void UpdateMap(int[,] map, Tilemap tilemap) //Takes in our map and tilemap, setting null tiles where needed
        {
            for (int x = 0; x < map.GetUpperBound(0); x++)
            {
                for (int y = 0; y < map.GetUpperBound(1); y++)
                {
                    //We are only going to update the map, rather than rendering again
                    //This is because it uses less resources to update tiles to null
                    //As opposed to re-drawing every single tile (and collision data)
                    if (map[x, y] == 0)
                    {
                        tilemap.SetTile(new Vector3Int(x, y, 0), null);
                    }
                }
            }
        }

        public static int[,] PerlinNoise(int[,] map, float seed)
        {
            int newPoint;
            //Used to reduced the position of the Perlin point
            float reduction = 0.5f;
            //Create the Perlin
            for (int x = 0; x < map.GetUpperBound(0); x++)
            {
                newPoint = Mathf.FloorToInt((Mathf.PerlinNoise(x, seed) - reduction) * map.GetUpperBound(1));

                //Make sure the noise starts near the halfway point of the height
                newPoint += (map.GetUpperBound(1) / 2);
                for (int y = newPoint; y >= 0; y--)
                {
                    map[x, y] = 1;
                }
            }
            return map;
        }
    }
}