using Platformer.Mechanics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;


public class AIController : MonoBehaviour
{
    public GameObject player;
    public Vector3 bottomLeftCheck;  
    
    private PlayerController player_controller;
    public Tilemap tilemap;    
    public BoundsInt area;
    public Camera camera;
    private int[] current_tiles = new int[527];
    //31 x 17
    // Start is called before the first frame update
    void Start()
    {
        
        player_controller = player.GetComponent <PlayerController>();
        
        bottomLeftCheck = camera.ViewportToWorldPoint(new Vector3(0, 0, camera.nearClipPlane));
        


        //area = cameraBounds.size;


        /*foreach (var position in tilemap.cellBounds.allPositionsWithin)
        {
            
            
            if (tilemap.HasTile(position))
            {
                
                string tileName = tilemap.GetTile(position).name;
                if (tileName == "TileGround" || tileName == "TileGroundTop" || tileName == "TileFloatingLeftEdge" || tileName == "TileFloatingMiddle" || tileName == "TileFloatingRightEdge")
                {
                    Vector3 worldCoord = tilemap.CellToWorld(position);
                    if ((worldCoord.x > bottomLeft.x && worldCoord.y > bottomLeft.y) && (worldCoord.x < topRight.x && worldCoord.y < topRight.y))
                    {
                        //print(worldCoord + tileName);
                        int iX = Mathf.RoundToInt((worldCoord.x - bottomLeft.x));
                        int iY =Mathf.RoundToInt((worldCoord.y - bottomLeft.y)) ;
                        current_tiles[iX*iY] = 1;
                        
                    }
                    //print(position);
                    

                }
            }
            
            // Tile is not empty; do stuff
        }

        for (int i = 0; i < current_tiles.Length; ++i)
        {
            print(current_tiles[i]);
        }*/




    }
    

    // Update is called once per frame
    void FixedUpdate()
    {

        bool run = false;


        
        player_controller = player.GetComponent<PlayerController>();
        player_controller.move = Vector2.right;
        Vector3 bottomLeft = camera.ViewportToWorldPoint(new Vector3(0, 0, camera.nearClipPlane));
        Vector3 topRight = camera.ViewportToWorldPoint(new Vector3(1, 1, camera.nearClipPlane));
        int sizeY = (int)Mathf.Abs(topRight.y - bottomLeft.y);
        int sizeX = (int)Mathf.Abs(topRight.x - bottomLeft.x);
        print(bottomLeft);
        print(bottomLeftCheck);
        
        BoundsInt area = new BoundsInt((int)bottomLeft.x, (int)bottomLeft.y, 0, sizeX, sizeY, 1);
        TileBase[] tileArray = tilemap.GetTilesBlock(area);

        //area = cameraBounds.size;

        if ((bottomLeftCheck - bottomLeft).magnitude > 0.5) {
            print("inside");
            bottomLeftCheck = bottomLeft;
            

            foreach (var position in tilemap.cellBounds.allPositionsWithin)
            {


                if (tilemap.HasTile(position))
                {

                    string tileName = tilemap.GetTile(position).name;
                    if (tileName == "TileGround" || tileName == "TileGroundTop" || tileName == "TileFloatingLeftEdge" || tileName == "TileFloatingMiddle" || tileName == "TileFloatingRightEdge")
                    {
                        Vector3 worldCoord = tilemap.CellToWorld(position);
                        if ((worldCoord.x > bottomLeft.x && worldCoord.y > bottomLeft.y) && (worldCoord.x < topRight.x && worldCoord.y < topRight.y))
                        {
                            //print(worldCoord + tileName);
                            int iX = Mathf.RoundToInt((worldCoord.x - bottomLeft.x));
                            int iY = Mathf.RoundToInt((worldCoord.y - bottomLeft.y));
                            current_tiles[iX * iY] = 1;

                        }

                    }
                }

                
            }

            /*for (int i = 0; i < current_tiles.Length; ++i)
            {
                print(current_tiles[i]);
            }*/
        }
        

        
        

        

        
        
    }




  

   
}
