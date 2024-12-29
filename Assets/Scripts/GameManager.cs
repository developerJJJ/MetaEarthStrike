using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    public TMP_Text goldText;
    public float goldIncrementPerSecond = 1f; // Gold gained per second (float for smoother increase)
    public int playerGold = 0;
    public int enemyGold = 0;
    public Transform playerSpawnPoint;
    public Transform enemySpawnPoint;
    public GameObject antPrefab;
    public Button spawnButton;
    public float spawnInterval = 5f;
    private float playerSpawnTimer = 0f;
    private float enemySpawnTimer = 0f;

    void Start()
    {
        SpawnUnit(antPrefab, true);
        SpawnUnit(antPrefab, false);
        spawnButton.onClick.AddListener(SpawnUnitWithCost);
    }

    public void SpawnUnitWithCost()
    {
        int cost = 10;

        if (playerGold >= cost)
        {
            playerGold -= cost;
            SpawnUnit(antPrefab, true);
        }
        else
        {
            Debug.LogError("Not enough gold to spawn Unit!");
        }
    }

    public void SpawnUnit(GameObject unitPrefab, bool isPlayer)
    {
        Transform spawnLocation = isPlayer ? playerSpawnPoint : enemySpawnPoint;
        GameObject unit = Instantiate(unitPrefab, spawnLocation.position, Quaternion.identity);
        unit.tag = isPlayer ? "PlayerUnit" : "EnemyUnit";

        SpriteRenderer spriteRenderer = unit.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.color = isPlayer ? new Color(0.5f, 0.5f, 1f, 1f) : new Color(1f, 0.5f, 0.5f, 1f);
        }

        Unit unitScript = unit.GetComponent<Unit>();
        if (unitScript != null)
        {
            unitScript.SetDirection(isPlayer ? 1f : -1f);
        }
    }

    private float goldAccumulator = 0f; // Add this line
    private int accumulatedGold = 0; // Add this line

    void Update()
    {
        playerSpawnTimer += Time.deltaTime;
        enemySpawnTimer += Time.deltaTime;

        if (playerSpawnTimer >= spawnInterval)
        {
            SpawnUnit(antPrefab, true);
            playerSpawnTimer = 0f;
        }

        if (enemySpawnTimer >= spawnInterval)
        {
            SpawnUnit(antPrefab, false);
            enemySpawnTimer = 0f;
        }

        // Corrected gold increment logic
        goldAccumulator += goldIncrementPerSecond * Time.deltaTime;

        if (goldAccumulator >= 1f)
        {
            accumulatedGold = Mathf.FloorToInt(goldAccumulator);
            playerGold += accumulatedGold; // Add the accumulated gold
            goldAccumulator -= accumulatedGold; // Reset the accumulator, keeping the fractional part
        }

        Debug.Log("Player Gold: " + playerGold + ", Accumulator: " + goldAccumulator + ", Time.deltaTime: " + Time.deltaTime); // Improved debug

        if (goldText != null)
        {
            goldText.text = "Gold: " + playerGold.ToString();
        }
    }

}