using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Animation;
using SimpleReminders.Data;
using SimpleReminders.Models;
using SimpleReminders.Services;

namespace SimpleReminders;

public partial class MainWindow : Window
{
    private readonly ReminderStorage _storage = new();
    private readonly ICollectionView _reminderView;
    private readonly ICollectionView _storeView;

    public ObservableCollection<Reminder> Reminders { get; } = [];
    public ObservableCollection<ReminderList> CustomLists { get; } = [];
    public ObservableCollection<StoreItem> StoreItems { get; } = [];
    public ObservableCollection<Achievement> Achievements { get; } = [];
    public ObservableCollection<StoreItem> OwnedStoreItems { get; } = [];
    public ObservableCollection<Reminder> CalendarDayReminders { get; } = [];

    private UserProgress _progress = new();
    private Guid? _selectedListId;
    private Reminder? _pendingCompletionReminder;
    private bool _ownerMode;
    private string _selectedStoreCategory = "All";
    private readonly TextBlock RewardsStatusText = new();
    private DateTime _calendarMonth = new(DateTime.Today.Year, DateTime.Today.Month, 1);
    private DateTime _selectedCalendarDate = DateTime.Today;
    private static readonly DoubleAnimation HalloweenPumpkinLeftAnimation = new()
    {
        From = -6,
        To = 6,
        Duration = TimeSpan.FromSeconds(2.8),
        AutoReverse = true,
        RepeatBehavior = System.Windows.Media.Animation.RepeatBehavior.Forever
    };

    private static readonly DoubleAnimation HalloweenPumpkinRightAnimation = new()
    {
        From = 5,
        To = -5,
        Duration = TimeSpan.FromSeconds(3.4),
        AutoReverse = true,
        RepeatBehavior = System.Windows.Media.Animation.RepeatBehavior.Forever
    };

    private static readonly DoubleAnimation HalloweenFogAnimationA = new()
    {
        From = 0.14,
        To = 0.34,
        Duration = TimeSpan.FromSeconds(4.5),
        AutoReverse = true,
        RepeatBehavior = System.Windows.Media.Animation.RepeatBehavior.Forever
    };

    private static readonly DoubleAnimation HalloweenFogAnimationB = new()
    {
        From = 0.08,
        To = 0.24,
        Duration = TimeSpan.FromSeconds(6.0),
        AutoReverse = true,
        RepeatBehavior = System.Windows.Media.Animation.RepeatBehavior.Forever
    };

    private static readonly DoubleAnimation HalloweenBatAnimation = new()
    {
        From = -30,
        To = 420,
        Duration = TimeSpan.FromSeconds(8),
        RepeatBehavior = System.Windows.Media.Animation.RepeatBehavior.Forever
    };

    private static readonly DoubleAnimation HalloweenSparkleAnimation = new()
    {
        From = 0.25,
        To = 0.95,
        Duration = TimeSpan.FromSeconds(1.6),
        AutoReverse = true,
        RepeatBehavior = System.Windows.Media.Animation.RepeatBehavior.Forever
    };


    public MainWindow()
    {
        InitializeComponent();

        _reminderView = CollectionViewSource.GetDefaultView(Reminders);
        _reminderView.Filter = FilterReminder;

        _storeView = CollectionViewSource.GetDefaultView(StoreItems);
        _storeView.Filter = FilterStoreItem;

        foreach (var item in RewardCatalog.CreateStoreItems())
        {
            StoreItems.Add(item);
        }

        LoadData();

        ReminderList.ItemsSource = _reminderView;
        CustomListsList.ItemsSource = CustomLists;
        StoreItemsList.ItemsSource = _storeView;
        RewardItemsList.ItemsSource = OwnedStoreItems;
        CalendarRemindersList.ItemsSource = CalendarDayReminders;
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

        // Keep only the current theme inventory. Older removed themes are migrated out.
        _progress.OwnedStoreItemIds = _progress.OwnedStoreItemIds
            .Where(id => id.Equals("theme.default", StringComparison.OrdinalIgnoreCase) ||
                         id.Equals("theme.halloween-night", StringComparison.OrdinalIgnoreCase))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (string.IsNullOrWhiteSpace(_progress.EquippedThemeId) ||
            (!_progress.EquippedThemeId.Equals("theme.default", StringComparison.OrdinalIgnoreCase) &&
             !_progress.EquippedThemeId.Equals("theme.halloween-night", StringComparison.OrdinalIgnoreCase)))
        {
            _progress.EquippedThemeId = "theme.default";
        }
    }

