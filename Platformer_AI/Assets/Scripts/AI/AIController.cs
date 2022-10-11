using GNN_AI;
using Platformer.Mechanics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using static GNN_AI.GNN;
using Platformer.Gameplay;
using TMPro;

using static Platformer.Core.Simulation;
using Platformer.Model;
using Platformer.Core;
using UnityEngine.UI;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System;
using MAPGEN;

namespace aicontroller
{
    public class AIController : MonoBehaviour
    {
        public bool useFile;
        public int timeScale;
        const int checkTime = 40;
        public GameObject player;
        public PlayerController playerController;
        public Vector3 bottomLeftCheck;
        public GameObject in_Nodes;
        public GameObject hidden_Nodes;
        public GameObject gen_Node;
        private TextMeshProUGUI inNodesText;
        private TextMeshProUGUI hiddenNodesText;
        private TextMeshProUGUI genText;
        private bool init = false;

        private GNN net;
        private PlayerController player_controller;
        public Tilemap tilemap;
        public BoundsInt area;
        public Camera camera;
        private float prev_x2 = 0;
        private double[,] current_tiles = new double[1, 558];
        private float prev_x = 0;
        public TileBase tile;
        public TileBase ground;
        //31 x 17
        private float targetTime = checkTime;
        // Start is called before the first frame update
        void Start()
        {
            Time.timeScale = timeScale;
            
            inNodesText = in_Nodes.GetComponent<TextMeshProUGUI>();
            hiddenNodesText = hidden_Nodes.GetComponent<TextMeshProUGUI>();
            genText = gen_Node.GetComponent<TextMeshProUGUI>();

            player_controller = player.GetComponent<PlayerController>();

            bottomLeftCheck = camera.ViewportToWorldPoint(new Vector3(0, 0, camera.nearClipPlane));

            
            int[,] map = TileMapGen.GenerateArray(150, 20, false);
            TileMapGen.RenderMap(map, tilemap,tile,ground);
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


            net = new GNN(this);

            net.createFirstGeneration();







        }


        // Update is called once per frame
        void FixedUpdate()
        {
            //Time.timeScale = timeScale;

            PlatformerModel model = Simulation.GetModel<PlatformerModel>();
            //var playerController = model.player;
            //playerController.dead = true;


            //print(targetTime);
            //float targetTime = 1.0f;

            //if(targetTime <= 0.0f) {
            if (player.transform.position.y < -3)
            {
                // print("Player dead");
                playerController.dead = true;
                
                



            }
            if (player.transform.position.x > 40)
            {
                print("Player won");
                playerController.won = true;
                
            }

            if (targetTime <= 0.0f)
            {
                if (player.transform.position.x <= playerController.prevX)//Input.GetButton("Jump")
                {

                    //print("current_pos: " + player.transform.position.x + " prev_pos: " + playerController.prevX);
                    playerController.dead = true;

                }
                else
                {

                    //print("updated prevX");
                    playerController.prevX = player.transform.position.x;
                }

                targetTime = checkTime;
            }


            targetTime--;
            // print(targetTime);





            //player_controller.move = Vector2.right;
            Vector3 bottomLeft = camera.ViewportToWorldPoint(new Vector3(0, 0, camera.nearClipPlane));
            Vector3 topRight = camera.ViewportToWorldPoint(new Vector3(1, 1, camera.nearClipPlane));

            GameObject platform1 = FindClosestPlatform();
            GameObject platform2 = FindSecondClosestPlatformToTheRight(platform1);

            //print("Platform 1: " + platform1.transform.position);
            //print("Platform 2: " + platform2.transform.position);

            
            
          
            if (playerController.dead || playerController.won)
            {
                playerController.move = Vector2.zero;
                playerController.jumpState = PlayerController.JumpState.Grounded;
                playerController.Teleport(model.spawnPoint.transform.position);
                playerController.prevX = 0;
                if (!useFile)
                {
                    net.breedNetworks(genText, playerController.won);

                    if (playerController.won)
                    {
                        Time.timeScale = 1;
                        
                        //useFile = true;
                        int[,] map = TileMapGen.GenerateArray(150, 20, false);
                        TileMapGen.RenderMap(map, tilemap, tile, ground);
                    }
                }
                playerController.won = false;
                playerController.dead = false;
                return;
            }


            double[,] output = net.runForward(inNodesText, hiddenNodesText, useFile);



            //print(output[0,0]);
            //print(output[0,1]);
            //print(output[0,2]);

            if (output[0, 0] > 0.5 && player_controller.jumpState == PlayerController.JumpState.Grounded)
            {
                player_controller.jumpState = PlayerController.JumpState.PrepareToJump;
            }

            //player_controller.move = Vector2.zero;

            if (output[0, 1] > 0.5)
            {
                player_controller.move = Vector2.right;
            }

            if (output[0, 2] > 0.5)
            {
                player_controller.move = Vector2.left;
            }


            
            //print( "in ai controller: "+ player_controller.move);
            LayerMask mask = LayerMask.GetMask("Default");
            //RaycastHit2D hitObject ;
            var hitObject = Physics2D.Raycast(player.transform.position, player.transform.right, 0.5f, mask);

            if (player_controller.move == Vector2.right && (hitObject.collider != null))
            {
                player_controller.move = Vector2.zero;

                //print("stopping right movement" + hitObject.transform.gameObject.name);
            }


            prev_x2 = player.transform.position.x;


        }

