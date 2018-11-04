using UnityEngine;
using UnityEngine.SceneManagement;

public class Exit : MonoBehaviour
{
    public string nextScene;

    public Sprite offSprite;
    public Sprite onSprite;


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
            Player.Score += Player.SCORE_PERLEVEL;
            SceneManager.LoadScene(nextScene);
        }
    }
}