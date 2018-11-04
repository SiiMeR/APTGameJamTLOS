using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

public enum GravityPowerupState
{
    NO_GRAVITYPOWERUP,
    HAS_GRAVITYPOWERUP,
    REVERSED_GRAVITY,
}

public enum CheatState
{
    NOTHING_PRESSED,
    FIRST_PRESS,
    ACTIVATED
}


[RequireComponent(typeof(BoxController2D))]
public class Player : MonoBehaviour
{
    public float minJumpHeight = 1f;

    public float MaxJumpHeight
    {
        get { return _maxJumpHeight; }
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

    private Vector2 _lastFacingDirection;
    private float _maxJumpVelocity;
    private float _minJumpVelocity;
    private float _velocityXSmoothing;

    public BoxController2D _controller;

    private bool _hasJumped;
    public bool hasMovedThisFrame;
    private bool _outOfBounds;
    public bool hasMoleManUpgrade;
    public bool doSpawnAnimation;
    public LayerMask moleManLayers;

    public GravityPowerupState _gravityPowerupState = GravityPowerupState.NO_GRAVITYPOWERUP;

    private Tilemap _tileMap;

    private Animator _animator;
    public bool hasShroomEffect;

    public RuntimeAnimatorController _moleManAnimator;

    public Tile redPortal;

    public Tile bluePortal;

    private List<(TileBase, Vector3Int)> paintedTiles;

    // Use this for initialization
    void Start()
    {
        
        DontDestroyOnLoad(FindObjectOfType<Game>().gameObject);
        paintedTiles = new List<(TileBase, Vector3Int)>();
        
        if (doSpawnAnimation)
        {
            StartCoroutine(Game.Instance.SpawnPlayer(2.0f));
        }

        _animator = GetComponent<Animator>();

        AudioManager.Instance.Play("green", isLooping: true);

        _controller = GetComponent<BoxController2D>();

        CalculateGravity();

        _tileMap = FindObjectOfType<Tilemap>();
    }


    void CalculateGravity()
    {
        var gravity = -(2 * _maxJumpHeight) / Mathf.Pow(timeToJumpApex, 2);

        Physics2D.gravity = new Vector3(gravity, 0, 0);

        _maxJumpVelocity = Mathf.Abs(gravity * timeToJumpApex);
        _minJumpVelocity = Mathf.Sqrt(2 * Mathf.Abs(gravity) * minJumpHeight);
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.timeScale > .01f)
        {
            CheckScreenBoundaries();
            LimitMaxSpeed();
            UpdateDirection();
            UpdateMovement();
            CheckGravityPowerup();
            CheckMoleManPowerup();
            CheckShroomPortalPowerup();
            CheckShortcuts();
            
        }
    }



