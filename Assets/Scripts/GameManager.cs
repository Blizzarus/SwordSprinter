using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] Vector3 spawnPosition;
    public GameObject enemy;
    public bool encounter;

    public float moveSpeed = 8.0f;
    public float attackLength = 1.0f;
    public float impactDelay = 0.5f;
    public float counterThreshold = 0.9f;

    public int enemyIntelligence = 1;
    public float enemyDelay = 0.5f;
    float spawnTime = 5.0f;

    public int enemiesDefeated;
    void Start()
    {
        //Time.timeScale = 0.2f;
        encounter = false;
        StartCoroutine("SpawnEnemy");
    }

    IEnumerator SpawnEnemy()
    {
        yield return new WaitForSeconds(spawnTime);
        Instantiate(enemy, spawnPosition, enemy.transform.rotation);
    }

    public void EncounterStart()
    {
        encounter = true;
    }

    public void EncounterEnd()
    {
        encounter = false;
        enemiesDefeated++;
        enemyIntelligence = enemiesDefeated switch
        {
            <= 1 => 1,
            > 1 and <= 4 => 2,
            > 4 and <= 9 => 3,
            > 9 and <= 20 => 4,
            > 20 => 5
        };
        enemyDelay = enemiesDefeated switch
        {
            <= 4 => 0.5f,
            > 4 and <= 9 => 0.4f,
            > 9 and <= 20 => 0.3f,
            > 20 => 0.2f
        };
        StartCoroutine("SpawnEnemy");
    }
}
