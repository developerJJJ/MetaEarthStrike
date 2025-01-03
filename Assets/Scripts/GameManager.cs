using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    public TMP_Text goldText;
    public float goldIncrementPerSecond = 10f; // Gold gained per second (float for smoother increase)
    public int playerGold = 0;
    public int enemyGold = 0;
    public Transform playerSpawnPoint;
    public Transform enemySpawnPoint;
    public GameObject prefabWarrior;
    public GameObject prefabArcher;
    public Button spawnButton1;
    public Button spawnButton2;
    public float spawnInterval = 10f;
    private float playerSpawnTimer = 0f;
    private float enemySpawnTimer = 0f;
    private float goldAccumulator = 0f; // Add this line
    private int accumulatedGold = 0; // Add this line

    void Start()
    {
        SpawnUnit(prefabWarrior, true);
        SpawnUnit(prefabWarrior, false);
        spawnButton1.onClick.AddListener(() => SpawnUnitWithCost(1));
        spawnButton2.onClick.AddListener(() => SpawnUnitWithCost(2));
    }

    public void SpawnUnitWithCost(int buttonNumber)
    {
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
            return; // Invalid button number
        }

        if (playerGold >= cost)
        {
            playerGold -= cost;
            SpawnUnit(unitPrefab, true);
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

    void Update()
    {
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

        // Corrected gold increment logic
        goldAccumulator += goldIncrementPerSecond * Time.deltaTime;

        if (goldAccumulator >= 1f)
        {
            accumulatedGold = Mathf.FloorToInt(goldAccumulator);
            playerGold += accumulatedGold; // Add the accumulated gold
            goldAccumulator -= accumulatedGold; // Reset the accumulator, keeping the fractional part
        }

        if (goldText != null)
        {
            goldText.text = "Gold: " + playerGold.ToString();
        }
    }

}