    private void SyncRewardCatalogState()
    {
        foreach (var item in StoreItems)
        {
            item.IsOwned = _progress.OwnedStoreItemIds.Contains(item.Id);
            item.IsEquipped =
                item.Category.Equals("Themes", StringComparison.OrdinalIgnoreCase) &&
                item.Id.Equals(
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

    private bool FilterStoreItem(object item)
    {
        if (item is not StoreItem storeItem)
        {
            return false;
        }

        return _selectedStoreCategory.Equals("All", StringComparison.OrdinalIgnoreCase) ||
               storeItem.Category.Equals(_selectedStoreCategory, StringComparison.OrdinalIgnoreCase);
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
        var coinBalance = _ownerMode ? "∞" : _progress.Coins.ToString();

        RewardsCoinsText.Text = coinBalance;
        StoreCoinsText.Text = coinBalance;
        RewardsStatusText.Text =
            string.IsNullOrWhiteSpace(RewardsStatusText.Text)
                ? (_ownerMode
                    ? "Owner mode enabled. Coins are unlimited."
                    : "Finish a reminder to earn 10 coins.")
                : RewardsStatusText.Text;

        SyncRewardCatalogState();
    }

    private void ActivateOwnerMode()
    {
        if (_ownerMode)
        {
            return;
        }

        _ownerMode = true;
        RefreshRewardsView();
    }

    private void DeactivateOwnerMode()
    {
        if (!_ownerMode)
        {
            return;
        }

        _ownerMode = false;
        RewardsStatusText.Text = "Owner mode disabled.";
        RefreshRewardsView();
    }

    private void RefreshCalendar()
    {
        CalendarMonthText.Text = _calendarMonth.ToString("MMMM yyyy");
        CalendarDaysGrid.Children.Clear();

        var firstVisibleDay = _calendarMonth.AddDays(-(int)_calendarMonth.DayOfWeek);

        for (var i = 0; i < 42; i++)
        {
            var day = firstVisibleDay.AddDays(i);
            var reminders = Reminders.Where(r => r.DueDate?.Date == day.Date).ToList();
            var isCurrentMonth = day.Month == _calendarMonth.Month;
            var isToday = day.Date == DateTime.Today;
            var isSelected = day.Date == _selectedCalendarDate.Date;

            var background = isSelected
                ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3478F6"))
                : isToday
                    ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#EAF1FF"))
                    : Brushes.Transparent;

            var foreground = isSelected
                ? Brushes.White
                : isCurrentMonth
                    ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#30353B"))
                    : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#B6BBC2"));

            var content = new StackPanel { VerticalAlignment = VerticalAlignment.Center };
            content.Children.Add(new TextBlock
            {
                Text = day.Day.ToString(),
                FontSize = 13,
                FontWeight = isToday || isSelected ? FontWeights.SemiBold : FontWeights.Normal,
                Foreground = foreground,
                HorizontalAlignment = HorizontalAlignment.Center,
                TextAlignment = TextAlignment.Center
            });

            if (reminders.Count > 0)
            {
                content.Children.Add(new Border
                {
                    Width = 5,
                    Height = 5,
                    CornerRadius = new CornerRadius(3),
                    Background = isSelected ? Brushes.White : (Brush)FindResource("AccentBrush"),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Margin = new Thickness(0, 4, 0, 0)
                });
            }

            var dayButton = new Button
            {
                Content = content,
                Tag = day,
                Margin = new Thickness(3),
                Padding = new Thickness(0),
                BorderThickness = isSelected ? new Thickness(0) : new Thickness(1),
                BorderBrush = isSelected
                    ? Brushes.Transparent
                    : isToday
                        ? (Brush)FindResource("AccentBrush")
                        : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#ECEEF1")),
                Background = background,
                HorizontalContentAlignment = HorizontalAlignment.Center,
                VerticalContentAlignment = VerticalAlignment.Center,
                FontSize = 13,
                ToolTip = reminders.Count == 0 ? null : $"{reminders.Count} reminder{(reminders.Count == 1 ? "" : "s")}"
            };

            dayButton.Click += (_, _) =>
            {
                _selectedCalendarDate = day;
                RefreshCalendar();
            };

            CalendarDaysGrid.Children.Add(dayButton);
        }

        RefreshCalendarDayDetails();
    }

    private void RefreshCalendarDayDetails()
    {
        var reminders = Reminders
            .Where(r => r.DueDate?.Date == _selectedCalendarDate.Date)
            .OrderBy(r => r.IsCompleted)
            .ThenBy(r => r.CreatedAt)
            .ToList();

        CalendarSelectedDateText.Text = _selectedCalendarDate.Date == DateTime.Today
            ? "Today"
            : _selectedCalendarDate.ToString("dddd, MMM d");
        CalendarSelectedCountText.Text = reminders.Count.ToString();
        CalendarDayReminders.Clear();
        foreach (var reminder in reminders)
        {
            CalendarDayReminders.Add(reminder);
        }

        CalendarEmptyText.Visibility = reminders.Count == 0
            ? Visibility.Visible
            : Visibility.Collapsed;
    }

    private void CalendarToday_Click(object sender, RoutedEventArgs e)
    {
        _calendarMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        _selectedCalendarDate = DateTime.Today;
        RefreshCalendar();
    }

    private void PreviousMonth_Click(object sender, RoutedEventArgs e)
    {
        _calendarMonth = _calendarMonth.AddMonths(-1);
        RefreshCalendar();
    }

    private void NextMonth_Click(object sender, RoutedEventArgs e)
    {
        _calendarMonth = _calendarMonth.AddMonths(1);
        RefreshCalendar();
    }

    private void AddReminder_Click(object sender, RoutedEventArgs e)
    {
        var title = ReminderInput.Text.Trim();

        if (string.IsNullOrWhiteSpace(title))
        {
            ReminderInput.Focus();
            return;
        }

        DateTime? dueDate = null;

        if (!string.IsNullOrWhiteSpace(DueDateInput.Text))
        {
            if (!TryParseDueDate(DueDateInput.Text, out dueDate))
            {
                MessageBox.Show(
                    "I couldn't understand that date. Try something like \"tomorrow\", \"12 of November\", \"Oct 12\", or \"12/10/2026\".",
                    "Invalid Due Date",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                DueDateInput.Focus();
                return;
            }
        }

        var reminder = new Reminder
        {
            Title = title,
            DueDate = dueDate,
            ListId = _selectedListId?.ToString()
        };

        AddReminderToCollection(reminder);
        SaveAll();

        ReminderInput.Clear();
        DueDateCalendar.SelectedDate = null;
        DueDateInput.Clear();
        DueDatePopup.IsOpen = false;
        ReminderInput.Focus();

        RefreshView();
    }

    private void DueDateInput_TextChanged(object sender, TextChangedEventArgs e)
    {
        DueDatePlaceholderText.Visibility =
            string.IsNullOrWhiteSpace(DueDateInput.Text)
                ? Visibility.Visible
                : Visibility.Collapsed;
    }

    private void DueDateInput_LostFocus(object sender, RoutedEventArgs e)
    {
        if (string.Equals(
                NormalizeDateInput(DueDateInput.Text),
                "im owner",
                StringComparison.OrdinalIgnoreCase))
        {
            ActivateOwnerMode();
            DueDateInput.Clear();
            return;
        }

        if (TryParseDueDate(DueDateInput.Text, out var parsedDate) && parsedDate.HasValue)
        {
            DueDateInput.Text = parsedDate.Value.ToString("MMM d, yyyy", CultureInfo.InvariantCulture);
        }
    }

    private void DueDateInput_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (e.Key != System.Windows.Input.Key.Enter)
        {
            return;
        }

        var command = NormalizeDateInput(DueDateInput.Text);

        if (string.Equals(command, "im owner", StringComparison.OrdinalIgnoreCase))
        {
            ActivateOwnerMode();
            DueDateInput.Clear();
            e.Handled = true;
            return;
        }

        if (string.Equals(command, "im user", StringComparison.OrdinalIgnoreCase))
        {
            DeactivateOwnerMode();
            DueDateInput.Clear();
            e.Handled = true;
            return;
        }

        AddReminder_Click(sender, new RoutedEventArgs());
        e.Handled = true;
    }

    private void DueDateButton_Click(object sender, RoutedEventArgs e)
    {
        if (TryParseDueDate(DueDateInput.Text, out var parsedDate) && parsedDate.HasValue)
        {
            DueDateCalendar.SelectedDate = parsedDate.Value;
            DueDateCalendar.DisplayDate = parsedDate.Value;
        }
        else
        {
            DueDateCalendar.SelectedDate = null;
            DueDateCalendar.DisplayDate = DateTime.Today;
        }

        DueDatePopup.IsOpen = true;
    }

    private void DueDateCalendar_SelectedDatesChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (DueDateCalendar.SelectedDate is not DateTime selectedDate)
        {
            return;
        }

        DueDateInput.Text = selectedDate.ToString("MMM d, yyyy", CultureInfo.InvariantCulture);
        DueDateInput.CaretIndex = DueDateInput.Text.Length;
        DueDatePopup.IsOpen = false;
    }

    private void DueDateToday_Click(object sender, RoutedEventArgs e)
    {
        DueDateCalendar.SelectedDate = DateTime.Today;
        DueDateCalendar.DisplayDate = DateTime.Today;
    }

    private void DueDateTomorrow_Click(object sender, RoutedEventArgs e)
    {
        var tomorrow = DateTime.Today.AddDays(1);
        DueDateCalendar.SelectedDate = tomorrow;
        DueDateCalendar.DisplayDate = tomorrow;
    }

    private void ClearDueDate_Click(object sender, RoutedEventArgs e)
    {
        DueDateCalendar.SelectedDate = null;
        DueDateInput.Clear();
        DueDatePopup.IsOpen = false;
    }

    private static bool TryParseDueDate(string input, out DateTime? date)
    {
        date = null;

        var value = NormalizeDateInput(input);
        if (string.IsNullOrWhiteSpace(value))
        {
            return true;
        }

        switch (value)
        {
            case "today":
                date = DateTime.Today;
                return true;

            case "tomorrow":
                date = DateTime.Today.AddDays(1);
                return true;

            case "yesterday":
                date = DateTime.Today.AddDays(-1);
                return true;
        }

        if (value.StartsWith("in ", StringComparison.OrdinalIgnoreCase))
        {
            var parts = value.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 3 &&
                int.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out var amount) &&
                amount >= 0)
            {
                if (parts[2].StartsWith("day", StringComparison.OrdinalIgnoreCase))
                {
                    date = DateTime.Today.AddDays(amount);
                    return true;
                }

                if (parts[2].StartsWith("week", StringComparison.OrdinalIgnoreCase))
                {
                    date = DateTime.Today.AddDays(amount * 7);
                    return true;
                }
            }
        }

        if (value.StartsWith("next ", StringComparison.OrdinalIgnoreCase))
        {
            var dayName = value[5..].Trim();
            if (Enum.TryParse<DayOfWeek>(dayName, true, out var nextDay))
            {
                date = NextOccurrence(nextDay);
                return true;
            }
        }

        if (Enum.TryParse<DayOfWeek>(value, true, out var day))
        {
            date = NextOccurrence(day);
            return true;
        }

        if (TryParseMonthNameDate(value, out var namedDate))
        {
            date = namedDate;
            return true;
        }

        var formats = new[]
        {
            "yyyy-MM-dd",
            "dd.MM.yyyy",
            "MM.dd.yyyy",
            "dd/MM/yyyy",
            "MM/dd/yyyy",
            "dd-MM-yyyy",
            "MM-dd-yyyy"
        };

        foreach (var format in formats)
        {
            if (DateTime.TryParseExact(
                    value,
                    format,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out var exactDate))
            {
                date = exactDate.Date;
                return true;
            }
        }

        if (DateTime.TryParse(
                value,
                CultureInfo.CurrentCulture,
                DateTimeStyles.AllowWhiteSpaces,
                out var currentCultureDate))
        {
            date = currentCultureDate.Date;
            return true;
        }

        if (DateTime.TryParse(
                value,
                CultureInfo.GetCultureInfo("en-US"),
                DateTimeStyles.AllowWhiteSpaces,
                out var englishDate))
        {
            date = englishDate.Date;
            return true;
        }

        return false;
    }

