using System;
using System.Collections.Generic;
using Dapper;
using Microsoft.Data.SqlClient;

public static class QuizApp
{
    private static string connectionString = "Server=DMYTROLEGION\\KARAKA;Database=QuizCSharp;Integrated Security=True;TrustServerCertificate=True;";

    public static void Start(string login)
    {
        try
        {
            Console.WriteLine("Выберите викторину:");
            var quizzes = GetQuizzes();

            if (quizzes.Count == 0)
            {
                Console.WriteLine("Викторины отсутствуют.");
                return;
            }

            for (int i = 0; i < quizzes.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {quizzes[i].Title} - {quizzes[i].Description}");
            }
            Console.WriteLine($"{quizzes.Count + 1}. Смешанная - Случайные вопросы из всех викторин");

            Console.Write("Ваш выбор: ");
            if (!int.TryParse(Console.ReadLine(), out int choice) || choice < 1 || choice > quizzes.Count+1)
            {
                Console.WriteLine("Некорректный выбор.");
                return;
            }

            if(choice == 1) RunQuizByTitle("География",login);
            if(choice == 2) RunQuizByTitle("История", login);
            if(choice == 3) RunQuizByTitle("Фильмы", login);
            if(choice == 4) RunRandomQuiz(login);


        }
        catch (Exception ex)
        {
            Console.WriteLine("Произошла ошибка: " + ex.Message);
        }
    }

