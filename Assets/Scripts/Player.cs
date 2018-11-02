using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Experimental.UIElements;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

[RequireComponent(typeof(BoxController2D))]
public class Player : MonoBehaviour
{
    
    public Vector3 Velocity;
    public bool IgnoreGround;

    [SerializeField] private float _accelerationTimeAirborne = .2f;
    [SerializeField] private float _accelerationTimeGrounded = .1f;
    
    [SerializeField] private GameObject _deathScreen;
    [SerializeField] private int _currency = 100;
    [SerializeField] private float _maxJumpHeight = 4f;
    [SerializeField] private float _moveSpeed = 10;
    [SerializeField] private float _secondsInvincibility = 1.5f;
    [SerializeField] private float _minJumpHeight = 1f;
    [SerializeField] private float _timeToJumpApex = .4f;

    private bool _canBoost = true;
    private BoxController2D _controller;
    private float _currentBoostTime;
    private Animator _animator;
    private bool _isBoosting;
    private Vector2 _lastFacingDirection;
    private Vector3 _lastInput;
    private float _maxJumpVelocity;
    private float _minJumpVelocity;
    private double _shotCoolDownTimer;
    private float _velocityXSmoothing;

    public int Currency
    {
        get => _currency;
        set => _currency = value;
    }



    private IEnumerator Death()
    {
        _deathScreen.SetActive(true);

        AudioManager.Instance.StopAllMusic();
        AudioManager.Instance.SetSoundVolume(0);

        Time.timeScale = 0.0f;
        
        yield return new WaitUntil(() => Input.GetButtonDown("Submit"));

        Time.timeScale = 1.0f;

        _deathScreen.SetActive(false);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    

    // Use this for initialization
    private void Start()
    {
        _lastFacingDirection = Vector2.right;

   //     _deathScreen.SetActive(false);
        _animator = GetComponent<Animator>();

        _controller = GetComponent<BoxController2D>();

        var gravity = -(2 * _maxJumpHeight) / Mathf.Pow(_timeToJumpApex, 2);

        Physics2D.gravity = new Vector3(gravity, 0, 0);

        _maxJumpVelocity = Mathf.Abs(gravity * _timeToJumpApex);
        _minJumpVelocity = Mathf.Sqrt(2 * Mathf.Abs(gravity) * _minJumpHeight);
    }

    // Update is called once per frame
    private void Update()
    {
        if (Time.timeScale > 0.01f)
        {
            UpdateMovement();

            if (Math.Abs(Velocity.x) > .01f)
            {
                _lastFacingDirection = ConvertToInteger(new Vector2(Velocity.x, Velocity.y));
            }
        }
    }



    public IEnumerator PlayerDamaged()
    {

        _animator.SetBool("Damaged", true);

        var randomXJitter = Random.Range(-1.5f, 1.5f);
        var randomYJitter = Random.Range(5f, 9f);

        _controller.collisions.below = false; // allows to move the player in y direction on ground

        Velocity += new Vector3(randomXJitter, randomYJitter, 0);

        var timer = _secondsInvincibility;
        while (timer > .0f)
        {
            timer -= Time.deltaTime;
            yield return null;
        }

        _animator.SetBool("Damaged", false);
    }

    private void UpdateMovement()
    {
        if (_controller.collisions.above || _controller.collisions.below)
        {
            if (!IgnoreGround)
            {
                Velocity.y = 0;
                //_velocity.y = -_velocity.y; TODO : PRODUCES BOUNCING
            }
            else
            {
                _controller.collisions.below = false; // allows to move the player in y direction on ground
            }
        }

        var input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        input = ConvertToInteger(input);


        if (input != Vector2.zero)
        {
            _lastInput = input;
        }

        Debug.DrawRay(transform.position, input, Color.yellow);


        if (Input.GetButtonDown("Jump") && _controller.collisions.below)
        {
            Velocity.y = _maxJumpVelocity;
        }

        if (Input.GetButtonUp("Jump") && _canBoost)
        {
            if (Velocity.y > _minJumpVelocity)
            {
                Velocity.y = _minJumpVelocity;
            }
        }

        var targetVelocityX = Mathf.Round(input.x) * _moveSpeed;

        Velocity.x = Mathf.SmoothDamp(Velocity.x, targetVelocityX, ref _velocityXSmoothing,
            _controller.collisions.below ? _accelerationTimeGrounded : _accelerationTimeAirborne);

        Velocity.y += Physics2D.gravity.x * Time.deltaTime;
        _controller.Move(Velocity * Time.deltaTime);
    }

    private static Vector2 ConvertToInteger(Vector2 input)
    {
        if (input.x < 0)
        {
            input.x = -1;
        }

        if (input.x > 0)
        {
            input.x = 1;
        }

        if (input.y < 0)
        {
            input.y = -1;
        }

        if (input.y > 0)
        {
            input.y = 1;
        }

        return input;
    }


}