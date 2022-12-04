using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] Vector3 spawnPosition;
    public GameObject enemy;
    public bool encounter;

    public float moveSpeed;
    public float attackLength;
    public float impactDelay;
    public float counterThreshold;

    public int enemyIntelligence;
    public float enemyDelay;
    float spawnTime;

    public int enemiesDefeated;
    void Awake()
    {
        moveSpeed = 10.0f;
        attackLength = 2.0f;
        impactDelay = 1;
        counterThreshold = 1.5f;

        enemyIntelligence = 1;
        enemyDelay = 1;

        spawnTime = 3.0f;

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
            <= 1 => 1,
            > 1 and <= 4 => 0.9f,
            > 4 and <= 9 => 0.8f,
            > 9 and <= 20 => 0.7f,
            > 20 => 0.6f
        };
        StartCoroutine("SpawnEnemy");
    }
}
