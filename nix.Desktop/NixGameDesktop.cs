// copyright (c) grvo <notgrivo@gmail.com>
// licenciado por meio da mit license.
//
// veja o arquivo LICENSE na raíz do repositório para ver licença completa.

using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.Versioning;

using Microsoft.Win32;

using nix.Desktop.Security;
using nix.Framework.Platform;
using nix.Game;
using nix.Desktop.Updater;
using nix.Framework;
using nix.Framework.Logging;
using nix.Game.Updater;
using nix.Desktop.Windows;
using nix.Game.IO;
using nix.Game.IPC;
using nix.Game.Online.Multiplayer;
using nix.Game.Utils;

using SDL2;

namespace nix.Desktop
{
    internal partial class NixGameDesktop : NixGame
    {
        private NixSchemeLinkIPCChannel? nixSchemeLinkIPCChannel;
        private ArchiveImportIPCChannel? archiveImportIPCChannel;

        public NixGameDesktop(string[]? args = null)
            : base(args)
        {
        }

        public override StableStorage? GetStorageForStableInstall()
        {
            try
            {
                if (Host is DesktopGameHost desktopHost)
                {
                    string? stablePath = getStableInstallPath();

                    if (!string.IsNullOrEmpty(stablePath))
                        return new StableStorage(stablePath, desktopHost);
                }
            }

            catch (Exception)
            {
                Logger.Log("não foi possível encontrar uma instalação estável", LoggingTarget.Runtime, LogLevel.Important);
            }

            return null;
        }

        private string? getStableInstallPath()
        {
            static bool checkExists(string p) => Directory.Exists(Path.Combine(p, "músicas")) || File.Exists(Path.Combine(p, "nix!.cfg"));

            string? stableInstallPath;

            if (OperatingSystem.IsWindows())
            {
                try
                {
                    stableInstallPath = getStableInstallPathFromRegistry();

                    if (!string.IsNullOrEmpty(stableInstallPath) && checkExists(stableInstallPath))
                        return stableInstallPath;
                }

                catch
                {
                }
            }

            stableInstallPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"nix!");

            if (checkExists(stableInstallPath))
                return stableInstallPath;

            stableInstallPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".nix");

            if (checkExists(stableInstallPath))
                return stableInstallPath;

            return null;
        }

        [SupportedOSPlatform("windows")]
        private string? getStableInstallPathFromRegistry()
        {
            using (RegistryKey? key = Registry.ClassesRoot.OpenSubKey("nix"))
                return key?.OpenSubKey(@"shell\open\command")?.GetValue(string.Empty)?.ToString()?.Split('"')[1].Replace("nix!.exe", "");
        }

        protected override UpdateManager CreateUpdateManager()
        {
            string? packageManaged = Environment.GetEnvironmentVariable("NIX_EXTERNAL_UPDATE_PROVIDER");

            if (!string.IsNullOrEmpty(packageManaged))
                return new NoActionUpdateManager();

            switch (RuntimeInfo.OS)
            {
                case RuntimeInfo.Platform.Windows:
                    Debug.Assert(OperatingSystem.IsWindows());

                    return new SquirrelUpdateManager();

                default:
                    return new SimpleUpdateManager();
            }
        }

        public override bool RestartAppWhenExited()
        {
            switch (RuntimeInfo.OS)
            {
                case RuntimeInfo.Platform.Windows:
                    Debug.Assert(OperatingSystem.IsWindows());

                    Squirrel.UpdateManager.RestartAppWhenExited().FireAndForget();

                    return true;
            }

            return base.RestartAppWhenExited();
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            LoadComponentAsync(new DiscordRichPresence(), Add);

            if (RuntimeInfo.OS == RuntimeInfo.Platform.Windows)
                LoadComponentAsync(new GameplayWinKeyBlocker(), Add);

            LoadComponentAsync(new ElevatedPrivilegesChecker(), Add);

            nixSchemeLinkIPCChannel = new NixSchemeLinkIPCChannel(Host, this);
            archiveImportIPCChannel = new ArchiveImportIPCChannel(Host, this);
        }

        public override void SetHost(GameHost host)
        {
            base.SetHost(host);

            var iconStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(GetType(), "lazer.ico");

            if (iconStream != null)
                host.Window.SetIconFromStream(iconStream);

            host.Window.CursorState |= CursorState.Hidden;
            host.Window.Title = Name;
        }

        protected override BatteryInfo CreateBatteryInfo() => new SDL2BatteryInfo();

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            nixSchemeLinkIPCChannel?.Dispose();
            archiveImportIPCChannel?.Dispose();
        }

        private class SDL2BatteryInfo : BatteryInfo
        {
            public override double? ChargeLevel
            {
                get
                {
                    SDL.SDL_GetPowerInfo(out _, out int percentage);

                    if (percentage == -1)
                        return null;

                    return percentage / 100.0;
                }
            }

            public override bool OnBattery => SDL.SDL_GetPowerInfo(out _, out _) == SDL.SDL_PowerState.SDL_POWERSTATE_ON_BATTERY;
        }
    }
}