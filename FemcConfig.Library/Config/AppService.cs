﻿using CommunityToolkit.Mvvm.ComponentModel;
using FemcConfig.Library.Common;
using FemcConfig.Library.Config.Models;
using FemcConfig.Library.Utils;

namespace FemcConfig.Library.Config;

public class AppService
{
    private AppContext? appContext;
    private readonly SavableFile<AppData> appData;

    public AppService()
    {
        var appDataDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        this.AppDataDir = Path.Join(appDataDir, "FemcConfigAdjuster");
        Directory.CreateDirectory(this.AppDataDir);

        var appDataFile = Path.Join(this.AppDataDir, "femc.json");
        this.appData = new(appDataFile);

        try
        {
            this.AutoInit();
        }
        catch (Exception) { }
    }

    public string AppDataDir { get; }

    public AppContext GetContext()
        => this.appContext ?? throw new Exception("App context not set.");

    public void Initialize(string reloadedDir)
    {
        //VERY IMPORTANT PLEASE UPDATE EVERY TIME TO MATCH LATEST FEMC MOD VERSION

        this.appData.Settings.ReloadedDir = reloadedDir;
        if (reloadedDir is null)
            throw new Exception("Reloaded Directory not found");
        var appConfigFile = Path.Join(reloadedDir, "Apps", "p3r.exe", "AppConfig.json");
        if (!File.Exists(appConfigFile))
        {
            throw new Exception("Failed to find Reloaded app config.");
        }

        var appConfig = new SavableFile<ReloadedAppConfig>(appConfigFile);
        var reloadedModsDir = Path.Join(reloadedDir, "Mods");

        // Verify FEMC mod install dir.
        // Manually search for FEMC DLL since folder name isn't constant.
        string? femcDir = null;
        foreach (var dir in Directory.EnumerateDirectories(reloadedModsDir))
        {
            var femcDll = Path.Join(dir, "p3rpc.femc.dll");
            if (File.Exists(femcDll))
            {
                femcDir = dir;
                break;
            }
        }

        // Find FEMC mod config file.
        var reloadedConfigsDir = Path.Join(reloadedDir, "User", "Mods");
        string? femcConfigFile = null;
        string? femcModConfigFile = null;
        foreach (var configDir in Directory.EnumerateDirectories(reloadedConfigsDir))
        {
            Console.WriteLine(configDir);
            var userConfigFile = Path.Join(configDir, "ModUserConfig.json");
            var userConfig = File.Exists(userConfigFile) ? JsonUtils.DeserializeFile<ReloadedModUserConfig>(userConfigFile) : null;

            if (userConfig is not null)
            {
                if(userConfig.ModId == Constants.FEMC_MOD_ID)
                {
                    femcConfigFile = Path.Join(configDir, "Config.json");
                    femcModConfigFile = Path.Join(reloadedModsDir, Constants.FEMC_MOD_ID, "ModConfig.json");
                    break;
                }
            }
        }

        if (femcConfigFile == null || File.Exists(femcConfigFile) == false)
        {
            throw new FemcNotFound();
        }

        string? movieConfigFile = null;
		if (appConfig.Settings.EnabledMods.Contains("Persona_3_Reload_Intro_Movies"))
		{
			foreach (var configDir in Directory.EnumerateDirectories(reloadedConfigsDir))
			{
				var userConfigFile = Path.Join(configDir, "ModUserConfig.json");

				if (File.Exists(userConfigFile))
				{
					var userConfig = JsonUtils.DeserializeFile<ReloadedModUserConfig>(userConfigFile);

					if (userConfig.ModId == Constants.MOVIE_MOD_ID)
					{
						movieConfigFile = Path.Join(configDir, "Config.json");
						break;
					}
				}
				else
				{
				}
			}
		}

        string femcModVersionStatus = "NotExecError";
        string? femcModVersion = null;
        //Check mod version.
        if (File.Exists(femcModConfigFile))
        {
            femcModVersion = JsonUtils.DeserializeFile<ModInfo>(femcModConfigFile).ModVersion;
            if (femcModVersion == Constants.FEMC_MOD_VER)
            {
                femcModVersionStatus = "SUPPORTED";
            }
            else
            {
                femcModVersionStatus = "UNSUPPORTED";
            }
        }
        else
        {
            femcModVersionStatus = "404FILENOTFOUND";
        }

        // Setup mod context.
        this.appContext = new()
        {
            ReloadedDir = reloadedDir,
            ModDir = femcDir,
            ReloadedAppConfig = appConfig,
            FemcConfig = Directory.Exists(femcDir) ? new(femcConfigFile) : null,
            FemcModVersion = femcModVersionStatus,
            MovieConfig = File.Exists(movieConfigFile) ? new(movieConfigFile) : null,
        };
    }

    // Automatically initialize by fetching Reloaded path from
    // ENV var.
    private void AutoInit()
    {
        if (this.appData.Settings.ReloadedDir != null)
        {
            this.Initialize(this.appData.Settings.ReloadedDir);
        }

        else if (Environment.GetEnvironmentVariable("RELOADEDIIMODS") is string reloadedModsDir)
        {
            var reloadedDir = Path.GetDirectoryName(reloadedModsDir)!;
            this.Initialize(reloadedDir);
        }
    }
}

public class FemcNotFound : Exception
{
    public FemcNotFound() : base("Failed to find FEMC config file. Please launch the game once with the mod enabled.")
    {
    }
}

public partial class AppData : ObservableObject
{
    [ObservableProperty]
    public string? reloadedDir;
}

public class ModInfo
{
    public string ModVersion { get; set; }
}