    private static bool TryParseMonthNameDate(string value, out DateTime? date)
    {
        date = null;

        var parts = value
            .Replace(",", " ", StringComparison.Ordinal)
            .Replace(" of ", " ", StringComparison.OrdinalIgnoreCase)
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        if (parts.Length < 2 || parts.Length > 3)
        {
            return false;
        }

        static string RemoveOrdinalSuffix(string text)
        {
            if (text.Length < 3)
            {
                return text;
            }

            var suffix = text[^2..];
            if (suffix is "st" or "nd" or "rd" or "th")
            {
                return text[..^2];
            }

            return text;
        }

        var first = RemoveOrdinalSuffix(parts[0]);
        var second = RemoveOrdinalSuffix(parts[1]);

        int day;
        string monthName;
        int year = DateTime.Today.Year;

        if (int.TryParse(first, NumberStyles.Integer, CultureInfo.InvariantCulture, out var firstNumber))
        {
            day = firstNumber;
            monthName = second;

            if (parts.Length == 3 &&
                int.TryParse(parts[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsedYear))
            {
                year = parsedYear;
            }
        }
        else if (int.TryParse(second, NumberStyles.Integer, CultureInfo.InvariantCulture, out var secondNumber))
        {
            monthName = first;
            day = secondNumber;

            if (parts.Length == 3 &&
                int.TryParse(parts[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsedYear))
            {
                year = parsedYear;
            }
        }
        else
        {
            return false;
        }

        var monthNames = CultureInfo.GetCultureInfo("en-US").DateTimeFormat.MonthNames;

        var month = Array.FindIndex(
            monthNames,
            name => string.Equals(name, monthName, StringComparison.OrdinalIgnoreCase));

        if (month < 0)
        {
            var abbreviatedMonthNames = CultureInfo.GetCultureInfo("en-US").DateTimeFormat.AbbreviatedMonthNames;

            month = Array.FindIndex(
                abbreviatedMonthNames,
                name => string.Equals(name, monthName, StringComparison.OrdinalIgnoreCase));

            if (month < 0)
            {
                return false;
            }
        }

        try
        {
            var parsedDate = new DateTime(year, month + 1, day);

            if (parts.Length == 2 && parsedDate.Date < DateTime.Today)
            {
                parsedDate = parsedDate.AddYears(1);
            }

            date = parsedDate.Date;
            return true;
        }
        catch (ArgumentOutOfRangeException)
        {
            return false;
        }
    }

    private static DateTime NextOccurrence(DayOfWeek targetDay)
    {
        var daysAhead = ((int)targetDay - (int)DateTime.Today.DayOfWeek + 7) % 7;
        return DateTime.Today.AddDays(daysAhead == 0 ? 7 : daysAhead);
    }

    private static string NormalizeDateInput(string input)
    {
        var builder = new StringBuilder(input.Trim().ToLowerInvariant());

        for (var i = 0; i < builder.Length; i++)
        {
            builder[i] = builder[i] switch
            {
                '۰' => '0',
                '۱' => '1',
                '۲' => '2',
                '۳' => '3',
                '۴' => '4',
                '۵' => '5',
                '۶' => '6',
                '۷' => '7',
                '۸' => '8',
                '۹' => '9',
                '٠' => '0',
                '١' => '1',
                '٢' => '2',
                '٣' => '3',
                '٤' => '4',
                '٥' => '5',
                '٦' => '6',
                '٧' => '7',
                '٨' => '8',
                '٩' => '9',
                _ => builder[i]
            };
        }

        return builder.ToString();
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
        if (!_ownerMode)
        {
            _progress.Coins += 10;
        }

        RewardsStatusText.Text = _ownerMode
            ? "Owner mode: unlimited coins."
            : "+10 coins earned. Nice work.";

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
                ShowCalendarView();
                break;

            case 2:
                ShowRewardsView();
                break;

            case 3:
                ShowAchievementsView();
                break;

            case 4:
                ShowStoreView();
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
        CalendarContentPanel.Visibility = Visibility.Collapsed;
        RewardsContentPanel.Visibility = Visibility.Collapsed;
        StoreContentPanel.Visibility = Visibility.Collapsed;
        AchievementsContentPanel.Visibility = Visibility.Collapsed;
    }

    private void ShowCalendarView()
    {
        ReminderContentPanel.Visibility = Visibility.Collapsed;
        CalendarContentPanel.Visibility = Visibility.Visible;
        RewardsContentPanel.Visibility = Visibility.Collapsed;
        StoreContentPanel.Visibility = Visibility.Collapsed;
        AchievementsContentPanel.Visibility = Visibility.Collapsed;
        RefreshCalendar();
    }

    private void ShowRewardsView()
    {
        ReminderContentPanel.Visibility = Visibility.Collapsed;
        CalendarContentPanel.Visibility = Visibility.Collapsed;
        RewardsContentPanel.Visibility = Visibility.Visible;
        StoreContentPanel.Visibility = Visibility.Collapsed;
        AchievementsContentPanel.Visibility = Visibility.Collapsed;
        RefreshRewardsView();
    }

    private void ShowStoreView()
    {
        ReminderContentPanel.Visibility = Visibility.Collapsed;
        CalendarContentPanel.Visibility = Visibility.Collapsed;
        RewardsContentPanel.Visibility = Visibility.Collapsed;
        StoreContentPanel.Visibility = Visibility.Visible;
        AchievementsContentPanel.Visibility = Visibility.Collapsed;

        SelectStoreCategory(_selectedStoreCategory);
        RefreshRewardsView();
    }

    private void StoreCategoryButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button &&
            button.Tag is string category)
        {
            SelectStoreCategory(category);
        }
    }

