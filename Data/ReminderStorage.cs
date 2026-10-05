using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using SimpleReminders.Models;

namespace SimpleReminders.Data;

public sealed class ReminderStorage
{
    private readonly string _filePath;
    private readonly JsonSerializerOptions _options = new()
    {
        WriteIndented = true
    };

    public ReminderStorage()
    {
        var folder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "SimpleReminders");

        Directory.CreateDirectory(folder);
        _filePath = Path.Combine(folder, "data.json");
    }

    public StorageData Load()
    {
        try
        {
            if (!File.Exists(_filePath))
            {
                return new StorageData();
            }

            var json = File.ReadAllText(_filePath);

            return JsonSerializer.Deserialize<StorageData>(json, _options)
                   ?? new StorageData();
        }
        catch
        {
            return new StorageData();
        }
    }

    public void Save(
        IEnumerable<Reminder> reminders,
        IEnumerable<ReminderList> lists,
        UserProgress progress)
    {
        var data = new StorageData
        {
            Reminders = reminders.ToList(),
            Lists = lists.ToList(),
            Progress = progress
        };

        var json = JsonSerializer.Serialize(data, _options);
        File.WriteAllText(_filePath, json);
    }
}

public sealed class StorageData
{
    public List<Reminder> Reminders { get; set; } = [];

    public List<ReminderList> Lists { get; set; } = [];

    public UserProgress Progress { get; set; } = new();
}
