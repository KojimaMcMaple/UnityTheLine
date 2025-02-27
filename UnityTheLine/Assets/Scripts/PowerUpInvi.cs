using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class PowerUpInvi : PowerUpSO
{
    public override void ApplyPowerUp(PlayerBall player_ball)
    {
        player_ball.SetInviMode(true);
        player_ball.SetPowerUpDuration(powerup_duration_);
    }
}
