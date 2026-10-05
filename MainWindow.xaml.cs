using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using SimpleReminders.Data;
using SimpleReminders.Models;

namespace SimpleReminders;

public partial class MainWindow : Window
{
    private readonly ReminderStorage _storage = new();
    private readonly ICollectionView _reminderView;

    public ObservableCollection<Reminder> Reminders { get; } = [];
    public ObservableCollection<ReminderList> CustomLists { get; } = [];

    private Guid? _selectedListId;

    public MainWindow()
    {
        InitializeComponent();

        _reminderView = CollectionViewSource.GetDefaultCollectionView(Reminders);
        _reminderView.Filter = FilterReminder;

        LoadData();

        ReminderList.ItemsSource = _reminderView;
        CustomListsList.ItemsSource = CustomLists;

        _selectedListId = null;
        RefreshView();
    }

    private void LoadData()
    {
        var data = _storage.Load();

        foreach (var reminder in data.Reminders)
        {
            AddReminderToCollection(reminder);
        }

        foreach (var list in data.Lists)
        {
            CustomLists.Add(list);
        }
    }

    private void AddReminderToCollection(Reminder reminder)
    {
        reminder.PropertyChanged += Reminder_PropertyChanged;
        Reminders.Add(reminder);
    }

    private void Reminder_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        _storage.Save(Reminders, CustomLists);

        if (e.PropertyName == nameof(Reminder.IsCompleted))
        {
            _reminderView.Refresh();
        }
    }

    private bool FilterReminder(object item)
    {
        if (item is not Reminder reminder)
        {
            return false;
        }

        if (_selectedListId.HasValue && reminder.ListId != _selectedListId.Value.ToString())
        {
            return false;
        }

        var search = SearchInput?.Text?.Trim();

        if (string.IsNullOrWhiteSpace(search))
        {
            return true;
        }

        return reminder.Title.Contains(search, StringComparison.OrdinalIgnoreCase) ||
               reminder.Notes.Contains(search, StringComparison.OrdinalIgnoreCase);
    }

    private void RefreshView()
    {
        _reminderView.Refresh();

        var visibleCount = _reminderView.Cast<object>().Count();

        ReminderCountText.Text = visibleCount.ToString();
        SidebarAllCountText.Text = Reminders.Count.ToString();
        EmptyStatePanel.Visibility = visibleCount == 0
            ? Visibility.Visible
            : Visibility.Collapsed;

        NoListsText.Visibility = CustomLists.Count == 0
            ? Visibility.Visible
            : Visibility.Collapsed;
    }

    private void AddReminder_Click(object sender, RoutedEventArgs e)
    {
        var title = ReminderInput.Text.Trim();

        if (string.IsNullOrWhiteSpace(title))
        {
            ReminderInput.Focus();
            return;
        }

        var reminder = new Reminder
        {
            Title = title,
            DueDate = DueDatePicker.SelectedDate,
            ListId = _selectedListId?.ToString()
        };

        AddReminderToCollection(reminder);
        _storage.Save(Reminders, CustomLists);

        ReminderInput.Clear();
        DueDatePicker.SelectedDate = null;
        ReminderInput.Focus();

        RefreshView();
    }

    private void DeleteReminder_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button button || button.Tag is not Reminder reminder)
        {
            return;
        }

        reminder.PropertyChanged -= Reminder_PropertyChanged;
        Reminders.Remove(reminder);

        _storage.Save(Reminders, CustomLists);
        RefreshView();
    }

    private void ReminderCheckBox_Changed(object sender, RoutedEventArgs e)
    {
        _storage.Save(Reminders, CustomLists);
        RefreshView();
    }

    private void SearchInput_TextChanged(object sender, TextChangedEventArgs e)
    {
        RefreshView();
    }

    private void NewListButton_Click(object sender, RoutedEventArgs e)
    {
        NewListPanel.Visibility = Visibility.Visible;
        NewListInput.Clear();
        NewListInput.Focus();
    }

    private void CreateList_Click(object sender, RoutedEventArgs e)
    {
        var name = NewListInput.Text.Trim();

        if (string.IsNullOrWhiteSpace(name))
        {
            NewListInput.Focus();
            return;
        }

        if (CustomLists.Any(x =>
                x.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
        {
            MessageBox.Show(
                "A list with this name already exists.",
                "Simple Reminders",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            NewListInput.Focus();
            return;
        }

        var list = new ReminderList
        {
            Name = name
        };

        CustomLists.Add(list);
        _storage.Save(Reminders, CustomLists);

        NewListPanel.Visibility = Visibility.Collapsed;
        NewListInput.Clear();

        CustomListsList.SelectedItem = list;
        RefreshView();
    }

    private void CancelNewList_Click(object sender, RoutedEventArgs e)
    {
        NewListPanel.Visibility = Visibility.Collapsed;
        NewListInput.Clear();
    }

    private void ListsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (ListsList.SelectedIndex != 0)
        {
            return;
        }

        _selectedListId = null;
        PageTitleText.Text = "All Reminders";

        if (CustomListsList.SelectedItem is not null)
        {
            CustomListsList.SelectedItem = null;
        }

        RefreshView();
    }

    private void CustomListsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (CustomListsList.SelectedItem is not ReminderList list)
        {
            return;
        }

        _selectedListId = list.Id;
        PageTitleText.Text = list.Name;

        if (ListsList.SelectedIndex != -1)
        {
            ListsList.SelectedIndex = -1;
        }

        RefreshView();
    }
}
