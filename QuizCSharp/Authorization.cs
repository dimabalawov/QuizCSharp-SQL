using System;
using Microsoft.Data.SqlClient;
using Dapper;

public static class Authorization
{
    private static string connectionString = "Server=DMYTROLEGION\\KARAKA;Database=QuizCSharp;Integrated Security=True;TrustServerCertificate=True;";

    public static string Login()
    {
        Console.WriteLine("=== Авторизация ===");
        Console.Write("Введите логин: ");
        string login = Console.ReadLine();

        Console.Write("Введите пароль: ");
        string password = Console.ReadLine();

        using (var connection = new SqlConnection(connectionString))
        {
            connection.Open();
            var user = connection.QueryFirstOrDefault<User>(
                "SELECT * FROM Users WHERE Login = @Login AND Password = @Password",
                new { Login = login, Password = password });

            if (user != null)
            {
                Console.WriteLine($"Добро пожаловать, {user.Login}!");
                return user.Login;
            }
            else
            {
                Console.WriteLine("Неверный логин или пароль. Попробуйте снова.");
                return null;
            }
        }
    }

    public static string Register()
    {
        Console.WriteLine("=== Регистрация ===");
        Console.Write("Введите логин: ");
        string login = Console.ReadLine();

        Console.Write("Введите пароль: ");
        string password = Console.ReadLine();

        Console.Write("Введите дату рождения (в формате ГГГГ-ММ-ДД): ");
        DateTime dateOfBirth;
        while (!DateTime.TryParse(Console.ReadLine(), out dateOfBirth))
        {
            Console.WriteLine("Некорректный формат. Попробуйте снова.");
        }

        using (var connection = new SqlConnection(connectionString))
        {
            connection.Open();
            var existingUser = connection.QueryFirstOrDefault<User>(
                "SELECT * FROM Users WHERE Login = @Login", new { Login = login });

            if (existingUser != null)
            {
                Console.WriteLine("Пользователь с таким логином уже существует. Попробуйте снова.");
                return null;
            }

            connection.Execute(
                "INSERT INTO Users (Login, Password, DateOfBirth) VALUES (@Login, @Password, @DateOfBirth)",
                new { Login = login, Password = password, DateOfBirth = dateOfBirth });

            Console.WriteLine("Регистрация успешна! Теперь вы можете войти.");
            return login;
        }
    }
}

public class User
{
    public string Login { get; set; }
    public string Password { get; set; }
    public DateTime DateOfBirth { get; set; }
}
