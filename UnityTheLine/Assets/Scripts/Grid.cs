using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid<T>
{
    public event EventHandler<OnGridObjectChangedEventArgs> OnGridObjectChanged;
    public class OnGridObjectChangedEventArgs : EventArgs
    {
        public int x_;
        public int y_;
    }

    private int width_;
    private int height_;
    private float cell_width_;
    private float cell_height_;
    private Vector3 origin_;
    private T[,] grid_arr_;

    public Grid(int width, int height, float cell_width, float cell_height, Vector3 origin,
        Func<Grid<T>, int, int, T> InitGridObj) //pass in Grid, x, y to init the GridObj of type T
    {
        this.width_ = width;
        this.height_ = height;
        this.cell_width_ = cell_width;
        this.cell_height_ = cell_height;
        this.origin_ = origin;

        grid_arr_ = new T[width, height];

        for (int x = 0; x < grid_arr_.GetLength(0); x++)
        {
            for (int y = 0; y < grid_arr_.GetLength(1); y++)
            {
                grid_arr_[x, y] = InitGridObj(this, x, y);
            }
        }
    }

    public void DoTriggerGridObjChanged(int x, int y)
    {
        OnGridObjectChanged?.Invoke(this, new OnGridObjectChangedEventArgs { x_ = x, y_ = y });
    }

    public void DoScrollGrid(float speed)
    {
        origin_ = new Vector3(origin_.x,
                           origin_.y - (speed * Time.deltaTime));

    }

    public float GetTopMostWordPos() 
    {
        return height_ * cell_height_ + origin_.y;
    }

    public Vector3 GetWorldPos(int x, int y) //GetCellWorldPos
    {
        return new Vector3(x * cell_width_ + origin_.x,
                           y * cell_height_ + origin_.y);
    }

    public Vector2Int GetGridCoords(Vector3 world_pos)
    {
        int x = Mathf.FloorToInt((world_pos.x - origin_.x) / cell_width_);
        int y = Mathf.FloorToInt((world_pos.y - origin_.y) / cell_height_);
        return new Vector2Int(x, y);
    }

    public void SetValue(int x, int y, T value) //SetCellValue
    {
        if (x < 0 || y < 0 || x >= width_ || y >= height_)
        {
            return;
        }
        grid_arr_[x, y] = value;
        if (OnGridObjectChanged != null)
        {
            OnGridObjectChanged(this, new OnGridObjectChangedEventArgs { x_ = x, y_ = y });
        }
    }

    public void SetValue(Vector3 world_pos, T value) //SetCellValue
    {
        Vector2Int coords = GetGridCoords(world_pos);
        SetValue(coords.x, coords.y, value);
    }

    public T GetGridObj(int x, int y)
    {
        if (x < 0 || y < 0 || x >= width_ || y >= height_)
        {
            return default(T); //C# default value of type T
        }
        else
        {
            return grid_arr_[x, y];
        }
    }

    public T GetGridObj(Vector3 world_pos)
    {
        Vector2Int coords = GetGridCoords(world_pos);
        return GetGridObj(coords.x, coords.y);
    }

    public int GetWidth()
    {
        return width_;
    }

    public int GetHeight()
    {
        return height_;
    }

    public float GetCellWidth()
    {
        return cell_width_;
    }

    public float GetCellHeight()
    {
        return cell_height_;
    }

    public void SetOrigin(Vector3 origin)
    {
        origin_ = origin;
    }
}