    private IEnumerator ActivateCheat()
    {
        var timer = 0f;

        while ((timer += Time.deltaTime) < 0.5f)
        {
            
            AudioManager.Instance.Play("jump2");
            yield return null;
        }
        
    }
    private void CheckShortcuts()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            AudioManager.Instance.StopAllMusic();
            Destroy(GameObject.Find("UI"));
            SceneManager.LoadScene("Menu");
        }

 
    }

    private void DrawShroomPortalHelpers()
    {
        foreach (var (tile, tilePos) in paintedTiles)
        {
            _tileMap.SetTile(tilePos, tile);
        }
        paintedTiles = new List<(TileBase, Vector3Int)>();
        
        var rayOrigin = transform.position;
        var rayOrigin2 = rayOrigin;

        var direction = Mathf.Sign(_lastFacingDirection.x) * Vector3.right;

        var hit = Physics2D.Raycast(rayOrigin, direction, 4f, moleManLayers);

        if (hit)
        {
            var hit2 = Physics2D.Raycast(rayOrigin, -direction, 20f, moleManLayers);

            if (hit2)
            {
                Debug.DrawRay(rayOrigin, direction, Color.yellow);
                Debug.DrawRay(rayOrigin, -direction, Color.cyan);
                
                var facingRight = Mathf.Sign(_lastFacingDirection.x) > float.Epsilon;
                
                if (facingRight)
                {
                    rayOrigin = rayOrigin.AddX(hit.distance).AddX(.5f).RoundX();
                    rayOrigin2 = rayOrigin2.AddX(-hit2.distance).AddX(-0.5f).RoundX();
                }
                else
                {
                    rayOrigin = rayOrigin.AddX(-hit.distance).AddX(-.5f).RoundX();
                    rayOrigin2 = rayOrigin2.AddX(hit2.distance).AddX(0.5f).RoundX();
                }

                rayOrigin = rayOrigin.Round();    
                rayOrigin2 = rayOrigin2.Round();

                var tile1 = _tileMap.GetTile(rayOrigin.Vector3Int());
                var tile2 = _tileMap.GetTile(rayOrigin2.Vector3Int());
                
                if (tile1 != null && tile2 != null)
                {
                    paintedTiles.Add((tile1, rayOrigin.Vector3Int()));
                    paintedTiles.Add((tile2, rayOrigin2.Vector3Int()));
                    
                    
                    _tileMap.SetTile(rayOrigin.Vector3Int(), redPortal);
                    _tileMap.SetTile(rayOrigin2.Vector3Int(), bluePortal);
                    
                }
                
            }
        }
    }

    private void CheckShroomPortalPowerup()
    {
        
        if (!hasShroomEffect) return;

        DrawShroomPortalHelpers();
        
        var rayOrigin = transform.position;

        if (Mathf.Abs(_velocity.x) > float.Epsilon)
        {
            var direction = Mathf.Sign(_velocity.x) * Vector3.right;
            
            var hit = Physics2D.Raycast(rayOrigin, direction, 1.5f, moleManLayers);

            if (hit)
            {
                var hit2 = Physics2D.Raycast(rayOrigin, -direction, 25f, moleManLayers);

                if (hit2)
                {
                    transform.position = hit2.point.Vector3() + direction;
                    StartCoroutine(ShroomCooldown(0.5f));
                }
            }
        }
    }

    private IEnumerator ShroomCooldown(float seconds)
    {
        hasShroomEffect = false;

        var timer = 0f;

        while ((timer += Time.deltaTime) < seconds)
        {
            yield return null;
        }

        hasShroomEffect = true;
    }
    private void CheckMoleManPowerup()
    {
        if (!hasMoleManUpgrade) return;

        var rayOrigin = transform.position;
        
        if (Mathf.Abs(_velocity.x) > float.Epsilon)
        {
            var facingRight = Mathf.Sign(_velocity.x) > float.Epsilon;
            
            var (hit, hit2, hit3) = TryDigInDirectionHorizontal(Vector2.right * Mathf.Sign(_velocity.x), rayOrigin);

            var mainHit = hit ? hit : hit2 ? hit2 : hit3;
            if (mainHit)
            {

                var centerPoint = rayOrigin;

                if (facingRight)
                {
                    centerPoint = centerPoint.AddX(mainHit.distance).AddX(0.5f).RoundX();
                }
                else
                {
                    centerPoint = centerPoint.AddX(-mainHit.distance).AddX(-0.5f).RoundX();
                }

                centerPoint = centerPoint.Round();
                
                var tile = _tileMap.GetTile(centerPoint.Vector3Int());
                
                if (tile && tile.name == "Glass")
                {
                    return; // cannot dig through glass
                }
                _animator.SetTrigger("Dig");
                _tileMap.SetTile(centerPoint.Vector3Int(), null);
                _tileMap.SetTile(centerPoint.Vector3Int() + Vector3Int.up, null);
            }
 

        }

        if (Input.GetButton("Jump"))
        {
            var (hit, hit2, hit3) = TryDigInDirectionVertical(Vector2.down, rayOrigin);

            var mainHit = hit ? hit : hit2 ? hit2 : hit3;
            
            if (mainHit)
            {
                var centerPoint = rayOrigin;

                centerPoint = centerPoint.AddY(-mainHit.distance).AddY(-0.5f).RoundY();
                var tile = _tileMap.GetTile(centerPoint.Vector3Int());

                if (tile && tile.name == "Glass")
                {
                    return; // cannot dig through glass
                }

                _animator.SetTrigger("Dig");
                _tileMap.SetTile(centerPoint.Vector3Int(), null);
                _tileMap.SetTile(centerPoint.Vector3Int() + Vector3Int.left, null);
                _tileMap.SetTile(centerPoint.Vector3Int() + Vector3Int.right, null);
            }
        }
    }


    private (RaycastHit2D, RaycastHit2D, RaycastHit2D) TryDigInDirectionHorizontal(Vector2 direction, Vector3 origin)
    {
        var distance = 1.1f;
        var hit = Physics2D.Raycast(origin + Vector3.up, direction, distance, moleManLayers);
        var hit2 = Physics2D.Raycast(origin, direction,distance, moleManLayers);
        var hit3 = Physics2D.Raycast(origin + Vector3.down, direction, distance, moleManLayers);

        return (hit,hit2, hit3);
    }
    
    private (RaycastHit2D, RaycastHit2D, RaycastHit2D) TryDigInDirectionVertical(Vector2 direction, Vector3 origin)
    {
        var distance = 1.1f;
        var hit = Physics2D.Raycast(origin + Vector3.left, direction, distance, moleManLayers);
        var hit2 = Physics2D.Raycast(origin, direction,distance, moleManLayers);
        var hit3 = Physics2D.Raycast(origin + Vector3.right, direction, distance, moleManLayers);

        return (hit,hit2, hit3);
    }

    private void CheckScreenBoundaries()
    {
        var dist = (transform.position - Camera.main.transform.position).z;

        var leftBorder = Camera.main.ViewportToWorldPoint(new Vector3(0, 0, dist)).x - 1;
        var rightBorder = Camera.main.ViewportToWorldPoint(new Vector3(1, 0, dist)).x + 1;
        var topBorder = Camera.main.ViewportToWorldPoint(new Vector3(0, 1, dist)).y + 1;
        var bottomBorder = Camera.main.ViewportToWorldPoint(new Vector3(0, 0, dist)).y - 1;

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
        yield return new WaitForSeconds(0.05f);
        Time.timeScale = 0f;
        _outOfBounds = true;
        yield return new WaitForSecondsRealtime(seconds);
        _outOfBounds = false;
        Time.timeScale = 1f;

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void LimitMaxSpeed()
    {
        _velocity.y = Mathf.Clamp(_velocity.y, -35f, 35f);
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

                    StartCoroutine(SmoothChangeGravity(Physics2D.gravity / 2, -Physics2D.gravity, .1f));
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

        while ((timer += Time.deltaTime) < time)
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

        var flipX = _velocity.x < float.Epsilon;
        var flipY = Physics2D.gravity.x > float.Epsilon;

        GetComponent<SpriteRenderer>().flipX = flipX;
        GetComponent<SpriteRenderer>().flipY = flipY;
    }


    private void UpdateMovement()
    {
        hasMovedThisFrame = false;

        if (_controller.collisions.above || _controller.collisions.below)
        {
            _velocity.y = 0;
            _hasJumped = false;
            _animator.SetTrigger("Stop");
        }

        var input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        if (input != Vector2.zero)
        {
            hasMovedThisFrame = true;
            _lastFacingDirection = input;
        }


        if (Input.GetButtonDown("Jump") && _controller.collisions.below && !hasMoleManUpgrade)
        {
            AudioManager.Instance.Play("jump2");
            _animator.SetTrigger("Jump");
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

        _velocity.x = Mathf.SmoothDamp(_velocity.x, targetVelocityX, ref _velocityXSmoothing,
            (_controller.collisions.below) ? accelerationTimeGrounded : accelerationTimeAirborne);

        _velocity.y += Physics2D.gravity.x * Time.deltaTime;

        _controller.Move(_velocity * Time.deltaTime);
        
        
    }

    private void LateUpdate()
    {
        if (hasMovedThisFrame)
        {
            _animator.SetTrigger("Walk");
        }
        else
        {
            _animator.SetTrigger("Stop");
        }
    }
}