using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PowerUpSO : ScriptableObject
{
    public GlobalEnums.PowerUpType powerup_type_;
    public float powerup_amount_ = 1.0f;
    public float powerup_duration_ = 8f;

    public abstract void ApplyPowerUp(PlayerBall player_ball);
}
