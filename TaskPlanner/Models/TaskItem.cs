/* Пространства имен */

using System;

// необходимое пространство имен для работы с JSON. Можно установить через NuGet
using Newtonsoft.Json; 

namespace TaskPlanner.Models
{
    // класс "содержимое задачи"
    public class TaskItem
    {

        public string Id { get; set; } // номер
        public string Title { get; set; } // заголовок
        public string Category { get; set; } // категория
        public DateTime CreatedDate { get; set; } // дата создания
        public bool IsCompleted { get; set; } // завершена или нет

        public TaskItem() 
        {
            Id = Guid.NewGuid().ToString();
            CreatedDate = DateTime.Now;
        }

        public TaskItem(string title, string category) : this()
        {
            Title = title;
            Category = category;
        }

    }
}