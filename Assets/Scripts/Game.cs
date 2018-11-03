using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// some god object to setup the game
public class Game : Singleton<Game>
{
    
    public IEnumerator SpawnPlayer(float seconds)
    {
        var entranceLocation = GameObject.FindGameObjectWithTag("Entrance").transform.position;

        var player = FindObjectOfType<Player>();
        player.transform.position = entranceLocation;

        player.enabled = false;

        var timer = 0f;

        var endPosition = entranceLocation.Vector2() + Vector2.down * 1.5f;
        
        while ((timer += Time.deltaTime) < seconds)
        {
            
            player.transform.position = Vector2.Lerp(entranceLocation, endPosition, timer / seconds);
            yield return null;
        }

        player.enabled = true;
        

    }
}
