using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShroomPortalPowerup : MonoBehaviour
{

    public float animationAmplitude = 1f;
    public float animationPeriod = 1f;


    private Vector3 startPos;
	
    // Use this for initialization
    void Start () {
        startPos = transform.position;
    }


    void Update()
    {
        var theta = Time.timeSinceLevelLoad / animationPeriod;
        var distance = animationAmplitude * Mathf.Sin(theta);
        transform.position = startPos + Vector3.up * distance;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Camera.main.GetComponent<ShroomEffect>().ToggleShroomEffect();
        FindObjectOfType<Player>().hasShroomEffect = true;
        Destroy(gameObject);
    }
}
