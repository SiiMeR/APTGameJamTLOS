﻿using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
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
    
    
    public const int SCORE_PERLEVEL = 2000;
    public const int SCORE_LOSS_PER_SECOND = 100;
    public const int SCORE_LOSS_PER_DEATH = 100;
    
    public static int Score = 5000;
    public static int DeathsInLevel = 0;
    
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

        var sa = GameObject.FindGameObjectWithTag("Say").GetComponent<TextMeshProUGUI>().color;
        sa.a = 0f;
        GameObject.FindGameObjectWithTag("Say").GetComponent<TextMeshProUGUI>().color = sa;
        
        
        
        switch (SceneManager.GetActiveScene().name)
        {
                case "Level1":
                    switch (DeathsInLevel)    
                    {
                            case 1:
                                StartCoroutine(SaySomething("Oops, I forgot to mention that you have to press SPACE to change gravity after getting the superpower"));
                                break;
                            
                            case 2:
                                StartCoroutine(SaySomething("I should add that you can press SPACE again to return the gravity to normal"));
                                break;
                            
                            case 3:
                                StartCoroutine(SaySomething("I thought you were smarter"));
                                break;
                    }
                    break;
                                
                case "Level2":
                    switch (DeathsInLevel)    
                    {
                        case 1:
                            StartCoroutine(SaySomething("Press SPACE to go deeper"));
                            break;
                            
                        case 2:
                            StartCoroutine(SaySomething("Be gentle"));
                            break;
                            
                        case 3:
                            StartCoroutine(SaySomething("-_- I told you be gentle"));
                            break;
                    }
                    break;
                                
                case "Level3":
                    switch (DeathsInLevel)    
                    {
                        case 1:
                            StartCoroutine(SaySomething("Dude, look at those beautiful walls"));
                            break;
                            
                        case 2:
                            StartCoroutine(SaySomething("Red takes you to blue"));
                            break;
                            
                        case 3:
                            StartCoroutine(SaySomething("Take it easy"));
                            break;
                    }
                    break;
                                
                case "Level4":
                    break;
                                
                case "Level5":
                    break;
                                
                case "Level6":
                    switch (DeathsInLevel)    
                    {
                        case 20:
                            StartCoroutine(SaySomething("Git gud"));
                            break;
    
                    }
                    break;
                
        }

//        DontDestroyOnLoad(FindObjectOfType<Game>());
        
        /*if (FindObjectsOfType<Game>().Length > 1)
        {
            Destroy(FindObjectOfType<Game>());
        }*/
        
        UpdateScoreUI();
        
        paintedTiles = new List<(TileBase, Vector3Int)>();
        
        if (doSpawnAnimation)
        {
            StartCoroutine(Game.Instance.GetComponent<Game>().SpawnPlayer(2.0f));
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
            CheckDeath();

        }
    }

    private void CheckDeath()
    {
        if (Score < 0)
        {
            SceneManager.LoadScene("Ending");
        }
    }


    private void FixedUpdate()
    {
        Score -= (int) (SCORE_LOSS_PER_SECOND * Time.deltaTime);
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

        var hit = Physics2D.Raycast(rayOrigin, direction, 5f, moleManLayers);

        if (hit)
        {
            var hit2 = Physics2D.Raycast(rayOrigin, -direction, 50f, moleManLayers);

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
                var hit2 = Physics2D.Raycast(rayOrigin, -direction, 50f, moleManLayers);

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

        var isReverse = Physics2D.gravity.x > float.Epsilon;
        
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

                if (isReverse)
                {
                    _tileMap.SetTile(centerPoint.Vector3Int() + Vector3Int.down, null);
                }
                else
                {
                    _tileMap.SetTile(centerPoint.Vector3Int() + Vector3Int.up, null);
                }

            }
 

        }

        if (Input.GetButton("Jump") )
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

        
        if (isReverse) // upside down auto digging
        {
            var (hit, hit2, hit3) = TryDigInDirectionVertical(Vector2.up, rayOrigin);

            var mainHit = hit ? hit : hit2 ? hit2 : hit3;

            if (mainHit)
            {
                var centerPoint = rayOrigin;

                centerPoint = centerPoint.AddY(mainHit.distance).AddY(0.5f).RoundY();
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

        var leftBorder = Camera.main.ViewportToWorldPoint(new Vector3(0, 0, dist)).x - 3;
        var rightBorder = Camera.main.ViewportToWorldPoint(new Vector3(1, 0, dist)).x + 3;
        var topBorder = Camera.main.ViewportToWorldPoint(new Vector3(0, 1, dist)).y + 3;
        var bottomBorder = Camera.main.ViewportToWorldPoint(new Vector3(0, 0, dist)).y - 3;

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

        
        DeathsInLevel++;
        Score -= SCORE_LOSS_PER_DEATH;
        StopAllCoroutines();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void UpdateScoreUI()
    {
        if (!GameObject.FindGameObjectWithTag("Score")) return;
        
        GameObject.FindGameObjectWithTag("Score").GetComponent<TextMeshProUGUI>().text = $"Score: {Score}";
    }
    
    private void LimitMaxSpeed()
    {
        _velocity.y = Mathf.Clamp(_velocity.y, -30f, 30f);
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

    public IEnumerator SaySomething(string toSay)
    {
        var say = GameObject.FindGameObjectWithTag("Say").GetComponent<TextMeshProUGUI>();
        
        yield return new WaitForSeconds(1.0f);
        var timer = 0f;

        say.text = $"???: {toSay}";
        
        while ((timer += Time.deltaTime) < 1.0f)
        {
            var col = say.color;
            col.a = Mathf.Lerp(0f, 1f, timer / 1.0f);
            say.GetComponent<TextMeshProUGUI>().color = col;
            
            yield return null;
        }
        
        var endColor = say.color;
        endColor.a = 1.0f;
        say.GetComponent<TextMeshProUGUI>().color = endColor;

        
        yield return new WaitForSeconds(4.0f);
        
        timer = 0f;

        while ((timer += Time.deltaTime) < 1.0f)
        {
            var col = say.color;
            col.a = Mathf.Lerp(1f, 0f, timer / 1.0f);
            say.GetComponent<TextMeshProUGUI>().color = col;
            
            yield return null;
        }
        
        endColor = say.color;
        endColor.a = 0.0f;
        say.GetComponent<TextMeshProUGUI>().color = endColor;
        
        yield return null;
        
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
        UpdateScoreUI();
        
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