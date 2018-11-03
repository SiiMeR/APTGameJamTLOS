using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// some god object to setup the game
public class Game : Singleton<Game>
{
    
    public IEnumerator SpawnPlayer(float seconds)
    {
        var entranceLocation = GameObject.FindGameObjectWithTag("Entrance");
        
        if (entranceLocation)
        {
            Debug.LogWarning("No entrance in scene");
        }
        var player = FindObjectOfType<Player>();
        player.transform.position = entranceLocation.transform.position + Vector3.up * 2;;

        player.enabled = false;

        var timer = 0f;

        var endPosition = entranceLocation.transform.position.Vector2() + Vector2.down * 2f;
        
        while ((timer += Time.deltaTime) < seconds)
        {
            
            player.transform.position = Vector2.Lerp(entranceLocation.transform.position + Vector3.up * 2, endPosition, timer / seconds);
            yield return null;
        }

        player.enabled = true;
        

    }
}
