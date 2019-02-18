using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mono.Data.Sqlite;

public class DBManager : MonoBehaviour, IDBManager {
    
    static string conn;
    SqliteConnection data;

    public void Awake()
    {
        conn = "URI=file:" + Application.dataPath + "/Data.db";
    }
    public void Start()
    {
        LoadDB();
    }
    public void LoadDB()
    {
        data = new SqliteConnection(conn);
        data.Open();
        Create();
    }
    public void Create()
    {
        SqliteCommand command = data.CreateCommand();
        command.CommandText = "CREATE TABLE IF NOT EXISTS items (id INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,name TEXT UNIQUE,price INTEGER,owned INTEGER)";
        command.ExecuteNonQuery();

        command.Dispose();
    }
    public void Close()
    {
        data.Close();
    }

    public void AddNewItem(string itemName, int itemPrice, bool isOwned)
    {
        SqliteCommand command = data.CreateCommand();
        command.CommandText = string.Format("INSERT INTO strings VALUES (NULL,'{0}',{1},{2})", itemName, itemPrice, isOwned ? 1 : 0);
        command.ExecuteNonQuery();

        command.Dispose();
    }

    public ArrayList GetItem(string itemName)
    {
        ArrayList ret = new ArrayList();
        SqliteCommand cmd = data.CreateCommand();
        cmd.CommandText = string.Format("SELECT * FROM items WHERE name='{0}'", itemName);
        SqliteDataReader reader = cmd.ExecuteReader();
        if (reader.GetValue(0) == DBNull.Value)
            return null;
        while (reader.Read())
        {
            ret.Add(new object[] { reader.GetInt32(0), reader.GetString(1), reader.GetInt32(2), reader.GetInt32(3) == 1 });
        }
        cmd.Dispose();
        return ret;
    }

    public void AllowItem(string itemName)
    {

    }
}
