using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [SerializeField] Vector3 spawnPosition;
    public GameObject enemy;
    public bool encounter;
    PlayerController player;
    public GameObject startMenu;
    public GameObject endMenu;
    public TextMeshProUGUI endTitle;
    public Button playButton;
    public Button restartButton;
    public Button exitButton;

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
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();

        playButton.onClick.AddListener(Play);
        restartButton.onClick.AddListener(Restart);
        exitButton.onClick.AddListener(Exit);

        moveSpeed = 10.0f;
        attackLength = 1.0f;
        impactDelay = 1.0f;
        counterThreshold = 0.75f;

        enemyIntelligence = 1;
        enemyDelay = 1;

        spawnTime = 3.0f;
        encounter = true;
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
        if(enemiesDefeated >= 5)
        {
            player.endDance();
            return;
        }
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

    void Play()
    {
        startMenu.SetActive(false);
        player.startRunning();
        encounter = false;
        StartCoroutine("SpawnEnemy");
    }

    void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void Exit()
    {
        Application.Quit();
    }

    public void Lose()
    {
        endTitle.color = Color.red;
        endTitle.text = "Game Over!";
        endMenu.SetActive(true);
    }

    public void Win()
    {
        endTitle.color = Color.white;
        endTitle.text = "You Win!";
        endMenu.SetActive(true);
    }
}
