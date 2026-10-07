using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using SimpleReminders.Data;
using SimpleReminders.Models;
using SimpleReminders.Services;

namespace SimpleReminders;

public partial class MainWindow : Window
{
    private readonly ReminderStorage _storage = new();
    private readonly ICollectionView _reminderView;

    public ObservableCollection<Reminder> Reminders { get; } = [];
    public ObservableCollection<ReminderList> CustomLists { get; } = [];
    public ObservableCollection<StoreItem> StoreItems { get; } = [];
    public ObservableCollection<Achievement> Achievements { get; } = [];
    public ObservableCollection<StoreItem> OwnedStoreItems { get; } = [];
    public ObservableCollection<Reminder> CalendarDayReminders { get; } = [];
    public ObservableCollection<Reminder> HistoryItems { get; } = [];

    private UserProgress _progress = new();
    private Guid? _selectedListId;
    private Reminder? _pendingCompletionReminder;
    private DateTime _calendarMonth = new(DateTime.Today.Year, DateTime.Today.Month, 1);
    private DateTime _selectedCalendarDate = DateTime.Today;

    public MainWindow()
    {
        InitializeComponent();

        _reminderView = CollectionViewSource.GetDefaultView(Reminders);
        _reminderView.Filter = FilterReminder;

        foreach (var item in RewardCatalog.CreateStoreItems())
        {
            StoreItems.Add(item);
        }

        LoadData();

        ReminderList.ItemsSource = _reminderView;
        CustomListsList.ItemsSource = CustomLists;
        StoreItemsList.ItemsSource = StoreItems;
        RewardItemsList.ItemsSource = OwnedStoreItems;
        AchievementItemsList.ItemsSource = OwnedStoreItems;
        CalendarRemindersList.ItemsSource = CalendarDayReminders;
        HistoryItemsList.ItemsSource = HistoryItems;
        EnsureProgressDefaults();
        ApplyTheme(_progress.EquippedThemeId);
        SyncRewardCatalogState();

        ListsList.SelectedIndex = 0;
        ShowRemindersView();
        RefreshView();
        RefreshRewardsView();
        RefreshCalendar();
    }

    private void LoadData()
    {
        var data = _storage.Load();

        _progress = data.Progress ?? new UserProgress();

        foreach (var reminder in data.Reminders)
        {
            AddReminderToCollection(reminder);
        }

        foreach (var list in data.Lists)
        {
            CustomLists.Add(list);
        }
    }

    private void EnsureProgressDefaults()
    {
        _progress.RewardedReminderIds ??= [];
        _progress.UnlockedAchievementIds ??= [];
        _progress.OwnedStoreItemIds ??= [];

        if (!_progress.OwnedStoreItemIds.Contains("theme.default"))
        {
            _progress.OwnedStoreItemIds.Add("theme.default");
        }

        if (string.IsNullOrWhiteSpace(_progress.EquippedThemeId))
        {
            _progress.EquippedThemeId = "theme.default";
        }
    }

    private void SyncRewardCatalogState()
    {
        foreach (var item in StoreItems)
        {
            item.IsOwned = _progress.OwnedStoreItemIds.Contains(item.Id);
            item.IsEquipped = item.Id.Equals(
                _progress.EquippedThemeId,
                StringComparison.OrdinalIgnoreCase);
        }

        foreach (var achievement in Achievements)
        {
            achievement.IsUnlocked = _progress.UnlockedAchievementIds.Contains(achievement.Id);
        }

        OwnedStoreItems.Clear();
        foreach (var item in StoreItems.Where(item => item.IsOwned))
        {
            OwnedStoreItems.Add(item);
        }
    }

    private void AddReminderToCollection(Reminder reminder)
    {
        reminder.PropertyChanged += Reminder_PropertyChanged;
        Reminders.Add(reminder);
    }

