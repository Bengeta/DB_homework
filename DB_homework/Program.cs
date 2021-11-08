using Dapper;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DB_homework
{
    class Program
    {
        private const string Connection = "User ID=postgres;Password=password;Host=localhost;Port=5432;Database=bd_homework;Pooling=false;Connection Idle Lifetime=10;";
        static void Main(string[] args)
        {
            Console.WriteLine("Hi i am DBW - DateBaseWorker \nPlease enter the command and arguments" +
                "\n1) You can add user (username,name,password)" +
                "\n2) You can add a game with existing tag (cost, nubers of tags) " +
                "\nExisting tags:");
            var tags = GetAllTags();
            for (var i = 0; i < tags.Count; i++)
                Console.WriteLine((i + 1) + ")" + tags[i]);
            while (true)
            {
                var command = Console.ReadLine().Split(' ').ToList();
                var answer = "You made mistake in command. Try again";
                switch (command[0])
                {
                    case "useradd":
                        if (command.Count < 4)
                        {
                            answer = "need more arguments";
                            break;
                        }
                        answer = UserAdd(command) ? "User has been added" : "You make mistake with arguments. Try again.";
                        break;
                    case "gameadd":
                        if (command.Count < 3)
                        {
                            answer = "need more arguments";
                            break;
                        }
                        answer = GameAdd(command) ? "Game has been added" : "You make mistake with arguments. Try again.";
                        break;
                }
                Console.WriteLine(answer);
            }
        }
        public static bool UserAdd(List<string> user_info)
        {
            if (user_info[1].Length > 20)
            {
                Console.WriteLine("username is too long");
                return false;
            }
            if (!int.TryParse(user_info[3], out var password))
            {
                Console.WriteLine("password must consist just of digitals");
                return false;
            }
            using (var connection = new NpgsqlConnection(Connection))
            {
                connection.Open();
                try
                {
                    connection.Execute(
                        "INSERT INTO users(username,name,password) VALUES (@username,@name,@password)",new {username = user_info[1], name = user_info[2], password =password });
                    return true;
                }
                catch (PostgresException e)
                {
                    Console.WriteLine(e.MessageText);
                    return false;
                }
                finally
                {
                    connection.Close();
                }

            }
        }
        public static bool GameAdd(List<string> user_info)
        {
            var game_name = Console.ReadLine();
            if (game_name.Length > 30)
            {
                Console.WriteLine("Name is too long");
                return false;
            }
            if (!int.TryParse(user_info[1], out var cost))
            {
                Console.WriteLine("price must be a number");
                return false;
            }
            var description = Console.ReadLine();
            using (var connection = new NpgsqlConnection(Connection))
            {
                connection.Open();
                try
                {
                    var game_id = connection.ExecuteScalar(
                        $"INSERT INTO game_shop (game,cost,description) VALUES (@game_name,@cost,@description) RETURNING game_id",
                        new { game_name = game_name, cost = cost, description = description });
                    for (var i = 2; i < user_info.Count; i++)
                        if(int.TryParse(user_info[i],out var tag_id))
                            connection.Execute(
                            $"INSERT INTO game_tags (tags_id,game_id) VALUES (@tag_id,@game_id)", new { tag_id = tag_id,game_id = game_id});
                    return true;
                }
                catch (PostgresException e)
                {
                    Console.WriteLine(e.MessageText);
                    return false;
                }
                finally
                {
                    connection.Close();
                }

            }


            return true;
        }
        public static List<string> GetAllTags()
        {
            var tags = new List<string>();
            using (var connection = new NpgsqlConnection(Connection))
            {
                connection.Open();
                tags = connection.Query<String>("Select * From tags").ToList();
                connection.Close();
            };
            return tags;
        }
    }
}
