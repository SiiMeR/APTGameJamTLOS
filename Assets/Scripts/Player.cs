using System;
using UnityEngine;

[RequireComponent(typeof(BoxController2D))]
public class Player : MonoBehaviour
{

    
    public float minJumpHeight = 1f;

    public float MaxJumpHeight
    {
        get { return _maxJumpHeight;}
        set
        {
            _maxJumpHeight = value;
            
            CalculateGravity();
        }
    }

    private float _maxJumpHeight = 4.5f;
    
    public float timeToJumpApex = .4f;

    public float moveSpeed = 10;

    public float accelerationTimeAirborne = .2f;
    public float accelerationTimeGrounded = .1f;

    public Vector3 _velocity;
	
    private float _maxJumpVelocity;
    private float _minJumpVelocity;
    private float _velocityXSmoothing;

    private BoxController2D _controller;

    private bool _hasJumped;
    public bool _hasMovedThisFrame;

    // Use this for initialization
    void Start ()
    {
        _controller = GetComponent<BoxController2D>();
        
        CalculateGravity();

    }

    void CalculateGravity()
    {
        var gravity = -(2 * _maxJumpHeight) / Mathf.Pow(timeToJumpApex, 2);
		
        Physics2D.gravity = new Vector3(gravity, 0, 0);

        _maxJumpVelocity = Mathf.Abs(gravity * timeToJumpApex);
        _minJumpVelocity = Mathf.Sqrt(2 * Mathf.Abs(gravity) * minJumpHeight);
    }
    
    // Update is called once per frame
    void Update () {
        if (Time.timeScale > .01f)
        {
            UpdateDirection();
            UpdateMovement();
        }
    }

    private void UpdateDirection()
    {
        if (Math.Abs(_velocity.x) < float.Epsilon)
        {
            return;
        }

        var flipX = _velocity.x > float.Epsilon;

        GetComponent<SpriteRenderer>().flipX = flipX;
    }

    private void UpdateMovement()
    {
        _hasMovedThisFrame = false;
		
        if (_controller.collisions.above || _controller.collisions.below)
        {
            _velocity.y = 0;
            _hasJumped = false;
        }
		
        var input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        if (input != Vector2.zero)
        {
            _hasMovedThisFrame = true;
        }
        
        Debug.DrawRay(transform.position, input, Color.red);

        if (Input.GetButtonDown("Jump") && _controller.collisions.below)
        {
            _velocity.y = _maxJumpVelocity;
            _hasJumped = true;
        }

        if (Input.GetButtonUp("Jump") && !_hasJumped)
        {
            if (_velocity.y > _minJumpVelocity)
            {
                _velocity.y = _minJumpVelocity;
            }
			
        }
		
        var targetVelocityX = input.x * moveSpeed;

        _velocity.x = Mathf.SmoothDamp(_velocity.x, targetVelocityX, ref _velocityXSmoothing, (_controller.collisions.below) ? accelerationTimeGrounded : accelerationTimeAirborne);
		
        _velocity.y += Physics2D.gravity.x * Time.deltaTime;
        
        _controller.Move(_velocity * Time.deltaTime);
		
    }
}