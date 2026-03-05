using UnityEngine;

public class MatManager : MonoBehaviour
{
    public static MatManager instance;
    int fuel, log, scrap;
    public enum matType
    {
        fuel,
        log,
        scrap
    }


    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
            Destroy(this);
    }
    public void GetMat(matType type, int quantity)
    {
        switch (type)
        {
            case matType.fuel:
                fuel += quantity;
                break;
            case matType.log:
                log += quantity;
                break;
            case matType.scrap:
                scrap += quantity;
                break;
        }
    }
    public bool UseMAt(matType type, int quantity)
    {
        switch (type)
        {
            case matType.fuel:
                if (fuel >= quantity)
                {
                    fuel -= quantity;
                    return true;
                }
                else
                    return false;
            case matType.log:
                if (log >= quantity)
                {
                    log -= quantity;
                    return true;
                }
                else
                    return false;
            case matType.scrap:
                if (scrap >= quantity)
                {
                    scrap -= quantity;
                    return true;
                }
                else
                    return false;
        }
        return false;
    }
}
