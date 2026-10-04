using UnityEngine;

public class CoinRush : MonoBehaviour
{
    private int score;
    private int lives = 3;
    private float timeLeft = 60f;
    private float playerX;
    private bool playing;
    private bool gameOver;
    private float spawnTimer;

    private const float PlayerWidth = 1.8f;
    private const float PlayerY = -3.8f;
    private const float CoinSize = 0.55f;

    private GUIStyle titleStyle;
    private GUIStyle hudStyle;
    private GUIStyle buttonStyle;

    private void Start()
    {
        playerX = 0f;
        CreateStyles();
    }

    private void Update()
    {
        if (!playing) return;

        timeLeft -= Time.deltaTime;
        spawnTimer -= Time.deltaTime;

        float input = Input.GetAxisRaw("Horizontal");
        playerX += input * 6f * Time.deltaTime;
        playerX = Mathf.Clamp(playerX, -3.6f, 3.6f);

        if (spawnTimer <= 0f)
        {
            SpawnCoin();
            spawnTimer = Mathf.Max(0.25f, 0.7f - score * 0.0015f);
        }

        if (timeLeft <= 0f)
            EndGame();
    }

    private void CreateStyles()
    {
        titleStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 52,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter
        };

        hudStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 28,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter
        };

        buttonStyle = new GUIStyle(GUI.skin.button)
        {
            fontSize = 34,
            fontStyle = FontStyle.Bold
        };
    }

    private void SpawnCoin()
    {
        GameObject coin = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        coin.name = "Coin";
        coin.transform.position = new Vector3(Random.Range(-3.6f, 3.6f), 5.5f, 0f);
        coin.transform.localScale = Vector3.one * CoinSize;

        Renderer renderer = coin.GetComponent<Renderer>();
        renderer.material.color = Color.yellow;

        Rigidbody body = coin.AddComponent<Rigidbody>();
        body.useGravity = false;
        body.velocity = Vector3.down * (3.2f + score * 0.01f);

        CoinObject coinObject = coin.AddComponent<CoinObject>();
        coinObject.game = this;
    }

    public void CollectCoin(GameObject coin)
    {
        score += 10;
        Destroy(coin);
    }

    public void MissedCoin(GameObject coin)
    {
        lives--;
        Destroy(coin);

        if (lives <= 0)
            EndGame();
    }

    private void EndGame()
    {
        playing = false;
        gameOver = true;
    }

    private void StartGame()
    {
        foreach (CoinObject coin in FindObjectsOfType<CoinObject>())
            Destroy(coin.gameObject);

        score = 0;
        lives = 3;
        timeLeft = 60f;
        playerX = 0f;
        spawnTimer = 0f;
        playing = true;
        gameOver = false;
    }

    private void OnGUI()
    {
        if (titleStyle == null) CreateStyles();

        GUI.Label(new Rect(20, 15, Screen.width - 40, 70),
            "COIN RUSH", titleStyle);

        if (!playing && !gameOver)
        {
            GUI.Label(new Rect(20, Screen.height * 0.35f,
                Screen.width - 40, 100),
                "Catch the falling coins!", hudStyle);

            if (GUI.Button(new Rect(Screen.width * 0.25f, Screen.height * 0.55f,
                Screen.width * 0.5f, 90), "START GAME", buttonStyle))
                StartGame();

            return;
        }

        GUI.Label(new Rect(10, 90, Screen.width / 3f, 50),
            "Score: " + score, hudStyle);
        GUI.Label(new Rect(Screen.width / 3f, 90, Screen.width / 3f, 50),
            "Lives: " + lives, hudStyle);
        GUI.Label(new Rect(Screen.width * 2f / 3f, 90, Screen.width / 3f, 50),
            "Time: " + Mathf.Max(0, Mathf.CeilToInt(timeLeft)), hudStyle);

        float px = (playerX + 4.5f) / 9f * Screen.width;
        float py = Screen.height * 0.82f;
        GUI.Box(new Rect(px - 55, py, 110, 55), "PLAYER");

        if (gameOver)
        {
            GUI.Label(new Rect(20, Screen.height * 0.4f,
                Screen.width - 40, 80),
                "GAME OVER\nScore: " + score, titleStyle);

            if (GUI.Button(new Rect(Screen.width * 0.25f, Screen.height * 0.62f,
                Screen.width * 0.5f, 90), "PLAY AGAIN", buttonStyle))
                StartGame();
        }

        // Large mobile controls.
        if (GUI.RepeatButton(new Rect(20, Screen.height - 130, 180, 100), "LEFT", buttonStyle))
            playerX = Mathf.Clamp(playerX - 4f * Time.deltaTime, -3.6f, 3.6f);

        if (GUI.RepeatButton(new Rect(Screen.width - 200, Screen.height - 130, 180, 100), "RIGHT", buttonStyle))
            playerX = Mathf.Clamp(playerX + 4f * Time.deltaTime, -3.6f, 3.6f);
    }
}

public class CoinObject : MonoBehaviour
{
    public CoinRush game;

    private void Update()
    {
        if (game == null) return;

        Vector3 screen = Camera.main.WorldToScreenPoint(transform.position);
        float playerX = 0f;

        // Simple catch zone around the player.
        float distanceX = Mathf.Abs(transform.position.x - playerX);

        if (transform.position.y < -3.2f && distanceX < 1.15f)
        {
            game.CollectCoin(gameObject);
            return;
        }

        if (transform.position.y < -6f)
            game.MissedCoin(gameObject);
    }
}
