using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public enum Phase { Plant, Grow, Enemy, Harvest, GameOver }

    public static GameManager Instance;

    public Phase currentPhase;

    public int score = 0;
    public int energy = 100;
    public int maxEnergy = 100;
    public int lives = 3;

    public float plantTime = 5f;
    public float growTime = 2f;
    public float harvestTime = 4f;

    public TextMeshProUGUI livesText;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI energyText;
    public TextMeshProUGUI messageText;
    public TextMeshProUGUI phaseText;
    public TextMeshProUGUI phaseTimerText;
    public int highScore = 0;
    public TextMeshProUGUI highScoreText;

    public GameObject gameOverPanel;
    public GameObject enemyPrefab;

    public int enemySpawnGap = 3;
    private int roundCount = 0;

    private Coroutine messageRoutine;
    private bool isGameOver = false;
    private bool canTakeDamage = true;
    public TextMeshProUGUI startCountdownText;
    public CanvasGroup darkOverlay;

    [Header("Start Menu")]
    public GameObject startPanel;
    public GameObject helpPanel;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        highScore = PlayerPrefs.GetInt("HighScore", 0);

        UpdateLivesUI();
        UpdateScoreUI();
        UpdateEnergyUI();
        UpdateHighScoreUI();

        if (PlayerPrefs.GetInt("SkipStartPanel", 0) == 1)
        {
            PlayerPrefs.SetInt("SkipStartPanel", 0);

            if (startPanel != null)
                startPanel.SetActive(false);

            StartCoroutine(BeginGame());
        }
        else
        {
            Time.timeScale = 0f;

            if (startPanel != null)
                startPanel.SetActive(true);

            if (helpPanel != null)
                helpPanel.SetActive(false);
        }
    }

    public void AddScore(int amount)
    {
        if (isGameOver) return;

        score += amount;

        if (score > highScore)
        {
            highScore = score;

            PlayerPrefs.SetInt("HighScore", highScore);
        }

        UpdateScoreUI();
        UpdateHighScoreUI();
    }
    public bool UseEnergy(int amount)
    {
        if (isGameOver) return false;

        if (energy < amount)
        {
            Debug.Log("Not enough energy!");
            return false;
        }

        energy -= amount;
        UpdateEnergyUI();

        return true;
    }

    public void RestoreEnergy(int amount)
    {
        if (isGameOver) return;

        energy += amount;

        if (energy > maxEnergy)
            energy = maxEnergy;

        UpdateEnergyUI();
    }

    public void TakeDamage(int amount)
    {
        if (isGameOver || !canTakeDamage) return;

        canTakeDamage = false;

        lives--;
        UpdateLivesUI();
        ShowMessage("-1 Life!");

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayDamage();

        if (lives <= 0)
            GameOver();
        else
            StartCoroutine(DamageCooldown());
    }

    void GameOver()
    {
        isGameOver = true;
        currentPhase = Phase.GameOver;

        ShowMessage("GAME OVER");

        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayGameOver();

        Time.timeScale = 0f;
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;

        PlayerPrefs.SetInt("SkipStartPanel", 1);

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    IEnumerator GameLoop()
    {
        while (!isGameOver)
        {
            currentPhase = Phase.Plant;
            ShowPhase("PLANT PHASE");
            yield return StartCoroutine(PhaseTimer("PLANT PHASE", plantTime));

            currentPhase = Phase.Grow;
            ShowPhase("GROW PHASE");
            GridManager.Instance.GrowAllTiles();
            yield return StartCoroutine(PhaseTimer("GROW PHASE", growTime));

            currentPhase = Phase.Enemy;
            roundCount++;

            if (roundCount == 1 || (roundCount - 1) % enemySpawnGap == 0)
            {
                ShowPhase("ENEMY PHASE");
                SpawnEnemy();
                yield return StartCoroutine(PhaseTimer("ENEMY PHASE", 2f));
            }

            currentPhase = Phase.Harvest;
            ShowPhase("HARVEST PHASE");
            yield return StartCoroutine(PhaseTimer("HARVEST PHASE", harvestTime));
        }
    }

    void SpawnEnemy()
    {
        if (enemyPrefab == null) return;

        int x = Random.Range(0, GridManager.Instance.gridWidth);
        int y = Random.Range(0, GridManager.Instance.gridHeight);

        Vector3 spawnPos = new Vector3(
            x - 3.5f,
            y - 3.5f,
            -0.4f
        );

        Instantiate(enemyPrefab, spawnPos, Quaternion.identity);

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayEnemySpawn();
    }

    void ShowPhase(string phaseName)
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayPhase();
    }

    IEnumerator PhaseTimer(string phaseName, float duration)
    {
        float timer = duration;

        while (timer > 0)
        {
            if (phaseTimerText != null)
                phaseTimerText.text = phaseName + " : " + Mathf.CeilToInt(timer);

            timer -= Time.deltaTime;
            yield return null;
        }

        if (phaseTimerText != null)
            phaseTimerText.text = phaseName + " : 0";
    }

    public void ShowMessage(string message)
    {
        if (messageText == null) return;

        if (messageRoutine != null)
            StopCoroutine(messageRoutine);

        messageRoutine = StartCoroutine(ShowMessageRoutine(message));
    }

    IEnumerator ShowMessageRoutine(string message)
    {
        messageText.text = message;

        yield return new WaitForSeconds(2f);

        if (!isGameOver)
            messageText.text = "";
    }

    IEnumerator DamageCooldown()
    {
        yield return new WaitForSeconds(1f);
        canTakeDamage = true;
    }

    void UpdateScoreUI()
    {
        if (scoreText != null)
            scoreText.text = "Score: " + score;
    }

    void UpdateEnergyUI()
    {
        if (energyText != null)
            energyText.text = "Energy: " + energy;
    }

    void UpdateLivesUI()
    {
        if (livesText != null)
        {
            string hearts = "";

            for (int i = 0; i < lives; i++)
                hearts += "♥ ";

            livesText.text = hearts;
        }
    }
    IEnumerator StartCountdown()
    {
        if (startCountdownText == null)
            yield break;

        darkOverlay.alpha = 1f;

        startCountdownText.gameObject.SetActive(true);

        startCountdownText.text = "3";
        yield return new WaitForSecondsRealtime(1f);

        startCountdownText.text = "2";
        yield return new WaitForSecondsRealtime(1f);

        startCountdownText.text = "1";
        yield return new WaitForSecondsRealtime(1f);

        startCountdownText.text = "START!";
        yield return new WaitForSecondsRealtime(0.5f);

        startCountdownText.gameObject.SetActive(false);

        // fade out darkness
        while (darkOverlay.alpha > 0)
        {
            darkOverlay.alpha -= Time.unscaledDeltaTime * 2f;
            yield return null;
        }

        darkOverlay.alpha = 0f;
    }
    IEnumerator BeginGame()
    {
        Time.timeScale = 0f;

        yield return StartCoroutine(StartCountdown());

        Time.timeScale = 1f;

        StartCoroutine(GameLoop());
    }
    void UpdateHighScoreUI()
    {
        if (highScoreText != null)
        {
            highScoreText.text = "BEST : " + highScore;
        }
    }
    public void StartGame()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayButton();

        if (startPanel != null)
            startPanel.SetActive(false);

        StartCoroutine(BeginGame());
    }

    public void ToggleHelp()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayButton();

        if (helpPanel != null)
            helpPanel.SetActive(!helpPanel.activeSelf);
    }

    public void ExitGame()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayButton();

        Application.Quit();
    }
    public void CheckAllTilesCorrupt()
    {
        if (GridManager.Instance.AreAllTilesCorrupt())
        {
            GameOver();
        }
    }
}