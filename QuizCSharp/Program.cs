using System;

class Program
{
    static void Main()
    {
        Console.WriteLine("Добро пожаловать в игру Викторина");
        string login;
        while (true)
        {
            Console.WriteLine("1 - Авторизация | 2 - Регистрация");
            uint choice;
            while (!uint.TryParse(Console.ReadLine(), out choice) || (choice != 1 && choice != 2))
            {
                Console.WriteLine("Введите корректный выбор: 1 или 2");
            }

            if (choice == 1)
            {
                login = Authorization.Login();
                if (login != null)
                {
                    break;
                }
            }
            else if (choice == 2)
            {
                login = Authorization.Register();
                if (login != null)
                {
                    break;
                }
            }
        }

        Console.WriteLine("Вы успешно вошли в игру!");
        
        while (true)
        {
            Console.WriteLine("1 - Начать викторину | 2 - Прошлые результаты | 3 - Изменить данные | 4 - Выход");
            uint choice;
            while (!uint.TryParse(Console.ReadLine(), out choice) || (choice != 1 && choice != 2 && choice != 3 && choice != 4))
            {
                Console.WriteLine("Введите корректный выбор: 1, 2, 3 либо 4");
            }

            if (choice == 1)
            {
                QuizApp.Start(login);
            }
            else if (choice == 2)
            {
                Results.GetUserQuizResults(login);

            }
            else if (choice == 3)
            {
                ChangeInfo.UpdateUserInfo(login);

            }
            else if (choice == 4)
            {
                Console.WriteLine("Программа завершена.");
                return;
            }
        }
    }
}
