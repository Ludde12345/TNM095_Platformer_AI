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

namespace aicontroller
{ 
    public class AIController : MonoBehaviour
    {
        public GameObject player;
        public PlayerController playerController;
        public Vector3 bottomLeftCheck;
        public GameObject in_Nodes;
        public GameObject hidden_Nodes;

        private TextMeshProUGUI inNodesText;
        private TextMeshProUGUI hiddenNodesText;


        private GNN net;
        private PlayerController player_controller;
        //public Tilemap tilemap;    
        public BoundsInt area;
        public Camera camera;
        private int[] current_tiles = new int[527];
        private float prev_x = 0;
        private float targetTime = 1.0f;
        //31 x 17
        // Start is called before the first frame update
        void Start()
        {

            inNodesText = in_Nodes.GetComponent<TextMeshProUGUI>();
            hiddenNodesText = hidden_Nodes.GetComponent<TextMeshProUGUI>();


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


            net = new GNN(this);
             
            net.createFirstGeneration();
           
             
             
            



        }


        // Update is called once per frame
        void FixedUpdate()
        {
            PlatformerModel model = Simulation.GetModel<PlatformerModel>();
            //var playerController = model.player;
            //playerController.dead = true;
            
            
            //print(targetTime);
                
                if(player.transform.position.x == prev_x)
                {

                    targetTime -= Time.deltaTime;
                    if(targetTime <= 0.0f) {
                        playerController.dead = true;
                        if (playerController.health.IsAlive)
                        {
                            print("Player dead");
                            playerController.health.Die();
                            model.virtualCamera.m_Follow = null;
                            model.virtualCamera.m_LookAt = null;
                            // player.collider.enabled = false;
                            playerController.controlEnabled = false;

                            if (playerController.audioSource && playerController.ouchAudio)
                                playerController.audioSource.PlayOneShot(playerController.ouchAudio);
                            playerController.animator.SetTrigger("hurt");
                            playerController.animator.SetBool("dead", true);
                            Simulation.Schedule<PlayerSpawn>(2);
                        }
                    
                        //Schedule<PlayerDeath>();
                        targetTime = 1.0f;
                }
                    
                    
                    
                }
                else 
                    targetTime = 1.0f;
                
                
                prev_x = player.transform.position.x;
            


            //player_controller.move = Vector2.right;
            Vector3 bottomLeft = camera.ViewportToWorldPoint(new Vector3(0, 0, camera.nearClipPlane));
            Vector3 topRight = camera.ViewportToWorldPoint(new Vector3(1, 1, camera.nearClipPlane));
        
            GameObject platform1 = FindClosestPlatform();
            GameObject platform2 = FindSecondClosestPlatformToTheRight(platform1);

            print("Platform 1: " + platform1.transform.position);
            print("Platform 2: " + platform2.transform.position);

            double[,] output = net.runForward(inNodesText, hiddenNodesText);
            if (playerController.won || playerController.dead)
            {
                net.breedNetworks();
                playerController.won = false;
                playerController.dead = false;
            }

            //print(output[0,0]);
            //print(output[0,1]);
            //print(output[0,2]);

            if (output[0,0] > 0.5 && player_controller.jumpState == PlayerController.JumpState.Grounded)
            {
                player_controller.jumpState = PlayerController.JumpState.PrepareToJump;
            }

            player_controller.move = Vector2.zero;

            if (output[0,1] > 0.5) { 
                player_controller.move = Vector2.right;
            }
            
            if (output[0, 2] > 0.5) {
                player_controller.move = Vector2.left;
            }


            print( "in ai controller: "+ player_controller.move);



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
        
        void GetTiles(Vector3 bottomLeft, Vector3 topRight, Tilemap tilemap)
        {
            int sizeY = (int)Mathf.Abs(topRight.y - bottomLeft.y);
            int sizeX = (int)Mathf.Abs(topRight.x - bottomLeft.x);
            BoundsInt area = new BoundsInt((int)bottomLeft.x, (int)bottomLeft.y, 0, sizeX, sizeY, 1);
            TileBase[] tileArray = tilemap.GetTilesBlock(area);
            if ((bottomLeftCheck - bottomLeft).magnitude > 0.5)
            {
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
}