using System;

namespace JsonTesting;

internal class Program
{
    public static void Main()
    {
        var jsonFromServer = GetJsonFromServer();
        var deserializedObjectForClient = JsonParser.Deserialize<UserModel>(jsonFromServer);

        deserializedObjectForClient.Login = "New login";
        deserializedObjectForClient.Password = "New password";
        deserializedObjectForClient.SuperCar.Name = "New name";
        deserializedObjectForClient.Cars.Remove(deserializedObjectForClient.Cars.Find(w => w.Id == 2)!);

        var serializedObjectForServer = JsonParser.Convert(deserializedObjectForClient);
        Console.Write(serializedObjectForServer);
        Console.Read();
    }

    private static string GetJsonFromServer() =>
        """
        {
          "Login": "Old login",
          "Password": "Old password",
          "TestArray": [1,2,3],
          "Key": 123,
          "SuperCar": 
          {
            "Id" : 1,
            "Name" : "Super car",
            "Color" : "Super color"
          },
          "Cars": [ 
              {
                   "Id" : 1,
                   "Name" : "First car",
                   "Color" : "Red"
              },
              {
                   "Id" : 2,
                   "Name" : "Second car",
                   "Color" : "Green"
              },
              {
                   "Id" : 3,
                   "Name" : "Third car",
                   "Color" : "Blue"
              }
          ]
        }
        """;
}