using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridCell
{
    private CellItem cell_item_; //item on cell

    private Grid<GridCell> grid_;
    private int x_;
    private int y_;

    // PATH FINDING
    public int g_cost_;
    public int h_cost_;
    public int f_cost_;
    public bool is_walkable_;
    public GridCell came_from_cell_;

    public GridCell(Grid<GridCell> grid, int x, int y)
    {
        grid_ = grid;
        x_ = x;
        y_ = y;

        is_walkable_ = true;
    }

    public void CalculateFCost() //path finding
    {
        f_cost_ = g_cost_ + h_cost_;
    }

    public CellItem GetCellItem()
    {
        return cell_item_;
    }
    public void SetCellItem(CellItem cell_item)
    {
        cell_item_ = cell_item;
        grid_.DoTriggerGridObjChanged(x_, y_);
    }
    public bool HasCellItem()
    {
        return cell_item_ != null;
    }
    public void ClearCellItem()
    {
        cell_item_ = null;
    }
    public void DestroyCellItem()
    {
        cell_item_?.Destroy();
        grid_.DoTriggerGridObjChanged(x_, y_);
    }

    public Grid<GridCell> GetGrid()
    {
        return grid_;
    }

    public int GetX()
    {
        return x_;
    }
    public int GetY()
    {
        return y_;
    }
    public Vector3 GetWorldPos()
    {
        return grid_.GetWorldPos(x_, y_);
    }

    public void UpdateCellItemWorldPost()
    {
        cell_item_.SetWorldPos(GetWorldPos());
    }
}
