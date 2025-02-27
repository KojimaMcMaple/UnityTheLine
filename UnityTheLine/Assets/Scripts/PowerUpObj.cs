using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameManager;

public class PowerUpObj : MonoBehaviour
{
    public event EventHandler OnCollected;

    public PowerUpSO power_up_so_;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("PowerUp Collected");
            power_up_so_.ApplyPowerUp(collision.GetComponent<PlayerBall>());
            //Destroy(gameObject);
            OnCollected?.Invoke(this, EventArgs.Empty);
        }
    }
}
