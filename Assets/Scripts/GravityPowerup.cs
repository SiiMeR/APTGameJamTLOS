using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GravityPowerup : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            var player = other.GetComponent<Player>();

            player._gravityPowerupState = GravityPowerupState.HAS_GRAVITYPOWERUP;
            
            Destroy(gameObject);
        }
    }
}