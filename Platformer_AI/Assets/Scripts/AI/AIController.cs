using Platformer.Mechanics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;


public class AIController : MonoBehaviour
{
    public GameObject player;
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
        Vector3 bottomLeft = getBottomLeft(10.0f);
        Vector3 topRight = getTopRight(10.0f);
        int sizeY = (int)Mathf.Abs(topRight.y - bottomLeft.y);
        int sizeX = (int)Mathf.Abs(topRight.x - bottomLeft.x);
        
        BoundsInt area = new BoundsInt((int)bottomLeft.x, (int)bottomLeft.y, 0, sizeX,sizeY,1);
        TileBase[] tileArray = tilemap.GetTilesBlock(area);

        //area = cameraBounds.size;


        foreach (var position in tilemap.cellBounds.allPositionsWithin)
        {
            if (tilemap.HasTile(position))
            {
                if (tilemap.GetTile(position).name == "TileGround")
                {
                    print(position);
                }
            }

            // Tile is not empty; do stuff
        }

        for (int index = 0; index < tileArray.Length; index++)
        {
            //print(tileArray[index]);
            if (tileArray[index] != null)
            {
                current_tiles[index] = tileArray[index].name == "TileGround" || tileArray[index].name == "TileWall" ? 1 : 0;
                if (current_tiles[index] == 1)
                {
                    //print(((Tile)tileArray[index]).GetTileData());
                }
            }
        }
        print(current_tiles);

        /*for (int index = 0; index < tileArray.Length; index++)
        {
            if (current_tiles[index] == 1)
            {
                print("x: "+  index%31);
                print("y: " + index / 31);

            }
        }*/
        //BoundsInt bounds = tilemap.cellBounds;
        //TileBase[] allTiles = tilemap.GetTilesBlock(bounds);
        //Debug.Log(bounds.zMin);
        /*for (int x = 0; x < bounds.size.x; x++)
        {
            for (int y = 0; y < bounds.size.y; y++)
            {
                TileBase tile = allTiles[x + y * bounds.size.x];
                if (tile != null)
                {
                    Debug.Log("x:" + x + " y:" + y + " tile:" + tile.name);
                }
                else
                {
                    Debug.Log("x:" + x + " y:" + y + " tile: (null)");
                }
            }
        }*/




    }

    // Update is called once per frame
    void FixedUpdate()
    {
        player_controller.move = Vector2.right;

        Vector3 bottomLeft = getBottomLeft(10.0f);
        Vector3 topRight = getTopRight(10.0f);
        int sizeY = (int)Mathf.Abs(topRight.y - bottomLeft.y);
        int sizeX = (int)Mathf.Abs(topRight.x - bottomLeft.x);

        BoundsInt area = new BoundsInt((int)bottomLeft.x, (int)bottomLeft.y, 0, sizeX, sizeY, 1);
        TileBase[] tileArray = tilemap.GetTilesBlock(area);

        //area = cameraBounds.size;

        for (int index = 0; index < tileArray.Length; index++)
        {
            //print(tileArray[index]);
            if (tileArray[index]!= null) { 
                current_tiles[index] = tileArray[index].name == "TileGround" || tileArray[index].name == "TileWall" ? 1 : 0;
            }
        }

        /*for (int index = 0; index < tileArray.Length; index++)
        {
            if (current_tiles[index] == 1)
            {
                print("O");
            }
            else
            {
                print(" ");
            }
            if (index % 31 == 0)
            {
                print("\n");
            }
        }*/
    }

    public Vector3 getTopLeft(float distance)
    {
        float y = distance * Mathf.Tan(camera.fieldOfView * Mathf.Deg2Rad / 2);
        float x = y * camera.aspect;
        return this.transform.TransformPoint(new Vector3(-x, y, distance));
    }
    public Vector3 getTopRight(float distance)
    {
        float y = distance * Mathf.Tan(this.camera.fieldOfView * Mathf.Deg2Rad / 2);
        float x = y * this.camera.aspect;
        return new Vector3(x, y, distance);
    }
    public Vector3 getBottomLeft(float distance)
    {
        float y = distance * Mathf.Tan(camera.fieldOfView * Mathf.Deg2Rad / 2);
        float x = y * camera.aspect;
        return new Vector3(-x, -y, distance);
    }

    public Vector3 getBottomRight(float distance)
    {
        float y = distance * Mathf.Tan(camera.fieldOfView * Mathf.Deg2Rad / 2);
        float x = y * camera.aspect;
        return new Vector3(x, -y, distance);
    }
}
