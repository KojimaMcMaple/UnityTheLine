using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class PowerUpSize : PowerUpSO
{
    public override void ApplyPowerUp(PlayerBall player_ball)
    {
        player_ball.SetSizeMod(powerup_amount_);
        player_ball.SetPowerUpDuration(powerup_duration_);
    }
}
