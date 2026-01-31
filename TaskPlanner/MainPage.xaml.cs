using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using TaskPlanner.Models;
using TaskPlanner.Services;
using Xamarin.Forms;

namespace TaskPlanner
{
    public partial class MainPage : ContentPage
    {
        private List<TaskItem> _tasks;
        private DataService _dataService;
        private string _selectedCategory = "Работа";

        public int TotalTasks => _tasks?.Count ?? 0;
        public int CompletedTasks => _tasks?.Count(t => t.IsCompleted) ?? 0;

        public MainPage()
        {
            InitializeComponent();
            _dataService = new DataService();
            _tasks = new List<TaskItem>();

            // Устанавливаем первую категорию по умолчанию
            CategoryPicker.SelectedIndex = 0;

            // Загружаем задачи при запуске
            LoadTasks();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadTasks();
        }

        private async Task LoadTasks()
        {
            _tasks = await _dataService.LoadTasksAsync();
            UpdateTasksList();
            OnPropertyChanged(nameof(TotalTasks));
            OnPropertyChanged(nameof(CompletedTasks));
        }

        private void UpdateTasksList()
        {
            TasksContainer.Children.Clear();

            if (!_tasks.Any())
            {
                var emptyLabel = new Label
                {
                    Text = "Задачи отсутствуют",
                    HorizontalOptions = LayoutOptions.Center,
                    FontSize = Device.GetNamedSize(NamedSize.Medium, typeof(Label)),
                    TextColor = Color.Gray,
                    Margin = new Thickness(0, 20)
                };
                TasksContainer.Children.Add(emptyLabel);
                return;
            }

            foreach (var task in _tasks.OrderByDescending(t => t.CreatedDate))
            {
                var taskFrame = CreateTaskFrame(task);
                TasksContainer.Children.Add(taskFrame);
            }
        }

        private Frame CreateTaskFrame(TaskItem task)
        {
            // Сначала создаем Grid
            var grid = new Grid
            {
                ColumnDefinitions =
        {
            new ColumnDefinition { Width = GridLength.Auto },
            new ColumnDefinition { Width = GridLength.Star },
            new ColumnDefinition { Width = GridLength.Auto }
        },
                RowDefinitions =
        {
            new RowDefinition { Height = GridLength.Auto },
            new RowDefinition { Height = GridLength.Auto }
        },
                ColumnSpacing = 10,
                RowSpacing = 5
            };

            // Создаем CheckBox
            var checkBox = new CheckBox
            {
                IsChecked = task.IsCompleted,
                Color = Color.FromHex("#4CAF50"),
                VerticalOptions = LayoutOptions.Start
            };

            // Создаем Frame ПЕРЕД тем как добавлять обработчик
            var taskFrame = new Frame
            {
                Content = grid,
                BackgroundColor = task.IsCompleted ? Color.FromHex("#E8F5E9") : Color.White,
                BorderColor = Color.FromHex("#E0E0E0"),
                CornerRadius = 8,
                Padding = new Thickness(10),
                Margin = new Thickness(0, 0, 0, 5)
            };

            // Теперь добавляем обработчик - taskFrame уже создан!
            checkBox.CheckedChanged += async (s, e) =>
            {
                task.IsCompleted = e.Value;
                _dataService.UpdateTask(task);
                await _dataService.SaveTasksAsync();
                UpdateTaskVisual(taskFrame, task);
                OnPropertyChanged(nameof(CompletedTasks));
            };

            // Добавляем CheckBox в Grid
            Grid.SetRowSpan(checkBox, 2);
            grid.Children.Add(checkBox);

            // Название задачи
            var titleLabel = new Label
            {
                Text = task.Title,
                FontSize = Device.GetNamedSize(NamedSize.Medium, typeof(Label)),
                TextDecorations = task.IsCompleted ? TextDecorations.Strikethrough : TextDecorations.None,
                TextColor = task.IsCompleted ? Color.Gray : Color.Black
            };
            Grid.SetColumn(titleLabel, 1);
            grid.Children.Add(titleLabel);

            // Категория и дата
            var detailsLabel = new Label
            {
                Text = $"{task.Category} • {task.CreatedDate:dd.MM.yyyy HH:mm}",
                FontSize = Device.GetNamedSize(NamedSize.Small, typeof(Label)),
                TextColor = Color.Gray
            };
            Grid.SetRow(detailsLabel, 1);
            Grid.SetColumn(detailsLabel, 1);
            grid.Children.Add(detailsLabel);

            // Кнопки действий
            var actionsStack = new StackLayout
            {
                Orientation = StackOrientation.Horizontal,
                Spacing = 5
            };


            // Кнопка редактирования
            var editButton = new Button
            {
                Text = "Редактировать",
                BackgroundColor = Color.Transparent,
                FontSize = Device.GetNamedSize(NamedSize.Medium, typeof(Button)),
                WidthRequest = 40
            };
            editButton.Clicked += async (s, e) => await OnEditTaskClicked(task);

            // Кнопка удаления
            var deleteButton = new Button
            {
                Text = "Удалить",
                BackgroundColor = Color.Transparent,
                FontSize = Device.GetNamedSize(NamedSize.Medium, typeof(Button)),
                WidthRequest = 40
            };
            deleteButton.Clicked += async (s, e) => await OnDeleteTaskClicked(task);

            actionsStack.Children.Add(editButton);
            actionsStack.Children.Add(deleteButton);

            Grid.SetColumn(actionsStack, 2);
            Grid.SetRowSpan(actionsStack, 2);
            grid.Children.Add(actionsStack);

            return new Frame
            {
                Content = grid,
                BackgroundColor = task.IsCompleted ? Color.FromHex("#E8F5E9") : Color.White,
                BorderColor = Color.FromHex("#E0E0E0"),
                CornerRadius = 8,
                Padding = new Thickness(10),
                Margin = new Thickness(0, 0, 0, 5)
            };
        }

