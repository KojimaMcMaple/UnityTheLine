using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PathFinding
{
    public static List<GridCell> FindPath(int start_x, int start_y, int end_x, int end_y, Grid<GridCell> grid)
    {
        GridCell start_node = grid.GetGridObj(start_x, start_y);
        GridCell end_node = grid.GetGridObj(end_x, end_y);
        List<GridCell> open_list = new List<GridCell> { start_node };
        List<GridCell> closed_list = new List<GridCell>();

        for (int x = 0; x < grid.GetWidth(); x++)
        {
            for (int y = 0; y < grid.GetHeight(); y++)
            {
                GridCell path_node = grid.GetGridObj(x, y);
                path_node.g_cost_ = int.MaxValue;
                path_node.CalculateFCost();
                path_node.came_from_cell_ = null;
            }
        }

        start_node.g_cost_ = 0;
        start_node.h_cost_ = CalculateHCost(start_node, end_node);

        while (open_list.Count > 0)
        {
            GridCell curr_node = GetLowestFCostNode(open_list);
            if (curr_node == end_node)
            {
                return CalculatePath(end_node);
            }
            open_list.Remove(curr_node);
            closed_list.Add(curr_node);

            foreach (GridCell neighbour in GetNeighbours(curr_node, grid))
            {
                if (closed_list.Contains(neighbour))
                {
                    continue;
                }

                if (!neighbour.is_walkable_)
                {
                    closed_list.Add(neighbour);
                    continue;
                }

                int tentative_g_cost = curr_node.g_cost_ + CalculateHCost(curr_node, neighbour);
                if (tentative_g_cost < neighbour.g_cost_)
                {
                    neighbour.came_from_cell_ = curr_node;
                    neighbour.g_cost_ = tentative_g_cost;
                    neighbour.h_cost_ = CalculateHCost(neighbour, end_node);
                    neighbour.CalculateFCost();
                    if (!open_list.Contains(neighbour))
                    {
                        open_list.Add(neighbour);
                    }
                }
            }
        }

        return null;
    }

    private static int CalculateHCost(GridCell a, GridCell b)
    {
        int x_distance = Mathf.Abs(a.GetX() - b.GetX());
        int y_distance = Mathf.Abs(a.GetY() - b.GetY());
        return Mathf.Min(x_distance, y_distance);
    }

    private static GridCell GetLowestFCostNode(List<GridCell> path_node_list)
    {
        GridCell result = path_node_list[0];
        for (int i = 1; i < path_node_list.Count; i++)
        {
            if (path_node_list[i].f_cost_ < result.f_cost_)
            {
                result = path_node_list[i];
            }
        }
        return result;
    }

    private static List<GridCell> CalculatePath(GridCell end_node)
    {
        List<GridCell> result = new List<GridCell>();
        result.Add(end_node);
        GridCell curr_node = end_node;
        while (curr_node.came_from_cell_ != null)
        {
            result.Add(curr_node.came_from_cell_);
            curr_node = curr_node.came_from_cell_;
        }
        result.Reverse();
        return result;
    }

    private static List<GridCell> GetNeighbours(GridCell path_node, Grid<GridCell> grid)
    {
        List<GridCell> result = new List<GridCell>();
        //left
        if (path_node.GetX() - 1 >= 0)
        {
            result.Add(grid.GetGridObj(path_node.GetX() - 1, path_node.GetY()));
        }
        //right
        if (path_node.GetX() + 1 < grid.GetWidth())
        {
            result.Add(grid.GetGridObj(path_node.GetX() + 1, path_node.GetY()));
        }
        //up
        if (path_node.GetY() - 1 >= 0)
        {
            result.Add(grid.GetGridObj(path_node.GetX(), path_node.GetY() - 1));
        }
        //down
        if (path_node.GetY() + 1 < grid.GetHeight())
        {
            result.Add(grid.GetGridObj(path_node.GetX(), path_node.GetY() + 1));
        }
        return result;
    }
}
