/* Пространства имен */

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using TaskPlanner.Models;
using Newtonsoft.Json; // для работы с JSON
using Xamarin.Essentials;

namespace TaskPlanner.Services
{
    public class DataService
    {
        private readonly string _filePath; // путь к файлу
        private List<TaskItem> _tasks; // список задач

        public DataService()
        {
            _filePath = Path.Combine(FileSystem.AppDataDirectory, "tasks.json"); // имя файла
            _tasks = new List<TaskItem>();
        }

        public async Task<List<TaskItem>> LoadTasksAsync() // загрузка данных из JSON
        {
            try
            {
                if (File.Exists(_filePath)) // если файл имеется, загружаем все оттуда
                {
                    using (var reader = new StreamReader(_filePath))
                    {
                        var json = await reader.ReadToEndAsync();
                        _tasks = JsonConvert.DeserializeObject<List<TaskItem>>(json) ?? new List<TaskItem>();
                    }
                }
                return _tasks;
            }
            catch (Exception ex) // обработка исключений
            {
                Console.WriteLine($"Ошибка загрузки: {ex.Message}");
                return new List<TaskItem>();
            }
        }

        public async Task SaveTasksAsync() // сохраняем
        {
            try
            {
                var json = JsonConvert.SerializeObject(_tasks, Formatting.Indented);
                using (var writer = new StreamWriter(_filePath, false))
                {
                    await writer.WriteAsync(json);
                }
            }
            catch (Exception ex) // обработка исключений
            {
                Console.WriteLine($"Ошибка сохранения: {ex.Message}");
            }
        }

        public void AddTask(TaskItem task) // добавляем задачу
        {
            _tasks.Add(task);
        }

        public bool RemoveTask(string id) // удаляем по ID
        {
            var task = _tasks.Find(t => t.Id == id);
            if (task != null)
            {
                return _tasks.Remove(task);
            }
            return false;
        }

        public void UpdateTask(TaskItem updatedTask) // редактирование по ID
        {
            var index = _tasks.FindIndex(t => t.Id == updatedTask.Id);
            if (index != -1)
            {
                _tasks[index] = updatedTask;
            }
        }
    }
}
