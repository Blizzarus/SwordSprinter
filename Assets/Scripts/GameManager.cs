using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] Vector3 spawnPosition;
    public GameObject enemy;
    public enum combatState { Neutral = 0, UpwardAtk = 1, RightwardAtk = 2, DownwardAtk = 3, LeftwardAtk = 4, Staggered = 5 }
    public bool encounter;
    public float speed = 20.0f;
    // Start is called before the first frame update
    void Start()
    {
        encounter = false;
        SpawnEnemy();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void SpawnEnemy()
    {
        Instantiate(enemy, spawnPosition, enemy.transform.rotation);
    }

    public void EncounterStart()
    {
        encounter = true;
    }

    public void EncounterEnd()
    {
        encounter = false;
        SpawnEnemy();
    }
}
