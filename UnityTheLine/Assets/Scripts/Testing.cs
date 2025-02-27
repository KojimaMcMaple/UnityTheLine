using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Testing : MonoBehaviour
{
    private Grid<int> grid;

    // Start is called before the first frame update
    void Start()
    {
        float cam_dist = (Vector3.zero - Camera.main.transform.position).z;
        float left_border = Camera.main.ViewportToWorldPoint(new Vector3(0, 0, cam_dist)).x;
        float right_border = Camera.main.ViewportToWorldPoint(new Vector3(1, 0, cam_dist)).x;
        float top_border = Camera.main.ViewportToWorldPoint(new Vector3(0, 1, cam_dist)).y;
        float bottom_border = Camera.main.ViewportToWorldPoint(new Vector3(0, 0, cam_dist)).y;

        grid = new Grid<int>(10, 10, .25f, 1f, new Vector3(left_border, bottom_border, 0f), (Grid<int> grid, int x, int y) => 0);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
