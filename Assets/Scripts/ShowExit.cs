using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowExit : MonoBehaviour
{
    private float distanceToPlayer;
    private Player player;
    private bool hidden = true;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
    }

    void Update()
    {
        if (hidden)
        {
            distanceToPlayer = Vector2.Distance(transform.position, player.transform.position);
            if (distanceToPlayer < 3)
            {
                hidden = false;
                gameObject.transform.GetChild(0).gameObject.SetActive(true);
            }
        }
    }
}
