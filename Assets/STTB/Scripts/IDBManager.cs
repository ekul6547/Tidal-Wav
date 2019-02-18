using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDBManager
{
    void LoadDB();
    void Create();
    void Close();

    void AddNewItem(string itemName, int itemPrice, bool isOwned);
    ArrayList GetItem(string itemName);
}  