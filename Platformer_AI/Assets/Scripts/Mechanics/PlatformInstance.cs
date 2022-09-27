using Platformer.Mechanics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformInstance : MonoBehaviour
{
    public bool hit = false;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        //only exectue OnPlayerEnter if the player collides with this token.
        var player = other.gameObject.GetComponent<PlayerController>();
        if (player != null && player.transform.position.y >= transform.position.y) OnPlayerEnter(player);
    }

    void OnPlayerEnter(PlayerController player)
    {
        print("Hit platform!");
        if (hit) return;
        
        hit = true;
        player.hitPlatforms.Add(this);
  
    }

}
