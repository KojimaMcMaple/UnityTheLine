using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.VFX;
using UnityEngine.UI;
using static UnityEditor.PlayerSettings;
using UnityEditor;
using static UnityEngine.Rendering.DebugUI;
using UnityEditor.VersionControl;
using static UnityEngine.UI.Image;

public class GameView : MonoBehaviour
{
    private GameManager model_;
    private Dictionary<CellItem, RectVisual> rect_dict_; //linker between model and view
    private State state_;

    [Header("Grid Logic")]
    [SerializeField] private GameObject rect_visual_template_;

    [Header("UI Elements")]
    [SerializeField] private GameObject touch_zone_;
    [SerializeField] private TMP_Text score_txt_;
    [SerializeField] private TMP_Text gameover_score_txt_;
    [SerializeField] private GameObject game_over_panel_;
    [SerializeField] private GameObject pause_panel_;
    [SerializeField] private GameObject hud_panel_;

    private Queue<RectVisual> rect_visual_pool_;
    private int rect_visual_num_;

    private PowerUpPool powerup_pool_;
    private List<PowerUpObj> powerups_;

    private float touch_zone_max_y_;
    private float touch_zone_min_y_;
    private float touch_timer_ = 0f;
    private bool has_started_touch_ = false;
    
    public enum State
    {
        kAvailable,
        kGameOver
    }

    private void Awake()
    {
        game_over_panel_.SetActive(false);

        Init(FindObjectOfType<GameManager>());

        rect_visual_pool_ = new Queue<RectVisual>();

        powerup_pool_ = FindObjectOfType<PowerUpPool>();
        powerups_ = new List<PowerUpObj>();

        Bounds touch_bounds = touch_zone_.GetComponent<SpriteRenderer>().bounds;
        touch_zone_max_y_ = touch_bounds.max.y;
        touch_zone_min_y_ = touch_bounds.min.y;
    }

    private void Start()
    {
        state_ = State.kAvailable;
        
        model_.OnScoreChanged += HandleScoreChangedEvent;
    }

