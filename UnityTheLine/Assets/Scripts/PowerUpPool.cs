using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerUpPool : MonoBehaviour
{
    private Queue<GameObject> invi_powerup_pool_;
    private Queue<GameObject> size_powerup_pool_;
    [SerializeField] private int invi_powerup_num_;
    [SerializeField] private int size_powerup_num_;

    private PowerUpFactory factory_;

    private void Awake()
    {
        invi_powerup_pool_ = new Queue<GameObject>();
        size_powerup_pool_ = new Queue<GameObject>();
        factory_ = GetComponent<PowerUpFactory>();
        BuildPool(); //pre-build a certain num of vfxs to improve performance
    }

    private void BuildPool()
    {
        for (int i = 0; i < invi_powerup_num_; i++)
        {
            PreAddToPool(GlobalEnums.PowerUpType.kInvi);
        }
        for (int i = 0; i < size_powerup_num_; i++)
        {
            PreAddToPool(GlobalEnums.PowerUpType.kSize);
        }
    }

    private void PreAddToPool(GlobalEnums.PowerUpType type)
    {
        //var temp = Instantiate(vfx_obj, this.transform);
        var temp = factory_.CreateProduct(type);

        switch (type)
        {
            case GlobalEnums.PowerUpType.kInvi:
                invi_powerup_pool_.Enqueue(temp);
                break;
            case GlobalEnums.PowerUpType.kSize:
                size_powerup_pool_.Enqueue(temp);
                break;
            default:
                break;
        }
    }

    private void AddToPool(GlobalEnums.PowerUpType type)
    {
        //var temp = Instantiate(vfx_obj, this.transform);
        var temp = factory_.CreateProduct(type);

        switch (type)
        {
            case GlobalEnums.PowerUpType.kInvi:
                //temp.SetActive(false);
                invi_powerup_pool_.Enqueue(temp);
                invi_powerup_num_++;
                break;
            case GlobalEnums.PowerUpType.kSize:
                //temp.SetActive(false);
                size_powerup_pool_.Enqueue(temp);
                size_powerup_num_++;
                break;
            default:
                break;
        }
    }

    public GameObject GetFromPool(Vector3 position, GlobalEnums.PowerUpType type)
    {
        GameObject temp = null;
        switch (type)
        {
            case GlobalEnums.PowerUpType.kInvi:
                if (invi_powerup_pool_.Count < 1) //add one vfx if pool empty
                {
                    AddToPool(GlobalEnums.PowerUpType.kInvi);
                }
                temp = invi_powerup_pool_.Dequeue();
                break;
            case GlobalEnums.PowerUpType.kSize:
                if (size_powerup_pool_.Count < 1) //add one vfx if pool empty
                {
                    AddToPool(GlobalEnums.PowerUpType.kSize);
                }
                temp = size_powerup_pool_.Dequeue();
                break;
            default:
                break;
        }
        temp.transform.position = position;
        temp.SetActive(true);

        return temp;
    }

    public void ReturnToPool(GameObject returned, GlobalEnums.PowerUpType type)
    {
        returned.SetActive(false);

        switch (type)
        {
            case GlobalEnums.PowerUpType.kInvi:
                invi_powerup_pool_.Enqueue(returned);
                break;
            case GlobalEnums.PowerUpType.kSize:
                size_powerup_pool_.Enqueue(returned);
                break;
            default:
                break;
        }
    }
}