    private void Reminder_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is Reminder reminder &&
            e.PropertyName == nameof(Reminder.IsCompleted) &&
            !reminder.IsCompleted)
        {
            _progress.RewardedReminderIds.Remove(reminder.Id);
        }

        _storage.Save(Reminders, CustomLists, _progress);

        if (e.PropertyName == nameof(Reminder.IsCompleted) ||
            e.PropertyName == nameof(Reminder.DueDate))
        {
            _reminderView.Refresh();
            RefreshView();
            RefreshCalendar();
            RefreshHistory();
        }
    }

    private bool FilterReminder(object item)
    {
        if (item is not Reminder reminder)
        {
            return false;
        }

        if (_selectedListId.HasValue &&
            reminder.ListId != _selectedListId.Value.ToString())
        {
            return false;
        }

        var search = SearchInput.Text.Trim();

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

        var visibleCount = _reminderView.Cast<Reminder>().Count();

        ReminderCountText.Text = visibleCount.ToString();
        SidebarAllCountText.Text = Reminders.Count.ToString();

        EmptyStatePanel.Visibility = visibleCount == 0
            ? Visibility.Visible
            : Visibility.Collapsed;

        NoListsText.Visibility = CustomLists.Count == 0
            ? Visibility.Visible
            : Visibility.Collapsed;

        RefreshRewardsView();
    }

    private void RefreshRewardsView()
    {
        RewardsCoinsText.Text = _progress.Coins.ToString();
        StoreCoinsText.Text = _progress.Coins.ToString();
        RewardsStatusText.Text =
            string.IsNullOrWhiteSpace(RewardsStatusText.Text)
                ? "Finish a reminder to earn 10 coins."
                : RewardsStatusText.Text;

        SyncRewardCatalogState();
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
        SaveAll();

        ReminderInput.Clear();
        DueDatePicker.SelectedDate = null;
        ReminderInput.Focus();

        RefreshView();
    }

    private void ReminderCheckBox_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (sender is not CheckBox checkBox ||
            checkBox.DataContext is not Reminder reminder)
        {
            return;
        }

        if (!reminder.IsCompleted)
        {
            e.Handled = true;
            ShowCompletionConfirmation(reminder);
        }
    }

    private void ShowCompletionConfirmation(Reminder reminder)
    {
        _pendingCompletionReminder = reminder;
        ConfirmationReminderText.Text = reminder.Title;
        CompletionConfirmationOverlay.Visibility = Visibility.Visible;
    }

    private void CancelCompletionConfirmation_Click(object sender, RoutedEventArgs e)
    {
        _pendingCompletionReminder = null;
        CompletionConfirmationOverlay.Visibility = Visibility.Collapsed;
    }

    private void ConfirmCompletion_Click(object sender, RoutedEventArgs e)
    {
        var reminder = _pendingCompletionReminder;

        _pendingCompletionReminder = null;
        CompletionConfirmationOverlay.Visibility = Visibility.Collapsed;

        if (reminder is null || reminder.IsCompleted)
        {
            return;
        }

        reminder.IsCompleted = true;
        AwardCompletion(reminder);

        SaveAll();
        RefreshView();
    }

    private void AwardCompletion(Reminder reminder)
    {
        if (_progress.RewardedReminderIds.Contains(reminder.Id))
        {
            RewardsStatusText.Text = "This reminder was already rewarded.";
            return;
        }

        _progress.RewardedReminderIds.Add(reminder.Id);
        _progress.TotalCompleted++;
        _progress.Coins += 10;
        RewardsStatusText.Text = "+10 coins earned. Nice work.";

        RefreshRewardsView();
        SaveAll();
    }

    private void DeleteReminder_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button button ||
            button.Tag is not Reminder reminder)
        {
            return;
        }

        var deleteMessage = reminder.IsCompleted
            ? "Delete this completed reminder?"
            : "What happened? Got tired already and gave up so quickly?";

        var result = MessageBox.Show(
            deleteMessage,
            "Delete Reminder",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result != MessageBoxResult.Yes)
        {
            return;
        }

        reminder.PropertyChanged -= Reminder_PropertyChanged;
        Reminders.Remove(reminder);

        SaveAll();
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
        SaveAll();

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

    private void DeleteList_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not MenuItem menuItem ||
            menuItem.DataContext is not ReminderList list)
        {
            return;
        }

        var result = MessageBox.Show(
            $"Delete the list \"{list.Name}\"? Reminders inside it will be kept in All Reminders.",
            "Delete List",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result != MessageBoxResult.Yes)
        {
            return;
        }

        var wasSelected = _selectedListId == list.Id;

        foreach (var reminder in Reminders.Where(
                     r => r.ListId == list.Id.ToString()))
        {
            reminder.ListId = null;
        }

        CustomLists.Remove(list);
        SaveAll();

        if (wasSelected)
        {
            _selectedListId = null;
            ListsList.SelectedIndex = 0;
            PageTitleText.Text = "All Reminders";
        }

        RefreshView();
    }

    private void ListsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (ListsList.SelectedIndex == 0)
        {
            _selectedListId = null;
            PageTitleText.Text = "All Reminders";
            CustomListsList.SelectedItem = null;
            ShowRemindersView();
            RefreshView();
            return;
        }

        CustomListsList.SelectedItem = null;

        switch (ListsList.SelectedIndex)
        {
            case 1:
                ShowHistoryView();
                break;

            case 2:
                ShowCalendarView();
                break;

            case 3:
                ShowRewardsView();
                break;

            case 4:
                ShowStoreView();
                break;

            case 5:
                ShowAchievementsView();
                break;
        }
    }

    private void CustomListsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (CustomListsList.SelectedItem is not ReminderList list)
        {
            return;
        }

        _selectedListId = list.Id;
        PageTitleText.Text = list.Name;
        ListsList.SelectedIndex = -1;

        ShowRemindersView();
        RefreshView();
    }

    private void ShowRemindersView()
    {
        ReminderContentPanel.Visibility = Visibility.Visible;
        HistoryContentPanel.Visibility = Visibility.Collapsed;
        CalendarContentPanel.Visibility = Visibility.Collapsed;
        RewardsContentPanel.Visibility = Visibility.Collapsed;
        StoreContentPanel.Visibility = Visibility.Collapsed;
        AchievementsContentPanel.Visibility = Visibility.Collapsed;
    }

    private void ShowHistoryView()
    {
        ReminderContentPanel.Visibility = Visibility.Collapsed;
        HistoryContentPanel.Visibility = Visibility.Visible;
        CalendarContentPanel.Visibility = Visibility.Collapsed;
        RewardsContentPanel.Visibility = Visibility.Collapsed;
        StoreContentPanel.Visibility = Visibility.Collapsed;
        AchievementsContentPanel.Visibility = Visibility.Collapsed;
        RefreshHistory();
    }

    private void ShowCalendarView()
    {
        ReminderContentPanel.Visibility = Visibility.Collapsed;
        HistoryContentPanel.Visibility = Visibility.Collapsed;
        CalendarContentPanel.Visibility = Visibility.Visible;
        RewardsContentPanel.Visibility = Visibility.Collapsed;
        StoreContentPanel.Visibility = Visibility.Collapsed;
        AchievementsContentPanel.Visibility = Visibility.Collapsed;
        RefreshCalendar();
    }

    private void ShowRewardsView()
    {
        ReminderContentPanel.Visibility = Visibility.Collapsed;
        HistoryContentPanel.Visibility = Visibility.Collapsed;
        CalendarContentPanel.Visibility = Visibility.Collapsed;
        RewardsContentPanel.Visibility = Visibility.Visible;
        StoreContentPanel.Visibility = Visibility.Collapsed;
        AchievementsContentPanel.Visibility = Visibility.Collapsed;
        RefreshRewardsView();
    }

    private void ShowStoreView()
    {
        ReminderContentPanel.Visibility = Visibility.Collapsed;
        HistoryContentPanel.Visibility = Visibility.Collapsed;
        CalendarContentPanel.Visibility = Visibility.Collapsed;
        RewardsContentPanel.Visibility = Visibility.Collapsed;
        StoreContentPanel.Visibility = Visibility.Visible;
        AchievementsContentPanel.Visibility = Visibility.Collapsed;
        RefreshRewardsView();
    }

    private void ShowAchievementsView()
    {
        ReminderContentPanel.Visibility = Visibility.Collapsed;
        HistoryContentPanel.Visibility = Visibility.Collapsed;
        CalendarContentPanel.Visibility = Visibility.Collapsed;
        RewardsContentPanel.Visibility = Visibility.Collapsed;
        StoreContentPanel.Visibility = Visibility.Collapsed;
        AchievementsContentPanel.Visibility = Visibility.Visible;
        SyncRewardCatalogState();
    }

    private void StoreItemButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button button ||
            button.Tag is not StoreItem item)
        {
            return;
        }

        if (item.IsEquipped)
        {
            return;
        }

        if (!item.IsOwned)
        {
            if (_progress.Coins < item.Price)
            {
                RewardsStatusText.Text = "Not enough coins for this item.";
                return;
            }

            _progress.Coins -= item.Price;

            if (!_progress.OwnedStoreItemIds.Contains(item.Id))
            {
                _progress.OwnedStoreItemIds.Add(item.Id);
            }

            RewardsStatusText.Text = $"{item.Name} purchased.";
        }

        EquipStoreItem(item);
        SaveAll();
        RefreshRewardsView();
    }

    private void EquipStoreItem(StoreItem item)
    {
        foreach (var storeItem in StoreItems)
        {
            storeItem.IsEquipped = false;
        }

        item.IsOwned = true;
        item.IsEquipped = true;
        _progress.EquippedThemeId = item.Id;

        ApplyTheme(item.Id);
        RewardsStatusText.Text = $"{item.Name} equipped.";
    }

    private void ApplyTheme(string themeId)
    {
        var palette = themeId switch
        {
            "theme.ocean" => new ThemePalette(
                Color.FromRgb(13, 138, 188),
                Color.FromRgb(7, 108, 151),
                Color.FromRgb(230, 247, 253)),

            "theme.violet" => new ThemePalette(
                Color.FromRgb(124, 77, 255),
                Color.FromRgb(94, 53, 177),
                Color.FromRgb(241, 236, 255)),

            "theme.forest" => new ThemePalette(
                Color.FromRgb(46, 139, 87),
                Color.FromRgb(34, 110, 68),
                Color.FromRgb(232, 247, 238)),

            "theme.sunset" => new ThemePalette(
                Color.FromRgb(230, 106, 44),
                Color.FromRgb(194, 81, 28),
                Color.FromRgb(255, 239, 231)),

            _ => new ThemePalette(
                Color.FromRgb(52, 120, 246),
                Color.FromRgb(40, 101, 219),
                Color.FromRgb(234, 241, 255))
        };

        Resources["AccentBrush"] = new SolidColorBrush(palette.Accent);
        Resources["AccentHoverBrush"] = new SolidColorBrush(palette.Hover);
        Resources["AccentSoftBrush"] = new SolidColorBrush(palette.Soft);
    }

    private void SaveAll()
    {
        _storage.Save(Reminders, CustomLists, _progress);
    }

    private readonly record struct ThemePalette(
        Color Accent,
        Color Hover,
        Color Soft);
}
