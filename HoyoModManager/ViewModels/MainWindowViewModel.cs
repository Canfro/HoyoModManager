using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.RegularExpressions;
using Avalonia.Extensions.Controls;
using Avalonia.Threading;
using HoyoModManager.Models;
using ReactiveUI;
using Path = System.IO.Path;

namespace HoyoModManager.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    public static ObservableCollection<string> Games => GamesData.Games;
    public ObservableCollection<string> Characters { get; set; } = [];
    public ObservableCollection<string> Mods { get; set; } = [];
    
    private string _selectedGame = "Genshin Impact";
    public string SelectedGame
    {
        get => _selectedGame;
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedGame, value);
            UpdateCharacters();
        }
    }
    
    private string _selectedCharacter;
    public string SelectedCharacter
    {
        get => _selectedCharacter;
        set
        {
            string selectedCharacterName = Regex.Replace(value, @"\s\(\d+\)$", "");
            this.RaiseAndSetIfChanged(ref _selectedCharacter, selectedCharacterName);
            UpdateMods();
        }
    }
    
    private string _selectedMod;
    public string SelectedMod
    {
        get => _selectedMod;
        set
        {
            if (value == _selectedMod) return;
            this.RaiseAndSetIfChanged(ref _selectedMod, value);
            ToggleMod(value);
        }
    }
    
    public MainWindowViewModel()
    {
        Config.Load();
        UpdateCharacters();
    }

    private static void ShowMessage(string title, string message)
    {
        Dispatcher.UIThread.Post(() =>
        {
            MessageBox.Show(title, message, MessageBoxButtons.Ok);
        });
    }
    
    private void UpdateCharacters()
    {
        Dispatcher.UIThread.Post(() =>
        {
            Characters.Clear();

            ObservableCollection<string> newCharacters = SelectedGame switch
            {
                "Genshin Impact" => GamesData.GenshinNames,
                "Honkai: Star Rail" => GamesData.StarRailNames,
                "Zenless Zone Zero" => GamesData.ZenlessNames,
                _ => [],
            };
            
            string gamePath = GetGamePath(SelectedGame);
            string basePath = Path.Combine(gamePath, "HoyoModManager");
            
            foreach (string character in newCharacters)
            {
                string characterPath = Path.Combine(basePath, character);
                int modCount = Directory.GetDirectories(characterPath).Length;
                Characters.Add($"{character} ({modCount.ToString()})");
            }
        });
    }

    private void UpdateMods()
    {
        string gamePath = GetGamePath(SelectedGame);
        if (string.IsNullOrWhiteSpace(gamePath) || !Directory.Exists(gamePath)) return;
        string characterPath = Path.Combine(gamePath, "HoyoModManager", SelectedCharacter);
        
        Dispatcher.UIThread.Post(() =>
        {
            Mods.Clear();
        
            foreach (string dir in Directory.GetDirectories(characterPath))
            {
                Mods.Add(Path.GetFileNameWithoutExtension(dir));
            }
        });
    }

    public void CreateFolders()
    {
        if (string.IsNullOrWhiteSpace(GetGamePath(SelectedGame)) || !Directory.Exists(GetGamePath(SelectedGame)))
        {
            ShowMessage("Error", "Make sure the path is properly set in config.json");
            return;
        }

        string gamePath = GetGamePath(SelectedGame);
        string basePath = Path.Combine(gamePath, "HoyoModManager");

        if (!Directory.Exists(basePath))
        {
            Directory.CreateDirectory(basePath);
        }

        foreach (string character in Characters)
        {
            string characterName = Regex.Replace(character, @"\s\(\d+\)$", "");
            string characterPath = Path.Combine(basePath, characterName);

            if (!Directory.Exists(characterPath))
            {
                Directory.CreateDirectory(characterPath);
            }
        }
        
        UpdateCharacters();
    }

    private static string GetGamePath(string game) => game switch
    {
        "Genshin Impact" => Config.Current.Paths["GenshinPath"],
        "Honkai: Star Rail" => Config.Current.Paths["StarRailPath"],
        "Zenless Zone Zero" => Config.Current.Paths["ZenlessPath"],
        _ => throw new ArgumentOutOfRangeException(),
    };
    
    private void ToggleMod(string modName)
    {
        string gamePath = GetGamePath(SelectedGame);
        string characterPath = Path.Combine(gamePath, "HoyoModManager", SelectedCharacter);
        string modPath = Path.Combine(characterPath, modName);

        string newModPath = modName.StartsWith("DISABLED ")
            ? Path.Combine(characterPath, modName.Replace("DISABLED ", ""))
            : Path.Combine(characterPath, "DISABLED " + modName);

        Directory.Move(modPath, newModPath);

        /*
        foreach (string iniPath in Directory.GetFiles(newModPath, "*.ini", SearchOption.AllDirectories))
        {
            string iniName = Path.GetFileName(iniPath);
            string iniDir = Path.GetDirectoryName(iniPath)!;
            
            string newIniPath = iniName.StartsWith("DISABLED_") 
                ? Path.Combine(iniDir, iniName.Replace("DISABLED_", ""))
                : Path.Combine(iniDir, "DISABLED_" + iniName);
            
            File.Move(iniPath, newIniPath);
        }
        */
        
        UpdateMods();
    }

    public void RandomizeMods()
    {
        string gamePath = GetGamePath(SelectedGame);
        
        foreach (string character in Characters)
        {
            string characterPath = Path.Combine(gamePath, "HoyoModManager", character);

            foreach (string modPath in Directory.GetDirectories(characterPath))
            {
                string modName = Path.GetFileName(modPath);
                if (modName.StartsWith("DISABLED ")) continue;
                string newModPath = Path.Combine(characterPath, "DISABLED " + modName);
                Directory.Move(modPath, newModPath);
            }
            
            string[] mods = Directory.GetDirectories(characterPath);
            if (mods.Length == 0) continue;

            Random random = new();
            string randomModName = mods[random.Next(mods.Length)];
            string randomModPath = Path.Combine(characterPath, randomModName);
            string randomNewModPath = Path.Combine(characterPath, randomModName.Replace("DISABLED ", ""));
            Directory.Move(randomModPath, randomNewModPath);
        }
        
        UpdateMods();
    }
}