    private static List<Quiz> GetQuizzes()
    {
        try
        {
            Console.WriteLine("Получение списка викторин...");
            using (var conn = new SqlConnection(connectionString))
            {
                string query = "SELECT Id, Title, Description FROM Quizzes";
                var quizzes = conn.Query<Quiz>(query).AsList();

                Console.WriteLine($"Найдено викторин: {quizzes.Count}");
                return quizzes;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Ошибка при получении викторин: " + ex.Message);
            return new List<Quiz>();
        }
    }
    private static void RunQuizByTitle(string quizTitle, string login)
    {
        try
        {
            using (var conn = new SqlConnection(connectionString))
            {
                Console.WriteLine($"Начало викторины: {quizTitle}");

                // SQL-запрос для получения вопросов и ответов
                string query = @"
            SELECT 
                q.Id AS QuestionId,
                q.Text AS QuestionText,
                a.Id AS AnswerId,
                a.Text AS AnswerText,
                a.IsCorrect
            FROM Questions q
            JOIN Answers a ON q.Id = a.QuestionId
            JOIN Quizzes z ON q.QuizId = z.Id
            WHERE z.Title = @QuizTitle
            ORDER BY q.Id, a.Id";

                // Выполняем запрос и получаем список вопросов
                var questions = conn.Query<QuestionWithAnswers>(query, new { QuizTitle = quizTitle }).GroupBy(q => new { q.QuestionId, q.QuestionText }).ToList();

                if (questions.Count == 0)
                {
                    Console.WriteLine("Вопросы для этой викторины отсутствуют.");
                    return;
                }

                // Перебираем вопросы и проверяем ответы пользователя
                int score = 0;
                foreach (var group in questions)
                {
                    var question = group.Key;
                    Console.WriteLine($"Вопрос: {question.QuestionText}");

                    var answers = group.Select((a, index) => new { Index = index + 1, a.AnswerText, a.IsCorrect }).OrderBy(a => Guid.NewGuid()).ToList();
                    int i = 1;
                    foreach (var answer in answers)
                    {

                        Console.WriteLine($"{i++}. {answer.AnswerText}");
                    }

                    Console.Write("Введите номер ответа: ");
                    if (int.TryParse(Console.ReadLine()?.Trim(), out int userAnswerIndex) &&
                        userAnswerIndex > 0 && userAnswerIndex <= answers.Count &&
                        answers[userAnswerIndex - 1].IsCorrect)
                    {
                        Console.WriteLine("Верно!");
                        score++;
                    }
                    else
                    {
                        Console.WriteLine("Неверно.");
                    }
                }

                Console.WriteLine($"Вы ответили правильно на {score} из {questions.Count} вопросов.");
                string getUserIdQuery = "SELECT Id FROM Users WHERE Login = @Login";

                // Получаем UserId текущего пользователя
                int currentUserId = conn.QuerySingleOrDefault<int>(getUserIdQuery, new { Login = login });

                if (currentUserId == 0)
                {
                    Console.WriteLine("Ошибка: пользователь с логином не найден.");
                    return;
                }

                // После завершения викторины добавляем запись в таблицу результатов
                string insertQuery = @"
                INSERT INTO QuizResults (UserId, QuizTitle, Score, TotalQuestions, CompletionDate)
                VALUES (@UserId, @QuizTitle, @Score, @TotalQuestions, GETDATE())";

                conn.Execute(insertQuery, new
                {
                    UserId = currentUserId, 
                    QuizTitle = quizTitle,
                    Score = score,     
                    TotalQuestions = questions.Count 
                });
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Произошла ошибка: " + ex.Message);
        }
    }
    private static void RunRandomQuiz(string login)
    {
        try
        {
            using (var conn = new SqlConnection(connectionString))
            {
                Console.WriteLine("Начало случайной викторины.");

                string query = @"
            SELECT 
                q.Id AS QuestionId,
                q.Text AS QuestionText,
                a.Id AS AnswerId,
                a.Text AS AnswerText,
                a.IsCorrect
            FROM Questions q
            JOIN Answers a ON q.Id = a.QuestionId
            JOIN Quizzes z ON q.QuizId = z.Id
            ORDER BY q.Id, a.Id";

                var allQuestions = conn.Query<QuestionWithAnswers>(query).GroupBy(q => new { q.QuestionId, q.QuestionText }).ToList();

                if (allQuestions.Count == 0)
                {
                    Console.WriteLine("Вопросы отсутствуют.");
                    return;
                }

                var randomQuestions = allQuestions.OrderBy(q => Guid.NewGuid()).Take(5).ToList();

                int score = 0;
                foreach (var group in randomQuestions)
                {
                    var question = group.Key;
                    Console.WriteLine($"Вопрос: {question.QuestionText}");

                    var answers = group.Select((a, index) => new { Index = index + 1, a.AnswerText, a.IsCorrect }).OrderBy(a => Guid.NewGuid()).ToList();
                    int i = 1;
                    foreach (var answer in answers)
                    {
                        Console.WriteLine($"{i++}. {answer.AnswerText}");
                    }

                    Console.Write("Введите номер ответа: ");
                    if (int.TryParse(Console.ReadLine()?.Trim(), out int userAnswerIndex) &&
                        userAnswerIndex > 0 && userAnswerIndex <= answers.Count &&
                        answers[userAnswerIndex - 1].IsCorrect)
                    {
                        Console.WriteLine("Верно!");
                        score++;
                    }
                    else
                    {
                        Console.WriteLine("Неверно.");
                    }
                }

                Console.WriteLine($"Вы ответили правильно на {score} из {randomQuestions.Count} вопросов.");
                string getUserIdQuery = "SELECT Id FROM Users WHERE Login = @Login";
                int currentUserId = conn.QuerySingleOrDefault<int>(getUserIdQuery, new { Login = login });

                if (currentUserId == 0)
                {
                    Console.WriteLine("Ошибка: пользователь с логином не найден.");
                    return;
                }

                string insertQuery = @"
            INSERT INTO QuizResults (UserId, QuizTitle, Score, TotalQuestions, CompletionDate)
            VALUES (@UserId, @QuizTitle, @Score, @TotalQuestions, GETDATE())";

                conn.Execute(insertQuery, new
                {
                    UserId = currentUserId, 
                    QuizTitle = "Смешанная викторина", 
                    Score = score,       
                    TotalQuestions = randomQuestions.Count
                });
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Произошла ошибка: " + ex.Message);
        }
    }

    private class QuestionWithAnswers
    {
        public int QuestionId { get; set; }
        public string QuestionText { get; set; }
        public int AnswerId { get; set; }
        public string AnswerText { get; set; }
        public bool IsCorrect { get; set; }
    }

    public class Quiz
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
    }
}



