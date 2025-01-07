using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    public float normalTimeScale = 1f;  // Normal speed
    public float acceleratedTimeScale = 5f;  // Accelerated speed
    public TMP_Text goldText;
    public float goldIncrementPerSecond = 10f;
    public int playerGold = 0;
    public int enemyGold = 0;
    public Transform playerSpawnPoint;
    public Transform enemySpawnPoint;
    public LayerMask groundLayer; // Add a LayerMask to filter the raycast
    public GameObject prefabWarrior;
    public GameObject prefabArcher;
    public Button spawnButton1;
    public Button spawnButton2;
    public float spawnInterval = 10f;
    public GameObject enemyBase;
    public GameObject winPanel;
    public Button restartButton;
    public TMP_Text winText;
    private Transform spawnLocation;
    private float playerSpawnTimer = 0f;
    private float enemySpawnTimer = 0f;
    private float goldAccumulator = 0f;
    private int accumulatedGold = 0;
    private bool isGameOver = false;

    void Start()
    {
        Time.timeScale = acceleratedTimeScale;

        SpawnUnit(prefabWarrior, true);
        SpawnUnit(prefabWarrior, false);
        spawnButton1.onClick.AddListener(() => SpawnUnitWithCost(1));
        spawnButton2.onClick.AddListener(() => SpawnUnitWithCost(2));
        winPanel.SetActive(false);
        restartButton.onClick.AddListener(RestartGame);
    }

    public void SpawnUnitWithCost(int buttonNumber)
    {
        if (isGameOver) return;

        int cost;
        GameObject unitPrefab;

        if (buttonNumber == 1)
        {
            cost = 10;
            unitPrefab = prefabWarrior;
        }
        else if (buttonNumber == 2)
        {
            cost = 12;
            unitPrefab = prefabArcher;
        }
        else
        {
            return;
        }

        if (playerGold >= cost)
        {
            playerGold -= cost;
            SpawnUnit(unitPrefab, true);
        }
    }

    public void SpawnUnit(GameObject unitPrefab, bool isPlayer)
    {
        if (isGameOver) return;

        Transform spawnLocation = isPlayer ? playerSpawnPoint : enemySpawnPoint;

        RaycastHit2D hit = Physics2D.Raycast(spawnLocation.position, Vector2.down, Mathf.Infinity, groundLayer);

        if (hit.collider != null)
        {
            // 1. Instantiate the unit FIRST
            GameObject unitInstance = Instantiate(unitPrefab, spawnLocation.position, Quaternion.identity);

            // 2. Get the Collider2D *from the INSTANCE*
            BoxCollider2D unitCollider = unitInstance.GetComponent<BoxCollider2D>(); // Or your specific collider type
            if (unitCollider == null)
            {
                Debug.LogError("Unit instance does not have a BoxCollider2D!");
                Destroy(unitInstance); // Clean up the instantiated unit
                return;
            }

            // 3. Get the bounds *after* instantiation
            Bounds unitBounds = unitCollider.bounds;

            if (unitBounds.size.y == 0)
            {
                Debug.LogError("Unit collider has a height of 0! Check the collider on the prefab.");
                Destroy(unitInstance); // Clean up
                return;
            }

            Vector2 groundTopWorld = hit.point;

            // 4. Calculate the correct spawn position
            Vector2 spawnPosition = new Vector2(spawnLocation.position.x, groundTopWorld.y + unitBounds.extents.y);

            // 5. Move the instance to the correct position
            unitInstance.transform.position = spawnPosition;

            Unit unitScript = unitInstance.GetComponent<Unit>();
            if (unitScript != null)
            {
                unitScript.team = isPlayer ? "Player" : "Enemy";
                unitScript.SetDirection(isPlayer ? 1f : -1f);
            }

            unitInstance.tag = isPlayer ? "PlayerUnit" : "EnemyUnit";

            SpriteRenderer spriteRenderer = unitInstance.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                spriteRenderer.color = isPlayer ? new Color(0.5f, 0.5f, 1f, 1f) : new Color(1f, 0.5f, 0.5f, 1f);
            }
        }
        else
        {
            Debug.LogError("No ground found below spawn point!");
        }
    }

    void Update()
    {
        if (isGameOver) return;

        playerSpawnTimer += Time.deltaTime;
        enemySpawnTimer += Time.deltaTime;

        if (playerSpawnTimer >= spawnInterval)
        {
            SpawnUnit(prefabWarrior, true);
            playerSpawnTimer = 0f;
        }

        if (enemySpawnTimer >= spawnInterval)
        {
            SpawnUnit(prefabWarrior, false);
            enemySpawnTimer = 0f;
        }

        goldAccumulator += goldIncrementPerSecond * Time.deltaTime;

        if (goldAccumulator >= 1f)
        {
            accumulatedGold = Mathf.FloorToInt(goldAccumulator);
            playerGold += accumulatedGold;
            goldAccumulator -= accumulatedGold;
        }

        if (goldText != null)
        {
            goldText.text = "Gold: " + playerGold.ToString();
        }

        CheckWinCondition();
    }

    private void CheckWinCondition()
    {
        if (enemyBase == null)
        {
            PlayerWins();
        }
    }

    private void PlayerWins()
    {
        isGameOver = true;

        if (winPanel != null)
        {
            winPanel.SetActive(true);
        }
        if (winText != null)
        {
            winText.text = "You Win!";
        }
    }

    public void RestartGame()
    {
        if (isGameOver)
        {
            isGameOver = false;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            Debug.Log("Restarting game");
        }
    }
}