        private void UpdateTaskVisual(Frame taskFrame, TaskItem task)
        {
            var grid = taskFrame.Content as Grid;
            var titleLabel = grid.Children.OfType<Label>().FirstOrDefault();

            if (titleLabel != null)
            {
                titleLabel.TextDecorations = task.IsCompleted ?
                    TextDecorations.Strikethrough : TextDecorations.None;
                titleLabel.TextColor = task.IsCompleted ? Color.Gray : Color.Black;
            }

            taskFrame.BackgroundColor = task.IsCompleted ?
                Color.FromHex("#E8F5E9") : Color.White;
        }

        private async void OnAddTaskClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TaskEntry.Text))
            {
                await DisplayAlert("Ошибка", "Введите название задачи", "OK");
                return;
            }

            var newTask = new TaskItem(TaskEntry.Text, _selectedCategory);
            _dataService.AddTask(newTask);
            await _dataService.SaveTasksAsync();

            TaskEntry.Text = string.Empty;
            await LoadTasks();
        }

        private void OnCategorySelected(object sender, EventArgs e)
        {
            var picker = sender as Picker;
            if (picker.SelectedIndex != -1)
            {
                _selectedCategory = picker.SelectedItem.ToString();
            }
        }

        private async Task OnEditTaskClicked(TaskItem task)
        {
            var newTitle = await DisplayPromptAsync(
                "Редактирование задачи",
                "Введите новое название:",
                initialValue: task.Title,
                maxLength: 100);

            if (!string.IsNullOrWhiteSpace(newTitle))
            {
                task.Title = newTitle;
                _dataService.UpdateTask(task);
                await _dataService.SaveTasksAsync();
                await LoadTasks();
            }
        }

        private async Task OnDeleteTaskClicked(TaskItem task)
        {
            var result = await DisplayAlert(
                "Удаление задачи",
                $"Вы уверены, что хотите удалить задачу '{task.Title}'?",
                "Удалить",
                "Отмена");

            if (result)
            {
                _dataService.RemoveTask(task.Id);
                await _dataService.SaveTasksAsync();
                await LoadTasks();
            }
        }
    }
}