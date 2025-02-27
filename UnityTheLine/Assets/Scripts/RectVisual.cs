using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class RectVisual : MonoBehaviour
{
    private GameObject obj_ref_;
    private CellItem rect_;

    public RectVisual(GameObject t, CellItem rect)
    {
        obj_ref_ = t;
        rect_ = rect;
    }

    public void UpdateWorldPos(Vector3 pos)
    {
        obj_ref_.transform.position = pos;
    }

    public GameObject GetObjRef()
    {
        return obj_ref_;
    }

    public void SetUpCellItemRef(CellItem rect)
    {
        rect_ = rect;
    }

    
}