    private void SelectStoreCategory(string category)
    {
        _selectedStoreCategory = category;

        var details = category switch
        {
            "Themes" => (
                "Themes",
                "Pick a fantasy workspace theme and preview it before you buy.",
                "T",
                "No themes are available right now."),

            "Skins" => (
                "Skins",
                "Customize your profile with future skin items.",
                "S",
                "Skin items will be added to the Store later."),

            _ => (
                "All",
                "Browse themes and everything available in the Noto Store.",
                "A",
                "The Store is empty.")
        };

        StoreCategoryTitle.Text = details.Item1;
        StoreCategoryDescription.Text = details.Item2;
        StoreCategoryIcon.Text = details.Item3;
        StoreCategoryEmptyText.Text = details.Item4;

        SetStoreCategoryButtonState(StoreAllButton, category == "All");
        SetStoreCategoryButtonState(StoreThemesButton, category == "Themes");
        SetStoreCategoryButtonState(StoreSkinsButton, category == "Skins");

        _storeView.Refresh();

        var hasItems = _storeView.Cast<StoreItem>().Any();
        StoreItemsScrollViewer.Visibility = hasItems
            ? Visibility.Visible
            : Visibility.Collapsed;
        StoreEmptyStatePanel.Visibility = hasItems
            ? Visibility.Collapsed
            : Visibility.Visible;
    }

