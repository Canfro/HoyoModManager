using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Avalonia.Controls;
using Avalonia.Threading;
using HoyoModManager.Models;
using ReactiveUI;
using MessageBox = HoyoModManager.Views.MessageBox;
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

    private readonly Window _window;
    
    public MainWindowViewModel(Window window)
    {
        _window = window;
        Config.Load();
        UpdateCharacters();
    }

    private void ShowMessage(string title, string message)
    {
        MessageBox popup = new(title, message);
        popup.ShowDialog(_window);
    }
    
    private void UpdateCharacters()
    {
        Dispatcher.UIThread.Post(() =>
        {
            string gamePath = GetGamePath(SelectedGame);
            string modsPath = Path.Combine(gamePath, "Mods");
            if (string.IsNullOrWhiteSpace(modsPath) || !Directory.Exists(modsPath))
            {
                ShowMessage("Error", "Make sure to set game path correctly in config.json");
                return;
            }
            
            string basePath = Path.Combine(modsPath, "HoyoModManager");
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
        string modsPath = Path.Combine(gamePath, "Mods");
        if (string.IsNullOrWhiteSpace(modsPath) || !Directory.Exists(modsPath))
        {
            ShowMessage("Error", "Make sure to set game path correctly in config.json");
            return;
        }
        
        string characterPath = Path.Combine(modsPath, "HoyoModManager", SelectedCharacter);
        
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
        string modsPath = Path.Combine(gamePath, "Mods");
        if (string.IsNullOrWhiteSpace(modsPath) || !Directory.Exists(modsPath))
        {
            ShowMessage("Error", "Make sure to set game path correctly in config.json");
            return;
        }
        
        string basePath = Path.Combine(modsPath, "HoyoModManager");
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
            "Genshin Impact" => Config.Current.Paths["PathGIMI"],
            "Honkai: Star Rail" => Config.Current.Paths["PathSRMI"],
            "Zenless Zone Zero" => Config.Current.Paths["PathZZMI"],
            _ => throw new ArgumentOutOfRangeException(),
        };
    }
    
    private void ToggleMod(string modName)
    {
        string gamePath = GetGamePath(SelectedGame);
        string modsPath = Path.Combine(gamePath, "Mods");
        if (string.IsNullOrWhiteSpace(modsPath) || !Directory.Exists(modsPath))
        {
            ShowMessage("Error", "Make sure to set game path correctly in config.json");
            return;
        }
        
        string characterPath = Path.Combine(modsPath, "HoyoModManager", SelectedCharacter);
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
        string modsPath = Path.Combine(gamePath, "Mods");
        if (string.IsNullOrWhiteSpace(modsPath) || !Directory.Exists(modsPath))
        {
            ShowMessage("Error", "Make sure to set game path correctly in config.json");
            return;
        }
        
        string basePath = Path.Combine(modsPath, "HoyoModManager");
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
    
    public void PermanentToggles()
    {
        string gamePath = GetGamePath(SelectedGame);
        string userIniPath = Path.Combine(gamePath, "d3dx_user.ini");

        if (!File.Exists(userIniPath))
        {
            ShowMessage("Error", "'d3dx_user.ini' file not found.");
            return;
        }
        
        string[] lines = File.ReadAllLines(userIniPath);
        Regex regex = new(@"^\$\\(.+?\.ini)\\([^\s=]+)\s*=\s*(.+)$");
        
        List<string> notFoundIniPaths = [];
        List<string> notFoundVariables = [];
        
        foreach (string line in lines)
        {
            Match match = regex.Match(line.Trim());
            if (!match.Success) continue;
            
            string iniRelativePath = match.Groups[1].Value.Replace("\\", Path.DirectorySeparatorChar.ToString());
            string variable = match.Groups[2].Value;
            string value = match.Groups[3].Value;
            string? iniPath = ResolveActualPath(gamePath, iniRelativePath);
            
            Regex regexLine = new($@"^\s*global\s+persist\s+\${Regex.Escape(variable)}\b", RegexOptions.IgnoreCase);
            
            if (iniPath == null)
            {
                notFoundIniPaths.Add($"Path could not be resolved for: {iniRelativePath}");
                continue;
            }
            if (!File.Exists(iniPath))
            {
                notFoundIniPaths.Add($"File not found: {iniPath}");
                continue;
            }
            
            string[] iniLines = File.ReadAllLines(iniPath);
            List<string> newLines = [];
            bool found = false;
            
            foreach (string iniLine in iniLines)
            {
                if (regexLine.Match(iniLine).Success)
                {
                    newLines.Add($"global persist ${variable} = {value}");
                    found = true;
                }
                else
                {
                    newLines.Add(iniLine);
                }
            }
            
            if (!found)
            {
                notFoundVariables.Add($"Variable {variable} not found in {iniPath}");
            }

            File.WriteAllLines(iniPath, newLines);
        }
        
        if (notFoundIniPaths.Count > 0) ShowMessage("Archivos no encontrados", string.Join("\n", notFoundIniPaths));
        if (notFoundVariables.Count > 0) ShowMessage("Variables no encontradas", string.Join("\n", notFoundVariables));
        ShowMessage("Success", "Toggles updated!");
    }
    
    private static string? ResolveActualPath(string basePath, string lowerCasePath)
    {
        string[] parts = lowerCasePath.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        string currentPath = basePath;

        foreach (string part in parts)
        {
            if (!Directory.Exists(currentPath)) return null;

            string? match = Directory
                .EnumerateFileSystemEntries(currentPath)
                .FirstOrDefault(entry => string.Equals(Path.GetFileName(entry), part, StringComparison.OrdinalIgnoreCase));

            if (match == null) return null;

            currentPath = match;
        }

        return currentPath;
    }
}
