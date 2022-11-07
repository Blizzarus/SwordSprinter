using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    GameManager gameManager;
    public int HP;
    // Start is called before the first frame update
    void Start()
    {
        gameManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameManager>();
    }

    // Update is called once per frame
    void Update()
    {
        if (HP < 1)
        {
            Destroy(gameObject);
            Debug.Log("Game Over");
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            //attack enemy
            EnemyActions en = GameObject.FindGameObjectWithTag("Enemy").GetComponent<EnemyActions>();
            en.HP -= 1;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("ye");
        if (other.gameObject.CompareTag("Enemy"))
        {
            Debug.Log("yo");
            gameManager.EncounterStart();
        }
    }
}
