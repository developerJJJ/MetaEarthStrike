using UnityEngine;
using UnityEngine.UI;
using TMPro; // Add this for TextMesh Pro support

public class GameManager : MonoBehaviour
{
    public TMP_Text goldText; // Reference to UI Text showing player's gold
    public int goldIncresement = 1;
    public int playerGold = 0;
    public int enemyGold = 0;
    public Transform playerSpawnPoint;
    public Transform enemySpawnPoint;
    public GameObject antPrefab; // Reference to the Ant Warrior prefab
    public Button spawnButton;
    public float spawnInterval = 5f; // Time between spawns
    private float playerSpawnTimer = 0f;
    private float enemySpawnTimer = 0f;
    private float _lastGoldUpdateTime = 0f; // Declare the variable here

    void Start()
    {
        SpawnUnit(antPrefab, true); // Player unit
        SpawnUnit(antPrefab, false); // Enemy unit
        spawnButton.onClick.AddListener(SpawnAntWarrior);
    }

    // Method to spawn unit when button is clicked
    public void SpawnAntWarrior()
    {
        int cost = 10; // Cost to spawn an Ant Warrior

        if (playerGold >= cost)
        {
            playerGold -= cost;
            SpawnUnit(antPrefab, true); // Spawn unit for player
        }
        else
        {
            Debug.Log("Not enough gold to spawn Ant Warrior!");
        }
    }
    public void SpawnUnit(GameObject unitPrefab, bool isPlayer)
    {
        // Determine spawn location based on ownership
        Transform spawnLocation = isPlayer ? playerSpawnPoint : enemySpawnPoint;

        // Instantiate the unit at the determined spawn location
        GameObject unit = Instantiate(unitPrefab, spawnLocation.position, Quaternion.identity);

        // Set the unit tag based on ownership
        unit.tag = isPlayer ? "PlayerUnit" : "EnemyUnit";

        // Set unit color based on ownership
        SpriteRenderer spriteRenderer = unit.GetComponent<SpriteRenderer>();
        // Inside SpawnUnit method
        if (spriteRenderer != null)
        {
            spriteRenderer.color = isPlayer ? new Color(0.5f, 0.5f, 1f, 1f) : new Color(1f, 0.5f, 0.5f, 1f);
        }

        // Set movement direction using the updated Unit class
        Unit unitScript = unit.GetComponent<Unit>();
        if (unitScript != null)
        {
            unitScript.SetDirection(isPlayer ? 1f : -1f); // Right for player, left for enemy
        }
    }

    void Update()
    {
        // Update timers
        playerSpawnTimer += Time.deltaTime;
        enemySpawnTimer += Time.deltaTime;

        // Spawn player unit if the timer exceeds the interval
        if (playerSpawnTimer >= spawnInterval)
        {
            SpawnUnit(antPrefab, true); // Player unit
            playerSpawnTimer = 0f;
        }

        // Spawn enemy unit if the timer exceeds the interval
        if (enemySpawnTimer >= spawnInterval)
        {
            SpawnUnit(antPrefab, false); // Enemy unit
            enemySpawnTimer = 0f;
        }


        playerGold += Mathf.FloorToInt(goldIncresement * Time.deltaTime * 10f); // Smooth increment
        // // Track time since last gold increase
        // float currentTime = Time.time;
        // float timeSinceLastGoldIncrease = currentTime - _lastGoldUpdateTime;

        // if (timeSinceLastGoldIncrease >= 0.1f)
        // {
        //     _lastGoldUpdateTime = currentTime;
        //     playerGold += goldIncresement;
        // }

        if (goldText != null)
        {
            goldText.text = "Gold: " + playerGold.ToString();
        }
    }

}
