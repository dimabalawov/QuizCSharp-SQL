using Dapper;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public static class Results
{
    private static string connectionString = "Server=DMYTROLEGION\\KARAKA;Database=QuizCSharp;Integrated Security=True;TrustServerCertificate=True;";

    public static void GetUserQuizResults(string login)
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

                string getResultsQuery = @"
            SELECT 
                QuizTitle,
                Score,
                TotalQuestions,
                CompletionDate
            FROM QuizResults
            WHERE UserId = @UserId
            ORDER BY CompletionDate DESC";

                var results = conn.Query(getResultsQuery, new { UserId = userId }).ToList();

                if (!results.Any())
                {
                    Console.WriteLine("У пользователя нет результатов викторин.");
                    return;
                }

                Console.WriteLine($"Результаты викторин для пользователя '{login}':");
                foreach (var result in results)
                {
                    Console.WriteLine($"Викторина: {result.QuizTitle}");
                    Console.WriteLine($"Результат: {result.Score}/{result.TotalQuestions}");
                    Console.WriteLine($"Дата завершения: {result.CompletionDate}");
                    Console.WriteLine(new string('-', 30));
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Произошла ошибка: " + ex.Message);
        }
    }

}
