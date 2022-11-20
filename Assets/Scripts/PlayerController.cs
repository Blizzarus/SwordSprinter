using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    GameManager gameManager;
    TextMeshProUGUI playerStatus;
    EnemyActions enemy;
    public int HP;
    public float delayState = 0;
    // Attack States: 0 = Left, 1 = Right, 2 = Up, 3 = Down
    public List<float> atkStates = new List<float>() { 0, 0, 0, 0 };
    char queuedAtk = 'N';
    // Start is called before the first frame update
    void Start()
    {
        playerStatus = GameObject.Find("PlayerStatus").GetComponent<TextMeshProUGUI>();
        gameManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameManager>();
        HP = 99;
    }

    // Update is called once per frame
    void Update()
    {
        // check for user input (TODO: add touch swipe control support)
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if (delayState == 0) { atkStates[0] = 1.0f; }
            else
            {
                queuedAtk = 'L';
            }
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            if (delayState == 0) { atkStates[1] = 1.0f; }
            else
            {
                queuedAtk = 'R';
            }
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            if (delayState == 0) { atkStates[2] = 1.0f; }
            else
            {
                queuedAtk = 'U';
            }
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            if (delayState == 0) { atkStates[3] = 1.0f; }
            else
            {
                queuedAtk = 'D';
            }
        }
        // update Delay and Attack States
        updateStates();
    }

    private void LateUpdate()
    {
        playerStatus.text = "Delay: " + delayState + "\nLeft: " + atkStates[0] + "\nRight: " + atkStates[1];
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Enemy"))
        {
            enemy = other.GetComponent<EnemyActions>();
            gameManager.EncounterStart();
        }
    }

    void updateStates()
    {
        //update delayState
        if (delayState > 0)
        {
            delayState -= Time.deltaTime / 10;
            if (delayState <= 0)
            {
                delayState = 0;
                switch (queuedAtk)
                {
                    case 'L':
                        atkStates[0] = 1.0f;
                        queuedAtk = 'N';
                        break;
                    case 'R':
                        atkStates[1] = 1.0f;
                        queuedAtk = 'N';
                        break;
                    case 'U':
                        atkStates[2] = 1.0f;
                        queuedAtk = 'N';
                        break;
                    case 'D':
                        atkStates[3] = 1.0f;
                        queuedAtk = 'N';
                        break;
                }
            }
        }

        //update attack states
        if (atkStates.Max() > 0)
        {
            int i = atkStates.IndexOf(atkStates.Max());
            atkStates[i] -= Time.deltaTime / 10;
            if (gameManager.encounter)
            {
                if (atkStates[i] <= 0)
                {
                    enemy.takeDamage();
                    atkStates[i] = 0;
                }
                else if (enemy.atkStates[i] != 0 && atkStates[i] + enemy.atkStates[i] > 0.9f && atkStates[i] + enemy.atkStates[i] < 1.0f)
                {
                    enemy.clash(i);
                    clash(i);
                }
            }
        }
    }

    void clash(int i)
    {
        Debug.Log("Clash!");
        delayState = atkStates[i];
        atkStates[0] = atkStates[1] = atkStates[2] = atkStates[3] = 0;
    }

    public void takeDamage()
    {
        HP--;
        if (HP <= 0)
        {
            Debug.Log("Game Over!");
            //Destroy(gameObject);
        }
    }
}
