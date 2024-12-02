using Dapper;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public static class ChangeInfo
{
    private static string connectionString = "Server=DMYTROLEGION\\KARAKA;Database=QuizCSharp;Integrated Security=True;TrustServerCertificate=True;";

    public static void UpdateUserInfo(string login)
    {
        try
        {
            using (var conn = new SqlConnection(connectionString))
            {
                string getUserIdQuery = "SELECT Id FROM Users WHERE Login = @Login";
                int userId = conn.QuerySingleOrDefault<int>(getUserIdQuery, new { Login = login });

                if (userId == 0)
                {
                    Console.WriteLine("Ошибка: пользователь с таким логином не найден.");
                    return;
                }
                Console.WriteLine("Что вы хотите изменить?");
                Console.WriteLine("1. Пароль");
                Console.WriteLine("2. Дата рождения");
                Console.Write("Введите номер опции: ");
                string choice = Console.ReadLine()?.Trim();

                if (choice == "1") 
                {
                    Console.Write("Введите новый пароль: ");
                    string newPassword = Console.ReadLine()?.Trim();

                    if (string.IsNullOrEmpty(newPassword))
                    {
                        Console.WriteLine("Ошибка: пароль не может быть пустым.");
                        return;
                    }

                    string updateQuery = "UPDATE Users SET Password = @NewPassword WHERE Id = @UserId";
                    int rowsAffected = conn.Execute(updateQuery, new { NewPassword = newPassword, UserId = userId });

                    Console.WriteLine(rowsAffected > 0 ? "Пароль успешно обновлен." : "Обновление пароля не удалось.");
                }
                else if (choice == "2")
                {
                    Console.Write("Введите новую дату рождения (в формате ГГГГ-ММ-ДД): ");
                    string inputDate = Console.ReadLine()?.Trim();

                    if (!DateTime.TryParse(inputDate, out DateTime newDateOfBirth))
                    {
                        Console.WriteLine("Ошибка: некорректная дата.");
                        return;
                    }

                    string updateQuery = "UPDATE Users SET DateOfBirth = @NewDateOfBirth WHERE Id = @UserId";
                    int rowsAffected = conn.Execute(updateQuery, new { NewDateOfBirth = newDateOfBirth, UserId = userId });

                    Console.WriteLine(rowsAffected > 0 ? "Дата рождения успешно обновлена." : "Обновление даты рождения не удалось.");
                }
                else
                {
                    Console.WriteLine("Ошибка: некорректный выбор.");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Произошла ошибка: " + ex.Message);
        }
    }


}
