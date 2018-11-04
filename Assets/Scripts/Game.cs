using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


// some god object to setup the game
public class Game : MonoBehaviour
{

    public List<GameObject> buttons;
    
    public static Game Instance;
    private CheatState _cheatState;

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

        yield return StartCoroutine(ShowButtons());

    }

    private void Awake()
    {
        buttons.ForEach(button =>
        {
            var color = button.GetComponent<Image>().color;
            color.a = 0f;
            button.GetComponent<Image>().color = color;
            
        });
        
        if (!Instance)
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);

        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    private IEnumerator ShowButtons()
    {
        
        yield return new WaitForSeconds(1.0f);
        var timer = 0f;

        while ((timer += Time.deltaTime) < 1.0f)
        {
            buttons.ForEach(button =>
            {
                var col = button.GetComponent<Image>().color;
                col.a = Mathf.Lerp(0f, 1f, timer / 1.0f);
                button.GetComponent<Image>().color = col;
            });
            
            
            yield return null;
        }
        
        buttons.ForEach(button =>
        {
            var col = button.GetComponent<Image>().color;
            col.a = 1f;
            button.GetComponent<Image>().color = col;
        });
        
        yield return new WaitForSeconds(4.0f);
        
        timer = 0f;

        while ((timer += Time.deltaTime) < 1.0f)
        {
            buttons.ForEach(button =>
            {
                var col = button.GetComponent<Image>().color;
                col.a = Mathf.Lerp(1f, 0f, timer / 1.0f);
                button.GetComponent<Image>().color = col;
            });
            
            
            yield return null;
        }
        
        buttons.ForEach(button =>
        {
            var col = button.GetComponent<Image>().color;
            col.a = 0f;
            button.GetComponent<Image>().color = col;
        });
        
        yield return null;
    }
    private void CheckCheat()
    {
        if (Input.anyKeyDown)
        {
            if (Input.GetKeyDown(KeyCode.F12))
            {
                switch (_cheatState)
                {
                    case CheatState.NOTHING_PRESSED:
                        _cheatState = CheatState.FIRST_PRESS;
                        break;
                    case CheatState.FIRST_PRESS:
                        _cheatState = CheatState.ACTIVATED;
                        StartCoroutine(ActivateCheat());
                        break;
                    case CheatState.ACTIVATED:
                        break;
                }
            }
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
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            Player.Score -= Player.SCORE_LOSS_PER_DEATH;
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            AudioManager.Instance.StopAllMusic();
            Destroy(GameObject.Find("UI"));
            SceneManager.LoadScene("Menu");
        }
        
        CheckCheat();
        
        if (_cheatState != CheatState.ACTIVATED)
        {
            return;
        }
        if (Input.GetKeyDown(KeyCode.F1))
        {
            SceneManager.LoadScene("Level1");
        }        
        if (Input.GetKeyDown(KeyCode.F2))
        {
            SceneManager.LoadScene("Level2");
        }        
        if (Input.GetKeyDown(KeyCode.F3))
        {
            SceneManager.LoadScene("Level3");
        }        
        if (Input.GetKeyDown(KeyCode.F4))
        {
            SceneManager.LoadScene("Level4");
        }        
        if (Input.GetKeyDown(KeyCode.F5))
        {
            SceneManager.LoadScene("Level5");
        }
        if (Input.GetKeyDown(KeyCode.F6))
        {
            SceneManager.LoadScene("Level6");
        }
    }
}