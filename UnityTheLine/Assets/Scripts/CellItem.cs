using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CellItem
{
    public event EventHandler OnDestroyed;

    private RectSO rect_;
    private int x_;
    private int y_;
    private Vector3 world_pos_;

    public CellItem(RectSO rect, int x, int y, Vector3 world_pos)
    {
        rect_ = rect;
        x_ = x;
        y_ = y;
        world_pos_ = world_pos;
    }

    public void UpdateCellItem(RectSO rect, Vector3 world_pos)
    {
        rect_ = rect;
        world_pos_ = world_pos;
    }

    public RectSO GetRectSO()
    {
        return rect_;
    }

    public int GetCoordsX()
    {
        return x_;
    }

    public int GetCoordsY()
    {
        return y_;
    }

    public void SetCoords(int x, int y)
    {
        x_ = x;
        y_ = y;
    }

    public Vector3 GetWorldPos()
    {
        return world_pos_;
    }

    public void SetWorldPos(Vector3 pos)
    {
        world_pos_ = pos;
    }

    public void Destroy()
    {
        OnDestroyed?.Invoke(this, EventArgs.Empty);
    }

}
