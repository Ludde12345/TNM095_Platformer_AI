using Platformer.Mechanics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIController : MonoBehaviour
{
    public GameObject player;
    private PlayerController player_controller;
    private int[,] current_tiles = new int[31, 17];
    //31 x 17
    // Start is called before the first frame update
    void Start()
    {
        player_controller = player.GetComponent <PlayerController> ();
        
    }

    // Update is called once per frame
    void Update()
    {
        player_controller.move = Vector2.right;     
    }
}