        private object Schedule<T>()
        {
            throw new System.NotImplementedException();
        }

        public GameObject FindSecondClosestPlatformToTheRight(GameObject closest)
        {
            GameObject[] gos;
            gos = GameObject.FindGameObjectsWithTag("Platform");
            GameObject secondClosest = null;
            float distance = Mathf.Infinity;
            Vector3 position = player.transform.position;
            foreach (GameObject go in gos)
            {
                Vector3 diff = go.transform.position - position;
                float curDistance = diff.sqrMagnitude;
                if (curDistance < distance && go.transform.position.x > closest.transform.position.x)
                {
                    secondClosest = go;
                    distance = curDistance;
                }
            }
            return secondClosest;
        }

        public GameObject FindClosestPlatform()
        {
            GameObject[] gos;
            gos = GameObject.FindGameObjectsWithTag("Platform");
            GameObject closest = null;
            float distance = Mathf.Infinity;
            Vector3 position = player.transform.position;
            foreach (GameObject go in gos)
            {
                Vector3 diff = go.transform.position - position;
                float curDistance = diff.sqrMagnitude;
                if (curDistance < distance)
                {
                    closest = go;
                    distance = curDistance;
                }
            }
            return closest;
        }

        public Vector3[] getRelativePos()
        {
            GameObject p1 = FindClosestPlatform();
            GameObject p2 = FindSecondClosestPlatformToTheRight(p1);

            Vector3 relp1 = p1.transform.position - player.transform.position;
            Vector3 relp2 = p2.transform.position - player.transform.position;
            Vector3[] vecArray = { relp1, relp2 };
            return vecArray;
        }

        public double[,] GetTiles(Vector3 bottomLeft, Vector3 topRight, Tilemap tilemap)
        {
            int sizeY = (int)Mathf.Abs(topRight.y - bottomLeft.y);
            int sizeX = (int)Mathf.Abs(topRight.x - bottomLeft.x);
            BoundsInt area = new BoundsInt((int)bottomLeft.x, (int)bottomLeft.y, 0, sizeX, sizeY, 1);
            TileBase[] tileArray = tilemap.GetTilesBlock(area);
            if (!init || (bottomLeftCheck - bottomLeft).magnitude > 0.5)
            {
                init = true;
                //print("inside");
                bottomLeftCheck = bottomLeft;
                //print(topRight);

                current_tiles = new double[1, 558];


                foreach (var position in tilemap.cellBounds.allPositionsWithin)
                {


                    if (tilemap.HasTile(position))
                    {
                        Vector3 worldCoord = tilemap.CellToWorld(position);
                        if ((worldCoord.x > bottomLeft.x && worldCoord.y > bottomLeft.y) && (worldCoord.x < topRight.x && worldCoord.y < topRight.y))
                        {
                            string tileName = tilemap.GetTile(position).name;
                            int iX = Math.Abs((tilemap.WorldToCell(bottomLeft).x - position.x));
                            int iY = Math.Abs((tilemap.WorldToCell(bottomLeft).y - position.y ));
                        if (tileName == "TileGround" || tileName == "TileGroundTop" || tileName == "TileFloatingLeftEdge" || tileName == "TileFloatingMiddle" || tileName == "TileFloatingRightEdge")
                        {

                            //print(worldCoord + tileName);
                            //current_tiles[iX * iY] = 1;
                            try
                            {
                                current_tiles[0, iX + (18 -iY) * 31 ] = 1;
                            }
                            catch (Exception e)
                            {
                                //print("error with" + iX + ", " + iY);
                            }
                        }


                        else
                        {
                            try
                            {
                                current_tiles[0, iX + (18 - iY) * 31] = 0;
                            }
                            catch (Exception e)
                            {
                                print("error with" + iX + ", " + iY);
                            }
                        }

                        }
                    }


                }

                /*for (int i = 0; i < current_tiles.Length; ++i)
                {
                    print(current_tiles[i]);
                }*/
            }
            return current_tiles;
        }


    }
}