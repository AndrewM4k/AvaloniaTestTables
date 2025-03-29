using Avalonia.Controls;
using AvaloniaTestTables.Context;
using AvaloniaTestTables.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ExcelDataReader;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Linq;Ы
using System.Threading.Tasks;

namespace AvaloniaTestTables.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly AppDbContext _dbContext;

    [ObservableProperty]
    private User _currentUser;

    [ObservableProperty]
    private ObservableCollection<Mode> _modes;

    [ObservableProperty]
    private ObservableCollection<Step> _steps;

    [ObservableProperty]
    private Mode _selectedMode;

    public MainWindowViewModel(AppDbContext dbContext)
    {
        _dbContext = dbContext;
        LoadData();
    }

    private void LoadData()
    {
        _dbContext.Modes.Load();
        _dbContext.Steps.Load();

        Modes = new ObservableCollection<Mode>(_dbContext.Modes.Local.ToObservableCollection());
        Steps = new ObservableCollection<Step>(_dbContext.Steps.Local.ToObservableCollection());

        if (Modes.Count != 0)
            SelectedMode = Modes[0];

        OnPropertyChanged(nameof(Modes));
        OnPropertyChanged(nameof(Steps));
    }

    [RelayCommand]
    private void AddMode()
    {
        var newMode = new Mode()
        {
            Name = "New Mode",
            MaxBottleNumber = 0,
            MaxUsedTips = 0
        };
        _dbContext.Modes.Add(newMode);
        _dbContext.SaveChanges();
        Modes.Add(newMode);

        if (Modes.Count != 0)
            SelectedMode = newMode;
    }

    [RelayCommand]
    private void DeleteMode(int id)
    {
        var mode = _dbContext.Modes.FirstOrDefault(x => x.ID == id);
        if (mode != null)
        {
            _dbContext.Modes.Remove(mode);
            _dbContext.SaveChanges();
            Modes.Remove(mode);
        }
    }

    [RelayCommand]
    private void SelectMode(int id)
    {
        var mode = _dbContext.Modes.FirstOrDefault(x => x.ID == id);
        if (mode != null)
        {
            SelectedMode = mode;
        }
    }

    [RelayCommand]
    private void AddStep()
    {
        var newStep = new Step()
        {
            ModeId = SelectedMode.ID,
            Timer = 0,
            Destination = "Default",
            Speed = 0,
            Type = "None",
            Volume = 0
        };
        _dbContext.Steps.Add(newStep);
        _dbContext.SaveChanges();
        Steps.Add(newStep);
    }

    [RelayCommand]
    private void DeleteStep(int id)
    {
        var step = _dbContext.Steps.FirstOrDefault(x => x.ID == id); 
        if (step != null)
        {
            _dbContext.Steps.Remove(step);
            _dbContext.SaveChanges();
            Steps.Remove(step);
        }
    }

    [RelayCommand]
    private async Task ImportFromExcel()
    {
        try
        {
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

            var fileDialog = new OpenFileDialog();
            fileDialog.Filters.Add(new FileDialogFilter { Name = "Excel Files", Extensions = { "xlsx", "xls" } });
            var result = await fileDialog.ShowAsync(new Window());

            if (result != null && result.Length > 0)
            {
                using var stream = File.Open(result[0], FileMode.Open, FileAccess.Read);
                using var reader = ExcelReaderFactory.CreateReader(stream);

                var dataSet = reader.AsDataSet(new ExcelDataSetConfiguration()
                {
                    ConfigureDataTable = (_) => new ExcelDataTableConfiguration()
                    {
                        UseHeaderRow = true
                    }
                });

                _dbContext.Modes.RemoveRange(_dbContext.Modes);
                _dbContext.Steps.RemoveRange(_dbContext.Steps);

                if (dataSet.Tables.Count > 0)
                {
                    foreach (DataRow row in dataSet.Tables[0].Rows)
                    {
                        var mode = new Mode
                        {
                            ID = SafeParseInt(row["ID"]),
                            Name = row["Name"]?.ToString() ?? string.Empty,
                            MaxBottleNumber = SafeParseInt(row["MaxBottleNumber"]),
                            MaxUsedTips = SafeParseInt(row["MaxUsedTips"])
                        };
                        var a = _dbContext.Modes.Add(mode);
                    }
                    await _dbContext.SaveChangesAsync();
                }

                if (dataSet.Tables.Count > 1)
                {
                    var modes = _dbContext.Modes.ToList();

                    foreach (DataRow row in dataSet.Tables[1].Rows)
                    {
                        var step = new Step
                        {
                            ModeId = SafeParseInt(row["ModeId"]),
                            Timer = SafeParseInt(row["Timer"]),
                            Destination = row["Destination"]?.ToString() ?? "NONE",
                            Speed = SafeParseInt(row["Speed"]),
                            Type = row["Type"]?.ToString() ?? "UNKNOWN",
                            Volume = SafeParseDecimal(row["Volume"])
                        };

                        if (!modes.Any(m => m.ID == step.ModeId))
                        {
                            Console.WriteLine($"Warning: ModeId {step.ModeId} not found");
                            continue;
                        }
                        _dbContext.Steps.Add(step);

                    }
                    await _dbContext.SaveChangesAsync();
                }

                LoadData();
            }
            await _dbContext.SaveChangesAsync();

            LoadData();

            Modes = new ObservableCollection<Mode>(_dbContext.Modes.Include(m => m.Steps).ToList());
            Steps = new ObservableCollection<Step>(_dbContext.Steps.ToList());

            OnPropertyChanged(nameof(Modes));
            OnPropertyChanged(nameof(Steps));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error importing Excel: {ex.Message}");
        }
    }

    private int SafeParseInt(object value)
    {
        if (value == null || value == DBNull.Value || value.ToString() == "NONE")
            return 0;
        return int.TryParse(value.ToString(), out int result) ? result : 0;
    }

    private decimal SafeParseDecimal(object value)
    {
        if (value == null || value == DBNull.Value || value.ToString() == "NONE")
            return 0m;
        return decimal.TryParse(value.ToString(), out decimal result) ? result : 0m;
    }
    [RelayCommand]
    private void SaveChanges()
    {
        _dbContext.SaveChanges();
    }
}
