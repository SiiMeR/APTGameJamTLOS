using System;
using System.Collections;
using UnityEngine;

public enum GravityPowerupState
{
    NO_GRAVITYPOWERUP,
    HAS_GRAVITYPOWERUP,
    REVERSED_GRAVITY,
}

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
    private bool _outOfBounds;

    public GravityPowerupState _gravityPowerupState = GravityPowerupState.NO_GRAVITYPOWERUP;
    
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
            CheckScreenBoundaries();
            LimitMaxSpeed();
            UpdateDirection();
            UpdateMovement();
            CheckGravityPowerup();
        }
    }

    private void CheckScreenBoundaries()
    {
        var dist = (transform.position - Camera.main.transform.position).z;
        
        var leftBorder = Camera.main.ViewportToWorldPoint(new Vector3(0, 0, dist)).x;
        var rightBorder = Camera.main.ViewportToWorldPoint(new Vector3(1, 0, dist)).x;
        var topBorder = Camera.main.ViewportToWorldPoint(new Vector3(0, 1, dist)).y;
        var bottomBorder = Camera.main.ViewportToWorldPoint(new Vector3(0, 0, dist)).y;

        if (!_outOfBounds &&
            (transform.position.y > topBorder || 
            transform.position.y < bottomBorder ||
            transform.position.x > rightBorder ||
            transform.position.x < leftBorder))
        {
            StartCoroutine(WaitAfterDeath(0.8f));
        }
    }
    
    private IEnumerator WaitAfterDeath(float seconds)
    {
        _outOfBounds = true;
        yield return new WaitForSeconds(seconds);
        transform.position = new Vector3(-13, -2, transform.position.z);
        _outOfBounds = false;

    }
    private void LimitMaxSpeed()
    {           
        _velocity.y = Mathf.Clamp(_velocity.y,-35f, 35f);
    }

    private void CheckGravityPowerup()
    {
        if (Input.GetButtonDown("Jump"))
        {
            switch (_gravityPowerupState)
            {
                case GravityPowerupState.NO_GRAVITYPOWERUP:
                    return;
                case GravityPowerupState.HAS_GRAVITYPOWERUP:
                    StartCoroutine(SmoothChangeGravity(Physics2D.gravity, -Physics2D.gravity, .1f));
                    _gravityPowerupState = GravityPowerupState.REVERSED_GRAVITY;
                    break;
                case GravityPowerupState.REVERSED_GRAVITY:
                    
                    StartCoroutine(SmoothChangeGravity(Physics2D.gravity/2, -Physics2D.gravity, .1f));
                    _gravityPowerupState = GravityPowerupState.NO_GRAVITYPOWERUP;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    private IEnumerator SmoothChangeGravity(Vector2 start, Vector2 end, float time)
    {
        var timer = 0f;
        
        while((timer += Time.deltaTime) < time)
        {
            Physics2D.gravity = Vector2.Lerp(start, end, timer);
            yield return null;
        }

        Physics2D.gravity = end;

    }
    private void UpdateDirection()
    {
        if (Math.Abs(_velocity.x) < float.Epsilon)
        {
            return;
        }

        var flipX = _velocity.x > float.Epsilon;
        var flipY = Physics2D.gravity.x > float.Epsilon;

        GetComponent<SpriteRenderer>().flipX = flipX;
        GetComponent<SpriteRenderer>().flipY = flipY;
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