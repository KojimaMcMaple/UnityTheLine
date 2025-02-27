using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public event EventHandler<GridIdxEventArgs> OnGridPopulated; //event to send command from model to view
    public event EventHandler<GridIdxEventArgs> OnGridFlushed; //event to send command from model to view
    public event EventHandler OnGridCellDestroyed; //event to send command from model to view
    public event EventHandler OnScoreChanged;
    public event EventHandler OnWin;
    public event EventHandler OnLoss;

    public class GridIdxEventArgs : EventArgs
    {
        public int grid_idx;
        public Vector3 powerup_pos;
    }

    private List<Grid<GridCell>> grids_;
    private int curr_grid_idx_ = 0;
    private int next_grid_idx_ = 1;
    private int queued_grid_idx_ = 2;
    private int grid_count_ = 3;

    [Header("Grid Logic")]
    [SerializeField]
    private int width_ = 5;
    [SerializeField]
    private int height_ = 11;
    private float cell_width_;
    [SerializeField]
    private float cell_height_ = 1.75f;
    [SerializeField]
    private int complexity_ = 5;
    [SerializeField] 
    private List<RectSO> rect_so_list_;

    [SerializeField]
    private float scroll_speed_ = 2.0f;

    private float cam_dist_;
    private float left_border_;
    private float right_border_;
    private float top_border_;
    private float bottom_border_;



    private PlayerBall player_ball_;

    private float score_;

    private State state_;
    public enum State
    {
        kAvailable,
        kGameOver
    }

    private void Awake()
    {
        cam_dist_ = (Vector3.zero - Camera.main.transform.position).z;
        left_border_ = Camera.main.ViewportToWorldPoint(new Vector3(0, 0, cam_dist_)).x;
        right_border_ = Camera.main.ViewportToWorldPoint(new Vector3(1, 0, cam_dist_)).x;
        top_border_ = Camera.main.ViewportToWorldPoint(new Vector3(0, 1, cam_dist_)).y;
        bottom_border_ = Camera.main.ViewportToWorldPoint(new Vector3(0, 0, cam_dist_)).y;

        cell_width_ = ((right_border_ - left_border_)) / width_;
        InitGrids();

        score_ = 0;

        player_ball_ = GameObject.FindWithTag("Player").GetComponent<PlayerBall>();
        player_ball_.OnHit += HandleOnHitEvent;

        player_ball_.transform.position = new Vector3(0f, bottom_border_ * 0.6f);
    }

    private void Start()
    {
        state_ = State.kAvailable;
    }

    void Update()
    {
        switch (state_)
        {
            case State.kAvailable:
                score_ += Time.deltaTime * 2.0f;
                OnScoreChanged?.Invoke(this, EventArgs.Empty);

                foreach (Grid<GridCell> grid in grids_)
                {
                    grid.DoScrollGrid(scroll_speed_);
                    for (int y = 0; y < grid.GetHeight(); y++)
                    {
                        for (int x = 0; x < grid.GetWidth(); x++)
                        {
                            grid.GetGridObj(x, y).UpdateCellItemWorldPost();
                        }
                    }
                    if (grid.GetTopMostWordPos() < bottom_border_)
                    {
                        curr_grid_idx_ = next_grid_idx_;
                        next_grid_idx_ = queued_grid_idx_;
                        queued_grid_idx_ = (queued_grid_idx_ == grid_count_ - 1) ? 0 : queued_grid_idx_ + 1;
                        grids_[queued_grid_idx_].SetOrigin(new Vector3(left_border_ + cell_width_ * .5f,
                                                                        grids_[next_grid_idx_].GetTopMostWordPos()));

                        OnGridFlushed?.Invoke(grid, new GridIdxEventArgs
                        {
                            grid_idx = grids_.IndexOf(grid)
                        });

                        PopulateGrid(grids_[queued_grid_idx_]);
                    }
                }
                break;

            case State.kGameOver:
                break;
        }
    }

    private void InitGrids()
    {
        grids_ = new List<Grid<GridCell>>();
        for (int i = 0; i < grid_count_; i++)
        {
            float offset = i * height_ * cell_height_;

            Grid<GridCell> temp_grid = new Grid<GridCell>(width_, height_,
            cell_width_,
            cell_height_,
            new Vector3(left_border_ + cell_width_ * .5f, 
                        bottom_border_ + cell_height_ * .5f + offset),
            (Grid<GridCell> grid_, int x, int y) => new GridCell(grid_, x, y));

            for (int y = 0; y < height_; y++)
            {
                for (int x = 0; x < width_; x++)
                {
                    CellItem rect = new CellItem(rect_so_list_[0], x, y, temp_grid.GetGridObj(x, y).GetWorldPos());
                    temp_grid.GetGridObj(x, y).SetCellItem(rect);
                }
            }
            grids_.Add(temp_grid);
        }

        PopulateStartingGrid(grids_[0]);
        for (int i = 1; i < grid_count_; i++)
        {
            PopulateGrid(grids_[i]);
        }
    }

    private void PopulateStartingGrid(Grid<GridCell> grid)
    {
        int half_height = Mathf.FloorToInt(grid.GetHeight() * .5f);
        // build starting rows
        for (int y = 0; y < half_height; y++)
        {
            for (int x = 0; x < grid.GetWidth(); x++)
            {
                grid.GetGridObj(x, y).GetCellItem().UpdateCellItem(rect_so_list_[0], grid.GetGridObj(x, y).GetWorldPos());
            }
        }
        // build end row
        for (int x = 0; x < width_; x++)
        {
            grid.GetGridObj(x, height_ - 1).GetCellItem().UpdateCellItem(rect_so_list_[2], grid.GetGridObj(x, height_ - 1).GetWorldPos());
        }
        // build path
        List<GridCell> path = new List<GridCell>();
        path = PathFinding.FindPath(    UnityEngine.Random.Range(0, width_), half_height,
                                        UnityEngine.Random.Range(0, width_), height_ - 1,
                                        grid);
        for (int y = half_height; y < height_ - 1; y++)
        {
            for (int x = 0; x < width_; x++)
            {
                int idx = 1;
                if (path.Contains(grid.GetGridObj(x, y)))
                {
                    idx = 0;
                }
                grid.GetGridObj(x, y).GetCellItem().UpdateCellItem(rect_so_list_[idx], grid.GetGridObj(x, y).GetWorldPos());
            }
        }

        OnGridPopulated?.Invoke(grid, new GridIdxEventArgs
        {
            grid_idx = grids_.IndexOf(grid)
        });
    }

    private void PopulateGrid(Grid<GridCell> grid)
    {
        for (int y = 0; y < grid.GetHeight(); y++)
        {
            for (int x = 0; x < grid.GetWidth(); x++)
            {
                grid.GetGridObj(x, y).is_walkable_ = true;
            }
        }
        List<GridCell> path = new List<GridCell>();
        do
        {
            List<GridCell> non_walkable_cells = new List<GridCell>();
            for (int c = 0; c <= complexity_; c++)
            {
                GridCell temp = grid.GetGridObj(UnityEngine.Random.Range(0, width_),
                                                UnityEngine.Random.Range(2, height_ - 1)); //avoid top and bottom row
                temp.is_walkable_ = false;
                non_walkable_cells.Add(temp);
            }
            path = PathFinding.FindPath(UnityEngine.Random.Range(0, width_), 2,
                                        UnityEngine.Random.Range(0, width_), height_ - 1,
                                        grid);
            if (path == null)
            {
                foreach (GridCell cell in non_walkable_cells)
                {
                    cell.is_walkable_ = true;
                }
                complexity_--;
            }
        }
        while (path == null);

        // build starting rows
        for (int y = 0; y < 2; y++)
        {
            for (int x = 0; x < width_; x++)
            {
                grid.GetGridObj(x, y).GetCellItem().UpdateCellItem(rect_so_list_[0], grid.GetGridObj(x, y).GetWorldPos());
            }
        }
        // build end row
        for (int x = 0; x < width_; x++)
        {
            grid.GetGridObj(x, height_ - 1).GetCellItem().UpdateCellItem(rect_so_list_[2], grid.GetGridObj(x, height_ - 1).GetWorldPos());
        }
        // build path
        for (int y = 2; y < height_-1; y++)
        {
            for (int x = 0; x < width_; x++)
            {
                int idx = 1;
                if (path.Contains(grid.GetGridObj(x, y)))
                {
                    idx = 0;
                }
                grid.GetGridObj(x, y).GetCellItem().UpdateCellItem(rect_so_list_[idx], grid.GetGridObj(x, y).GetWorldPos());
            }
        }

        // generate powerups
        int powerup_chance = 1;// UnityEngine.Random.Range(0, 3);
        Vector3 powerup_pos = Vector3.zero;
        if (powerup_chance == 1)
        {
            int powerup_idx = UnityEngine.Random.Range(0, path.Count());
            powerup_pos = path[powerup_idx].GetWorldPos();
        }

        OnGridPopulated?.Invoke(grid, new GridIdxEventArgs
        {
            grid_idx = grids_.IndexOf(grid),
            powerup_pos = powerup_pos
        });
    }

    public float GetScrollSpeed()
    {
        return scroll_speed_;
    }

    public Grid<GridCell> GetGridAtIdx(int idx)
    {
        return grids_[idx];
    }

    public int GetGridCount()
    {
        return grid_count_;
    }

    public float GetScore()
    {
        return score_;
    }

    public PlayerBall GetPlayerBall()
    {
        return player_ball_;
    }

    private bool IsValidCoords(int x, int y)
    {
        if (x < 0 || y < 0 || x >= width_ || y >= height_)
        {
            return false;
        }
        return true;
    }

    //private RectSO GetRectSOAtCoords(int x, int y)
    //{
    //    if (!IsValidCoords(x, y))
    //    {
    //        return null;
    //    }
    //    GridCell cell = grid_.GetGridObj(x, y);
    //    return cell.GetCellItem().GetRectSO();
    //}

    private void TryDestroyCellItem(GridCell cell)
    {
        if (cell.HasCellItem())
        {
            cell.DestroyCellItem();
            OnGridCellDestroyed?.Invoke(cell, EventArgs.Empty);
            cell.ClearCellItem();

            score_ += 100;
        }
    }

    private void HandleOnHitEvent(object sender, System.EventArgs e)
    {
        state_ = State.kGameOver;
        OnLoss?.Invoke(this, EventArgs.Empty);
    }
}
