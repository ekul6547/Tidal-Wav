using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PointSystem : MonoBehaviour {

    public int _points;
    public int currentPoints
    {
        get
        {
            return _points;
        }
        set
        {
            if (value >= 0)
                _points = value;
        }
    }

    public void AddPoints(int amount)
    {
        if(amount > 0)
            currentPoints += amount;
    }

    public void DeletePoints(int amount)
    {
        if (amount > 0)
            currentPoints -= amount;
    }

    public bool CanSpendPoints(int amount)
    {
        if (amount >= 0)
        {
            return currentPoints >= amount;
        }
        else
        {
            return false;
        }
    }

    public bool SpendPoints(int amount)
    {
        if (CanSpendPoints(amount))
        {
            DeletePoints(amount);
            return true;
        }
        return false;
    }
}
