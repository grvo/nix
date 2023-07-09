// copyright (c) grvo <notgrivo@gmail.com>
// licenciado por meio da mit license.
//
// veja o arquivo LICENSE na raíz do repositório para ver licença completa.

using System;
using System.Security.Principal;

using nix.Framework;
using nix.Framework.Allocation;
using nix.Framework.Graphics;
using nix.Framework.Graphics.Sprites;
using nix.Game.Graphics;
using nix.Game.Overlays;
using nix.Game.Overlays.Notifications;

namespace nix.Desktop.Security
{
    /// <summary>
    /// checa se o jogo está rodando com privilégios elevados (admin no windows / root no linux) e mostra uma notificação de aviso
    /// </summary>
    public partial class ElevatedPrivilegesChecker : Component
    {
        [Resolved]
        private INotificationOverlay notifications { get; set; } = null!;

        private bool elevated;

        [BackgroundDependencyLoader]
        private void load()
        {
            elevated = checkElevated();
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            if (elevated)
                notifications.Post(new ElevatedPrivilegesNotification());
        }

        private bool checkElevated()
        {
            try
            {
                switch (RuntimeInfo.OS)
                {
                    case RuntimeInfo.Platform.Windows:
                        if (!OperatingSystem.IsWindows()) return false;

                        var windowsIdentity = WindowsIdentity.GetCurrent();
                        var windowsPrincipal = new WindowsPrincipal(windowsIdentity);

                        return windowsPrincipal.IsInRole(WindowsBuiltInRole.Administrator);

                    case RuntimeInfo.Platform.macOS:
                    case RuntimeInfo.Platform.Linux:
                        return Mono.Unix.Native.Syscall.geteuid() == 0;
                }
            }
            catch
            {
            }

            return false;
        }

        private partial class ElevatedPrivilegesNotification : SimpleNotification
        {
            public override bool IsImportant => true;

            public ElevatedPrivilegesNotification()
            {
                Text = $"rodando nix! como {(RuntimeInfo.IsUnix ? "root" : "administrator")} não melhora a performance, pode quebrar algumas integrações e gerar algum risco de segurança. por favor rode o jogo como usuário.";
            }

            [BackgroundDependencyLoader]
            private void load(NixColour colours)
            {
                Icon = FontAwesome.Solid.ShieldAlt;
                
                IconContent.Colour = colours.YellowDark;
            }
        }
    }
}