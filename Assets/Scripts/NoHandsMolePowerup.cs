using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoHandsMolePowerup : MonoBehaviour
{
    public Sprite moleSprite;
    
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
        if (other.gameObject.CompareTag("Player"))
        {
            var player = other.gameObject.GetComponent<Player>();
            player.GetComponent<Animator>().runtimeAnimatorController = player._moleManAnimator;
            player.hasMoleManUpgrade = true;
            Destroy(player.GetComponent<BoxCollider2D>());

            var coll = player.gameObject.AddComponent<BoxCollider2D>();
            coll.isTrigger = true;

            
            var collSize = new Vector2();
            var sprite = moleSprite;
            collSize.x = (sprite.bounds.size.x - (sprite.border.x + sprite.border.z) /
                          sprite.pixelsPerUnit);
            collSize.y = (sprite.bounds.size.y - (sprite.border.w + sprite.border.y) /
                          sprite.pixelsPerUnit);

            coll.size = collSize;
            
            player._controller._coll = coll;
            
            player._controller.CalculateRaySpacing();
            
            Destroy(gameObject);
        }
    }
}
