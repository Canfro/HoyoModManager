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
            string gamePath = GetGamePath(SelectedGame);
            if (string.IsNullOrWhiteSpace(gamePath) || !Directory.Exists(gamePath))
            {
                ShowMessage("Error", "Make sure to set game path correctly in config.json");
                return;
            }
            
            string basePath = Path.Combine(gamePath, "HoyoModManager");
            if (!Directory.Exists(basePath))
            {
                ShowMessage("Error", $"Directory {basePath} not found, make sure to create the necessary folders by pressing the 'Create Folders' button");
                return;
            }
            
            Characters.Clear();

            ObservableCollection<string> newCharacters = SelectedGame switch
            {
                "Genshin Impact" => GamesData.GenshinNames,
                "Honkai: Star Rail" => GamesData.StarRailNames,
                "Zenless Zone Zero" => GamesData.ZenlessNames,
                _ => [],
            };
            
            foreach (string character in newCharacters)
            {
                string characterPath = Path.Combine(basePath, character);
                int modCount = Directory.Exists(characterPath)
                    ? Directory.GetDirectories(characterPath).Length
                    : 0;
                
                Characters.Add($"{character} ({modCount})");
            }

            if (Characters.Count > 0)
            {
                SelectedCharacter = Characters[0];
            }
        });
    }

    private void UpdateMods()
    {
        string gamePath = GetGamePath(SelectedGame);
        if (string.IsNullOrWhiteSpace(gamePath) || !Directory.Exists(gamePath))
        {
            ShowMessage("Error", "Make sure to set game path correctly in config.json");
            return;
        }
        
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
        string gamePath = GetGamePath(SelectedGame);
        if (string.IsNullOrWhiteSpace(gamePath) || !Directory.Exists(gamePath))
        {
            ShowMessage("Error", "Make sure to set game path correctly in config.json");
            return;
        }
        
        string basePath = Path.Combine(gamePath, "HoyoModManager");
        Directory.CreateDirectory(basePath);
        
        ObservableCollection<string> newCharacters = SelectedGame switch
        {
            "Genshin Impact" => GamesData.GenshinNames,
            "Honkai: Star Rail" => GamesData.StarRailNames,
            "Zenless Zone Zero" => GamesData.ZenlessNames,
            _ => [],
        };

        foreach (string character in newCharacters)
        {
            string characterName = Regex.Replace(character, @"\s\(\d+\)$", "");
            string characterPath = Path.Combine(basePath, characterName);
            Directory.CreateDirectory(characterPath);
        }
        
        UpdateCharacters();
    }

    private static string GetGamePath(string game)
    {
        return game switch
        {
            "Genshin Impact" => Config.Current.Paths["GenshinPath"],
            "Honkai: Star Rail" => Config.Current.Paths["StarRailPath"],
            "Zenless Zone Zero" => Config.Current.Paths["ZenlessPath"],
            _ => throw new ArgumentOutOfRangeException(),
        };
    }
    
    private void ToggleMod(string modName)
    {
        string gamePath = GetGamePath(SelectedGame);
        if (string.IsNullOrWhiteSpace(gamePath) || !Directory.Exists(gamePath))
        {
            ShowMessage("Error", "Make sure to set game path correctly in config.json");
            return;
        }
        
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
        if (string.IsNullOrWhiteSpace(gamePath) || !Directory.Exists(gamePath))
        {
            ShowMessage("Error", "Make sure to set game path correctly in config.json");
            return;
        }
        
        string basePath = Path.Combine(gamePath, "HoyoModManager");
        if (!Directory.Exists(basePath))
        {
            ShowMessage("Error", $"Directory {basePath} not found, make sure to create the necessary folders by pressing the 'Create Folders' button");
            return; 
        }
        
        foreach (string character in Characters)
        {
            string characterName = Regex.Replace(character, @"\s\(\d+\)$", "");
            string characterPath = Path.Combine(basePath, characterName);

            if (!Directory.Exists(characterPath))
            {
                ShowMessage("Error", $"Directory {characterPath} not found, make sure to create the necessary folders by pressing the 'Create Folders' button");
                return;
            }

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
