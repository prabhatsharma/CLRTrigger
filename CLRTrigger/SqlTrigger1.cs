using System;
// using System.Data;
using System.Data.SqlClient;
using Microsoft.SqlServer.Server;
using System.Net;

// using Amazon.SimpleNotificationService;
// using Amazon.SimpleNotificationService.Model;

using System.Threading.Tasks;

public partial class Triggers
{
    // Enter existing table or view for the target and uncomment the attribute line
    [Microsoft.SqlServer.Server.SqlTrigger(Name = "SqlTrigger1", Target = "customer", Event = "FOR INSERT, UPDATE, DELETE")]
    public static void SqlTrigger1()
    {
        SqlTriggerContext triggContext = SqlContext.TriggerContext;
        // Replace with your own code
        SqlContext.Pipe.Send("Trigger FIRED");
        SqlCommand command;
        SqlDataReader reader;
        SqlPipe pipe = SqlContext.Pipe;

        String uid;
        String customerName;

        switch (triggContext.TriggerAction)
        {
            case TriggerAction.Insert:
                using (SqlConnection conn = new SqlConnection("context connection=true"))
                {
                    conn.Open();
                    command = new SqlCommand(@"SELECT * FROM INSERTED;", conn);
                    reader = command.ExecuteReader();
                    reader.Read();
                    uid = reader[0].ToString();
                    customerName = (string)reader[1];

                    pipe.Send(@"You updated: '" + uid + @"' - '"
                       + customerName + @"'");

                    reader.Close();

                    SendMessage(uid, customerName, "insert");
                }
                break;
            case TriggerAction.Update:
                using (SqlConnection connection
            = new SqlConnection(@"context connection=true"))
                {
                    connection.Open();
                    command = new SqlCommand(@"SELECT * FROM INSERTED;",
                       connection);
                    reader = command.ExecuteReader();
                    reader.Read();

                    uid = reader[0].ToString();
                    customerName = (string)reader[1];

                    pipe.Send(@"You updated: '" + uid + @"' - '"
                       + customerName + @"'");

                    for (int columnNumber = 0; columnNumber < triggContext.ColumnCount; columnNumber++)
                    {
                        pipe.Send("Updated column "
                           + reader.GetName(columnNumber) + "? "
                           + triggContext.IsUpdatedColumn(columnNumber).ToString());
                    }

                    reader.Close();

                    SendMessage(uid, customerName, "update");
                }
                break;
            case TriggerAction.Delete:
                using (SqlConnection connection
               = new SqlConnection(@"context connection=true"))
                {
                    connection.Open();
                    command = new SqlCommand(@"SELECT * FROM DELETED;",
                       connection);
                    reader = command.ExecuteReader();

                    if (reader.HasRows)
                    {
                        pipe.Send(@"You deleted the following rows:");
                        while (reader.Read())
                        {
                            uid = reader.GetInt32(0).ToString();
                            customerName = reader.GetString(1);
                            pipe.Send(@"'" + reader.GetInt32(0)
                            + @"', '" + reader.GetString(1) + @"'");

                            SendMessage(uid, customerName, "delete");
                        }

                        reader.Close();

                        //alternately, to just send a tabular resultset back:  
                        //pipe.ExecuteAndSend(command);  
                    }
                    else
                    {
                        pipe.Send("No rows affected.");
                    }
                }
                break;
        }
        
    }

    public static string SendMessage(string uid, string customerName, string dmlType)
    {
        string originhostName = Dns.GetHostName();
        string smessage = "dmlType=" + dmlType + "&uid=" + uid + "&customername=" + customerName + "&origin=" + originhostName;

        var req = HttpWebRequest.Create("http://localhost:8080/message?" + smessage);

        req.ContentType = "application/json";
        req.Method = "GET";

        var res = req.GetResponseAsync();

        return "success";
    }

}