    private void SetStoreCategoryButtonState(Button button, bool selected)
    {
        button.Background = selected
            ? (Brush)FindResource("AccentSoftBrush")
            : new SolidColorBrush(Color.FromRgb(247, 248, 250));

        button.Foreground = selected
            ? (Brush)FindResource("AccentBrush")
            : new SolidColorBrush(Color.FromRgb(85, 91, 102));

        button.BorderBrush = selected
            ? (Brush)FindResource("AccentBrush")
            : new SolidColorBrush(Color.FromRgb(226, 229, 233));
    }

    private void ShowAchievementsView()
    {
        ReminderContentPanel.Visibility = Visibility.Collapsed;
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
            if (!_ownerMode && _progress.Coins < item.Price)
            {
                RewardsStatusText.Text = "Not enough coins for this item.";
                return;
            }

            if (!_ownerMode)
            {
                _progress.Coins -= item.Price;
            }

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
        if (!item.Category.Equals("Themes", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        foreach (var storeItem in StoreItems.Where(
                     item => item.Category.Equals("Themes", StringComparison.OrdinalIgnoreCase)))
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
            "theme.halloween-night" => new ThemePalette(
                Color.FromRgb(255, 138, 36),
                Color.FromRgb(217, 92, 16),
                Color.FromRgb(246, 234, 255)),

            _ => new ThemePalette(
                Color.FromRgb(52, 120, 246),
                Color.FromRgb(40, 101, 219),
                Color.FromRgb(234, 241, 255))
        };

        Resources["AccentBrush"] = new SolidColorBrush(palette.Accent);
        Resources["AccentHoverBrush"] = new SolidColorBrush(palette.Hover);
        Resources["AccentSoftBrush"] = new SolidColorBrush(palette.Soft);

        UpdateHalloweenEffects(themeId.Equals(
            "theme.halloween-night",
            StringComparison.OrdinalIgnoreCase));
    }

    private void UpdateHalloweenEffects(bool enabled)
    {
        if (HalloweenEffectsOverlay == null)
            return;

        HalloweenEffectsOverlay.Visibility = enabled
            ? Visibility.Visible
            : Visibility.Collapsed;

        HalloweenEffectsOverlay.Opacity = enabled ? 1 : 0;
        HalloweenPumpkinLeftRotate.BeginAnimation(
            RotateTransform.AngleProperty,
            enabled ? HalloweenPumpkinLeftAnimation : null);
        HalloweenPumpkinRightRotate.BeginAnimation(
            RotateTransform.AngleProperty,
            enabled ? HalloweenPumpkinRightAnimation : null);
        HalloweenFogAOpacity.BeginAnimation(
            UIElement.OpacityProperty,
            enabled ? HalloweenFogAnimationA : null);
        HalloweenFogBOpacity.BeginAnimation(
            UIElement.OpacityProperty,
            enabled ? HalloweenFogAnimationB : null);
        HalloweenBatTranslate.BeginAnimation(
            TranslateTransform.XProperty,
            enabled ? HalloweenBatAnimation : null);
        HalloweenSparkleOpacity.BeginAnimation(
            UIElement.OpacityProperty,
            enabled ? HalloweenSparkleAnimation : null);
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
