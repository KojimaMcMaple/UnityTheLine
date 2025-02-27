using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerUpFactory : MonoBehaviour
{
    [SerializeField] private GameObject invi_powerup_;
    [SerializeField] private GameObject size_powerup_;

    public GameObject CreateProduct(GlobalEnums.PowerUpType type)
    {
        GameObject temp = null;
        switch (type)
        {
            case GlobalEnums.PowerUpType.kSize:
                temp = Instantiate(size_powerup_, this.transform);
                break;
            case GlobalEnums.PowerUpType.kInvi:
                temp = Instantiate(invi_powerup_, this.transform);
                break;
            default:
                break;
        }
        temp.SetActive(false);
        return temp;
    }
}
