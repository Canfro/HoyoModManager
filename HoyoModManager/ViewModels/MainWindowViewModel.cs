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
    public ObservableCollection<string> ModDirs { get; set; } = [];
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
    
    private string _selectedModDir;
    public string SelectedModDir
    {
        get => _selectedModDir;
        set
        {
            string selectedModDirName = Regex.Replace(value, @"\s\(\d+\)$", "");
            this.RaiseAndSetIfChanged(ref _selectedModDir, selectedModDirName);
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
            
            ModDirs.Clear();

            ObservableCollection<string> newModDirs = SelectedGame switch
            {
                "Genshin Impact" => GamesData.GenshinNames,
                "Honkai: Star Rail" => GamesData.StarRailNames,
                "Zenless Zone Zero" => GamesData.ZenlessNames,
                _ => [],
            };
            
            foreach (string modDir in newModDirs)
            {
                string modDirPath = Path.Combine(basePath, modDir);
                int modCount = Directory.Exists(modDirPath)
                    ? Directory.GetDirectories(modDirPath).Length
                    : 0;
                
                ModDirs.Add($"{modDir} ({modCount})");
            }

            if (ModDirs.Count > 0)
            {
                SelectedModDir = ModDirs[0];
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
        
        string modDirPath = Path.Combine(modsPath, "HoyoModManager", SelectedModDir);
        
        Dispatcher.UIThread.Post(() =>
        {
            Mods.Clear();
        
            foreach (string dir in Directory.GetDirectories(modDirPath))
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
        
        ObservableCollection<string> newModDirs = SelectedGame switch
        {
            "Genshin Impact" => GamesData.GenshinNames,
            "Honkai: Star Rail" => GamesData.StarRailNames,
            "Zenless Zone Zero" => GamesData.ZenlessNames,
            _ => [],
        };

        foreach (string modDir in newModDirs)
        {
            string modDirName = Regex.Replace(modDir, @"\s\(\d+\)$", "");
            string modDirPath = Path.Combine(basePath, modDirName);
            Directory.CreateDirectory(modDirPath);
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
        
        string modDirPath = Path.Combine(modsPath, "HoyoModManager", SelectedModDir);
        string modPath = Path.Combine(modDirPath, modName);

        string newModPath = modName.StartsWith("DISABLED ")
            ? Path.Combine(modDirPath, modName.Replace("DISABLED ", ""))
            : Path.Combine(modDirPath, "DISABLED " + modName);

        Directory.Move(modPath, newModPath);
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
        
        foreach (string modDir in ModDirs)
        {
            string modDirName = Regex.Replace(modDir, @"\s\(\d+\)$", "");
            string modDirPath = Path.Combine(basePath, modDirName);

            if (!Directory.Exists(modDirPath))
            {
                ShowMessage("Error", $"Directory {modDirPath} not found, make sure to create the necessary folders by pressing the 'Create Folders' button");
                return;
            }

            foreach (string modPath in Directory.GetDirectories(modDirPath))
            {
                string modName = Path.GetFileName(modPath);
                if (modName.StartsWith("DISABLED ")) continue;
                string newModPath = Path.Combine(modDirPath, "DISABLED " + modName);
                Directory.Move(modPath, newModPath);
            }
            
            string[] mods = Directory.GetDirectories(modDirPath);
            if (mods.Length == 0) continue;

            Random random = new();
            string randomModName = mods[random.Next(mods.Length)];
            string randomModPath = Path.Combine(modDirPath, randomModName);
            string randomNewModPath = Path.Combine(modDirPath, randomModName.Replace("DISABLED ", ""));
            Directory.Move(randomModPath, randomNewModPath);
        }
        
        UpdateMods();
    }
    
    public void PermanentToggles()
    {
        string gamePath = GetGamePath(SelectedGame);
        string modsPath = Path.Combine(gamePath, "Mods");
        string basePath =  Path.Combine(modsPath, "HoyoModManager");
        string userIniPath = Path.Combine(gamePath, "d3dx_user.ini");

        if (!File.Exists(userIniPath))
        {
            ShowMessage("Error", "'d3dx_user.ini' file not found.");
            return;
        }
        
        string[] userIniLines = File.ReadAllLines(userIniPath);
        Regex regex = new(@"^\$\\(.+?)\\([^\s=?\\]+)\s*=\s*(.+)$");
        
        List<string> notFoundIniPaths = [];
        List<string> notFoundVariables = [];
        
        foreach (string line in userIniLines)
        {
            Match match = regex.Match(line.Trim());
            if (!match.Success) continue;
            
            string iniRelativePath = match.Groups[1].Value.Replace("\\", Path.DirectorySeparatorChar.ToString());
            string variable = match.Groups[2].Value;
            string value = match.Groups[3].Value;
            string iniPath = ResolveActualPath(gamePath, iniRelativePath);
            
            if (!File.Exists(iniPath))
            {
                bool namespaceFound = false;
                foreach (string modDir in ModDirs)
                {
                    if (namespaceFound) break;
                    
                    string modDirName = Regex.Replace(modDir, @"\s\(\d+\)$", "");
                    string modDirPath = Path.Combine(basePath, modDirName);
                    
                    if (!Directory.Exists(modDirPath))
                    {
                        ShowMessage("Error", $"Directory {modDirPath} not found, make sure to create the necessary folders by pressing the 'Create Folders' button");
                        return;
                    }

                    string[] modIniFiles =  Directory.GetFiles(modDirPath, "*.ini", SearchOption.AllDirectories);
                    foreach (string modIniFile in modIniFiles)
                    {
                        string[] modIniLines = File.ReadAllLines(modIniFile);

                        if (!modIniLines.Any(modIniLine => modIniLine.Contains(
                                $"namespace = {iniRelativePath.Replace("/", "\\")}",
                                StringComparison.OrdinalIgnoreCase))) continue;

                        namespaceFound = true;
                        iniPath = modIniFile;
                        break;
                    }
                }

                if (!namespaceFound)
                {
                    notFoundIniPaths.Add($"File or namespace not found: {iniPath}");
                    continue;
                }
            }
            
            Regex regexLine = new($@"^\s*global\s+persist\s+\${Regex.Escape(variable)}\b", RegexOptions.IgnoreCase);
            
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
        
        if (notFoundIniPaths.Count > 0) ShowMessage("Warning", string.Join("\n\n", notFoundIniPaths));
        if (notFoundVariables.Count > 0) ShowMessage("Warning", string.Join("\n\n", notFoundVariables));
        ShowMessage("Success", "Toggles updated!");
    }
    
    private static string ResolveActualPath(string basePath, string relativePath)
    {
        string[] parts = relativePath.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        string currentPath = basePath;

        foreach (string part in parts)
        {
            if (!Directory.Exists(currentPath) && !File.Exists(currentPath))
            {
                return relativePath;
            }
            
            string? match = Directory
                .EnumerateFileSystemEntries(currentPath)
                .FirstOrDefault(entry => string.Equals(Path.GetFileName(entry), part, StringComparison.OrdinalIgnoreCase));

            if (match == null)
            {
                return relativePath;
            }

            currentPath = match;
        }

        return currentPath;
    }
}
