using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerBall : MonoBehaviour
{
    public event EventHandler OnHit;

    private bool is_invi_mode_ = false;
    private bool is_jumping_ = false;
    private float size_mod_ = 1.0f;
    private Vector3 original_scale_;
    private Vector3 curr_scale_;
    private float powerup_duration_ = 8f;
    private float powerup_timer_ = 0f;

    void Start()
    {
        curr_scale_ = original_scale_ = this.transform.localScale;
    }

    void Update()
    {
        if (powerup_timer_ > 0)
        {
            powerup_timer_ -= Time.deltaTime;
            if (powerup_timer_ <= 0)
            {
                powerup_timer_ = 0;
                is_invi_mode_ = false;
                size_mod_ = 1.0f;
                this.transform.localScale = original_scale_;
                curr_scale_ = original_scale_;
            }
        }
    }

    private IEnumerator Jumping()
    {
        is_jumping_ = true;
        this.GetComponent<Collider2D>().enabled = false;
        for (int i = 0; i <= 6; ++i)
        {
            this.transform.localScale += 0.1f * Vector3.one;
            yield return new WaitForSeconds(.1f);
        }
        for (int i = 0; i <= 6; ++i)
        {
            this.transform.localScale -= 0.1f * Vector3.one;
            yield return new WaitForSeconds(.1f);
        }
        is_jumping_ = false;
        this.GetComponent<Collider2D>().enabled = true;
    }

    public void DoJump()
    {
        if (is_jumping_)
        {
            return;
        }
        StartCoroutine(Jumping());
    }

    public void SetPowerUpDuration(float powerup_duration) 
    {   
        powerup_duration_ = powerup_duration;
        powerup_timer_ = powerup_duration_;
    }

    public void SetInviMode(bool is_invi_mode) 
    { 
        is_invi_mode_ = is_invi_mode; 
    }

    public void SetSizeMod(float size_mod) 
    { 
        size_mod_ = size_mod; 
        this.transform.localScale = original_scale_ * size_mod_;
        curr_scale_ = this.transform.localScale;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Brick"))
        {
            OnHit?.Invoke(this, EventArgs.Empty);
        }
    }
}
