using UnityEngine;

public class MatManager : MonoBehaviour
{
    int fuel, log, scrap;
    public void GetMat(int id, int quantity)
    {
        switch (id)
        {
            case 0:
                fuel += quantity;
                break;
            case 1:
                log += quantity;
                break;
            case 2:
                scrap += quantity;
                break;
        }
    }
    public void UseMAt(int id, int quantity)
    {
        switch (id)
        {
            case 0:
                fuel -= quantity;
                break;
            case 1:
                log -= quantity;
                break;
            case 2:
                scrap -= quantity;
                break;
        }
    }
}
