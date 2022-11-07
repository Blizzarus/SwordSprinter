using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class EnemyActions : MonoBehaviour
{
    GameManager gameManager;
    public int HP;
    float speed;
    // Start is called before the first frame update
    void Awake()
    {
        gameManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameManager>();
        speed = gameManager.speed;
    }

    // Update is called once per frame
    void Update()
    {
        if(HP < 1)
        {
            gameManager.EncounterEnd();
            Destroy(gameObject);
        }

        if (gameManager.encounter == false)
        {
            transform.Translate(Vector3.forward * Time.deltaTime * speed);
        }
        else
        {
            //attack player
        }
    }

    private void attackPlayer()
    {

    }
}
