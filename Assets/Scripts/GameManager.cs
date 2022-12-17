using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    Vector3 spawnPosition;
    AudioSource audio;
    [SerializeField] AudioClip backgroundMusic;
    [SerializeField] AudioClip winMusic;
    [SerializeField] AudioClip[] clashSounds;
    [SerializeField] AudioClip[] hurtSounds;
    [SerializeField] AudioClip[] dieSounds;
    public TextMeshProUGUI timer;
    int timeElapsed;
    public GameObject enemy;
    public bool encounter;
    PlayerController player;
    public GameObject startMenu;
    public GameObject endMenu;
    public GameObject startView;
    public GameObject tutorialView;
    public TextMeshProUGUI endTitle;
    public TextMeshProUGUI scoreboard;
    public Button playButton;
    public Toggle cheatModeToggle;
    public Toggle hardModeToggle;
    public Button tutorialButton;
    public Button restartButton;
    public Button exitButton;

    public float moveSpeed;
    public float attackLength;
    public float impactDelay;
    public float counterThreshold;
    [SerializeField] float spawnTime;
    [SerializeField] int winCon;

    int lastClash;
    int lastHurt;
    int lastDie;

    public int enemyIntelligence;
    public float enemyDelay;
    int enemiesDefeated;
    public bool cheatMode;
    void Awake()
    {
        startMenu.SetActive(true);
        audio = GetComponent<AudioSource>();
        audio.clip = backgroundMusic;
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
        spawnPosition = player.transform.position + new Vector3(35,0,0);
        lastClash = lastHurt = lastDie = -1;

        setEnemyStats();

        enemiesDefeated = 0;
        encounter = true;
        cheatMode = false;

        playButton.onClick.AddListener(Play);
        restartButton.onClick.AddListener(Restart);
        exitButton.onClick.AddListener(Exit);
        tutorialButton.onClick.AddListener(toggleTutorial);
    }

    IEnumerator SpawnEnemy()
    {
        yield return new WaitForSeconds(spawnTime);
        Instantiate(enemy, spawnPosition, enemy.transform.rotation);
    }

    void UpdateTimer()
    {
        timeElapsed++;
        timer.text = TimeSpan.FromSeconds(timeElapsed).ToString("mm\\:ss");
    }

    public void EncounterStart()
    {
        encounter = true;
    }

    public void EncounterEnd()
    {
        encounter = false;
        enemiesDefeated++;
        if(enemiesDefeated >= winCon)
        {
            CancelInvoke();
            audio.clip = winMusic;
            audio.Play();
            player.EndDance();
            return;
        }

        setEnemyStats();
        StartCoroutine(SpawnEnemy());
    }

    void setEnemyStats()
    {
        enemyIntelligence = enemiesDefeated switch
        {
            <= 1 => 1,
            > 1 and <= 2 => 2,
            > 2 and <= 4 => 3,
            > 4 and <= 6 => 4,
            > 6 and <= 8 => 5,
            > 8 and <= 10 => 6,
            > 10 and <= 12 => 7,
            > 12 and <= 14 => 8,
            > 14 and <= 16 => 9,
            > 16 and <= 18 => 10,
            > 18 and <= 20 => 12,
            > 20 => 15
        };
        enemyDelay = enemiesDefeated switch
        {
            <= 1 => 1f,
            > 1 and <= 4 => 0.9f,
            > 4 and <= 9 => 0.75f,
            > 9 and <= 20 => 0.5f,
            > 20 => 0.4f
        };
    }

    public void clashSFX()
    {
        int rand;
        do
        {
            rand = UnityEngine.Random.Range(0, clashSounds.Length);
        } while (rand == lastClash);
        lastClash = rand;
        audio.PlayOneShot(clashSounds[rand]);
    }

    public void hurtSFX()
    {
        int rand;
        do
        {
            rand = UnityEngine.Random.Range(0, hurtSounds.Length);
        } while (rand == lastHurt);
        lastHurt = rand;
        audio.PlayOneShot(hurtSounds[rand]);
    }

    public void dieSFX()
    {
        int rand;
        do
        {
            rand = UnityEngine.Random.Range(0, dieSounds.Length);
        } while (rand == lastDie);
        lastDie = rand;
        audio.PlayOneShot(dieSounds[rand]);
    }

    void Play()
    {
        if (hardModeToggle.GetComponent<Toggle>().isOn) { Time.timeScale = 1.5f; }
        cheatMode = cheatModeToggle.GetComponent<Toggle>().isOn;
        InvokeRepeating("UpdateTimer", 0, Time.timeScale);
        startMenu.SetActive(false);
        audio.Play();
        player.StartRunning();
        encounter = false;
        StartCoroutine(SpawnEnemy());
    }

    void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void Exit()
    {
        Application.Quit();
    }

    void toggleTutorial()
    {
        if(startView.activeInHierarchy)
        {
            startView.SetActive(false);
            tutorialView.SetActive(true);
            return;
        }
        startView.SetActive(true);
        tutorialView.SetActive(false);
    }

    public void Lose()
    {
        audio.loop = false;
        scoreboard.text = "";
        endTitle.color = Color.red;
        endTitle.rectTransform.localPosition = new Vector2(0, 31);
        endTitle.text = "Game Over!";
        endMenu.SetActive(true);
    }

    public void Win()
    {
        if(cheatMode)
        {
            endTitle.color = new Color(0.8f, 0.67f, 0);
            endTitle.text = "You Win!";
            scoreboard.text = "No high scores in cheat mode though :)";
            endMenu.SetActive(true);
            return;
        }
        int score = (1500 - timeElapsed) + (100 * player.HP);
        if (hardModeToggle.GetComponent<Toggle>().isOn)
        {
            score *= 2;
        }
        endTitle.color = new Color(0.8f,0.67f,0);
        endTitle.text = "You Win!";
        scoreboard.text = "Well done, you completed Level 1!\n"
            + "Completion Time = " + TimeSpan.FromSeconds(timeElapsed).ToString("mm\\:ss") + "\n"
            + "HP Remaining = " + player.HP + "\n"
            + "Total Score = " + score + "!";
        endMenu.SetActive(true);
    }
}