    private void Update()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            Vector3 touch_pos = Camera.main.ScreenToWorldPoint(touch.position);
            switch (touch.phase)
            {
                case TouchPhase.Began:
                    has_started_touch_ = true;
                    break;

                case TouchPhase.Moved:
                    if (touch_pos.y >= touch_zone_min_y_ && touch_pos.y <= touch_zone_max_y_)
                    {
                        model_.GetPlayerBall().transform.position = new Vector3(touch_pos.x, model_.GetPlayerBall().transform.position.y, 0.0f);
                    }
                    break;

                case TouchPhase.Ended:
                    if (touch_timer_ < 0.4f)
                    {
                        model_.GetPlayerBall().DoJump();
                    }
                    has_started_touch_ = false;
                    touch_timer_ = 0f;
                    break;
            }
            if (has_started_touch_)
            {
                touch_timer_ += Time.deltaTime;
            }
        }
        DoUpdateView();
    }

    public void Init(GameManager model)
    {
        model_ = model;

        //cam_.position = new Vector3(    (grid_.GetWidth() * grid_.GetCellWidth() * .5f) - (grid_.GetCellWidth()/2), 
        //                                0.0f, 
        //                                cam_.position.z);

        model_.OnGridPopulated += HandleGridRepopulatedEvent;
        model_.OnGridFlushed += HandleGridFlushedEvent;
        model_.OnGridCellDestroyed += HandleGridCellDestroyedEvent;
        model_.OnWin += HandleWinEvent;
        model_.OnLoss += HandleLossEvent;

        rect_dict_ = new Dictionary<CellItem, RectVisual>();
    }

    private void AddRectVisualToPool()
    {
        GameObject scene_rect = Instantiate(rect_visual_template_, this.transform);
        scene_rect.SetActive(false);
        RectVisual rect_visual = new RectVisual(scene_rect, null);
        rect_visual_pool_.Enqueue(rect_visual);
        rect_visual_num_++;
    }

    public RectVisual GetRectVisualFromPool(Vector3 pos, Vector3 scale, CellItem rect)
    {
        if (rect_visual_pool_.Count < 1) //add one rect if pool empty
        {
            AddRectVisualToPool();
        }
        RectVisual temp = rect_visual_pool_.Dequeue();
        temp.SetUpCellItemRef(rect);
        temp.GetObjRef().transform.position = pos;
        temp.GetObjRef().transform.localScale = scale;
        temp.GetObjRef().GetComponent<SpriteRenderer>().color = rect.GetRectSO().prefab_.GetComponent<SpriteRenderer>().color;
        temp.GetObjRef().GetComponent<SpriteRenderer>().enabled = rect.GetRectSO().prefab_.GetComponent<SpriteRenderer>().enabled;
        temp.GetObjRef().GetComponent<BoxCollider2D>().enabled = rect.GetRectSO().prefab_.GetComponent<BoxCollider2D>().enabled;

        rect_dict_[rect] = temp;

        temp.GetObjRef().SetActive(true);
        return temp;
    }

    public void ReturnRectVisualToPool(RectVisual returned)
    {
        returned.SetUpCellItemRef(null);
        returned.GetObjRef().SetActive(false);

        rect_visual_pool_.Enqueue(returned);
    }

    private void BuildRectVisualForGridIdx(int idx)
    {
        for (int y = 0; y < model_.GetGridAtIdx(idx).GetHeight(); y++)
        {
            for (int x = 0; x < model_.GetGridAtIdx(idx).GetWidth(); x++)
            {
                GridCell cell = model_.GetGridAtIdx(idx).GetGridObj(x, y);
                CellItem rect = cell.GetCellItem();
                GetRectVisualFromPool(model_.GetGridAtIdx(idx).GetWorldPos(x, y),
                                            new Vector3(model_.GetGridAtIdx(idx).GetCellWidth(), model_.GetGridAtIdx(idx).GetCellHeight(), 1f),
                                            rect);
            }
        }
    }

    //private Transform CreateRectVisualAtWorldPos(Vector3 pos, Vector3 scale, CellItem rect)
    //{
    //    GameObject scene_rect = Instantiate(rect_visual_template_, pos, Quaternion.identity);
    //    scene_rect.transform.localScale = scale;
    //    scene_rect.GetComponent<SpriteRenderer>().color = rect.GetRectSO().prefab_.GetComponent<SpriteRenderer>().color;
    //    scene_rect.GetComponent<SpriteRenderer>().enabled = rect.GetRectSO().prefab_.GetComponent<SpriteRenderer>().enabled;
    //    scene_rect.GetComponent<BoxCollider2D>().enabled = rect.GetRectSO().prefab_.GetComponent<BoxCollider2D>().enabled;

    //    RectVisual rect_visual = new RectVisual(scene_rect, rect);

    //    rect_dict_[rect] = rect_visual;

    //    return scene_rect.transform;
    //}

    private void DoUpdateView()
    {
        foreach (CellItem rect in rect_dict_.Keys)
        {
            rect_dict_[rect].UpdateWorldPos(rect.GetWorldPos());
        }

        foreach (PowerUpObj powerup in powerups_)
        {
            powerup.transform.position = new Vector3(powerup.transform.position.x,
                                                    powerup.transform.position.y - (model_.GetScrollSpeed() * Time.deltaTime));
        }
    }

    private void DoWin()
    {
        game_over_panel_.SetActive(true);
    }

    public void DoQuit()
    {
        Application.Quit();
    }

    public void DoReloadLevel()
    {
        SceneManager.LoadScene("TheLine");
        Time.timeScale = 1.0f;
    }

    public void DoResumeGame()
    {
        pause_panel_.SetActive(false);
        hud_panel_.SetActive(true);
        Time.timeScale = 1.0f;
    }

    public void DoOpenPauseMenu()
    {
        hud_panel_.SetActive(false);
        pause_panel_.SetActive(true);
        Time.timeScale = 0.0f;
    }

    private void HandleGridRepopulatedEvent(object sender, GameManager.GridIdxEventArgs e)
    {
        BuildRectVisualForGridIdx(e.grid_idx);
        if (e.powerup_pos != Vector3.zero)
        {
            GameObject temp = powerup_pool_.GetFromPool(e.powerup_pos, GlobalEnums.PowerUpType.kSize);
            PowerUpObj powerup_obj = temp.GetComponent<PowerUpObj>();
            powerups_.Add(powerup_obj);
            powerup_obj.OnCollected += HandlePowerUpCollectedEvent;
        }
    }

    private void HandleGridFlushedEvent(object sender, GameManager.GridIdxEventArgs e)
    {
        for (int y = 0; y < model_.GetGridAtIdx(e.grid_idx).GetHeight(); y++)
        {
            for (int x = 0; x < model_.GetGridAtIdx(e.grid_idx).GetWidth(); x++)
            {
                GridCell cell = model_.GetGridAtIdx(e.grid_idx).GetGridObj(x, y);
                CellItem rect = cell.GetCellItem();
                ReturnRectVisualToPool(rect_dict_[rect]);
            }
        }
    }

    private void HandlePowerUpCollectedEvent(object sender, System.EventArgs e)
    {
        PowerUpObj powerup_obj = sender as PowerUpObj;
        if (powerup_obj != null)
        {
            powerup_obj.OnCollected -= HandlePowerUpCollectedEvent;
            powerups_.Remove(powerup_obj);
            powerup_pool_.ReturnToPool(powerup_obj.gameObject, powerup_obj.power_up_so_.powerup_type_);
        }
    }

    private void HandleGridCellDestroyedEvent(object sender, System.EventArgs e)
    {
        GridCell cell = sender as GridCell;
        if (cell != null && cell.GetCellItem() != null)
        {
            rect_dict_.Remove(cell.GetCellItem());
        }
    }

    private void HandleScoreChangedEvent(object sender, System.EventArgs e)
    {
        score_txt_.text = "SCORE\n" + Mathf.FloorToInt(model_.GetScore()).ToString();
    }

    private void HandleWinEvent(object sender, System.EventArgs e)
    {
        DoWin();
        Time.timeScale = 0.0f;
    }

    private void HandleLossEvent(object sender, System.EventArgs e)
    {
        hud_panel_.SetActive(false);
        pause_panel_.SetActive(false);
        gameover_score_txt_.text = "SCORE\n" + Mathf.FloorToInt(model_.GetScore()).ToString();
        game_over_panel_.SetActive(true);
        Time.timeScale = 0.0f;
    }
}
