using System.Data;
using UnityEngine;

public class ChatConnection : MonoBehaviour
{
    [SerializeField] WebService service;
    [SerializeField] string userName;
    [SerializeField] string email;
    [SerializeField] string password;

    [SerializeField] string urlDataBase;

    IDbCommand command;
    IDbConnection connection;

    void Start()
    {
        OpenDataBase();
        InsertData();
        GetInsertedIDQuery();

        MakeQuery();

        connection.Close();
    }

    void OpenDataBase()
    {
       // connection = new SqliteConnection(urlDataBase);

        Debug.Log(connection == null ? "DB Error" : "Connection Succeded");

        if (connection!= null)
        {
            command = connection.CreateCommand();

            connection.Open();
        }
    }

    [ContextMenu("Insert")]
    public void InsertData()
    {
        string sql = "INSERT INTO DB_User (Name, Email, Password_2) VALUES ('" + userName + "',\"" + email + "\",\"" + password;

        command.CommandText = sql;

        int result = command.ExecuteNonQuery();
        Debug.Log(result == -1 ? "DB Error" : "INSERT Succeded");
    }

    void MakeQuery()
    {
        string sqlQuery = "SELECT Name, Email, Password_2 FROM DB_User";

        command.CommandText = sqlQuery;

        IDataReader reader = command.ExecuteReader();

        while (reader.Read())
        {
            string n = reader.GetString(0);
            string e = reader.GetString(1);
            string p = reader.GetString(2);

            Debug.Log("Name = " + n + "Email = " + e + "Passaword = " + p);
        }
    }

    void GetInsertedIDQuery()
    {
        string sqlQuery = "SELECT MAX(id) AS LastID FROM DB_User";

        command.CommandText = sqlQuery;

        IDataReader reader = command.ExecuteReader();
        bool succeded = reader.Read();

        if (succeded)
        {
            Debug.Log("ID = " + reader.GetInt32(0));
        }
        else
        {
            Debug.Log("INSERTED ID QUERY ERROR");
        }
    }

    public void UpdateUser()
    {
        string sql = "UPDATE DB_User SET Email = '" + email + "' WHERE Email = '" + email + "'";

        command.CommandText = sql;

        int result = command.ExecuteNonQuery();
        Debug.Log(result == -1 ? "DB Error" : "UPDATE Succeded");
    }

    public void DeleteUser()
    {
        string sql = "DELETE FROM DB_User WHERE Email = '" + email + "'";

        command.CommandText = sql;

        int result = command.ExecuteNonQuery();
        Debug.Log(result == -1 ? "DB Error" : "DELETE Succeded");
    }
}
