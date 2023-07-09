// copyright (c) grvo <notgrivo@gmail.com>
// licenciado por meio da mit license.
//
// veja o arquivo LICENSE na raíz do repositório para ver licença completa.

using nix.Framework.Allocation;
using nix.Framework.Android.Input;
using nix.Framework.Bindables;
using nix.Framework.Graphics;
using nix.Framework.Localisation;

using nix.Game.Localisation;
using nix.Game.Overlays.Settings;

namespace nix.Android
{
    public partial class AndroidJoystickSettings : SettingsSubsection
    {
        protected override LocalisableString Header => JoystickSettingsStrings.JoystickGamepad;

        private readonly AndroidJoystickHandler joystickHandler;
        private readonly Bindable<bool> enabled = new BindableBool(true);

        private SettingsSlider<float> deadzoneSlider = null!;
        private Bindable<float> handlerDeadzone = null!;
        private Bindable<float> localDeadzone = null!;

        public AndroidJoystickSettings(AndroidJoystickHandler joystickHandler)
        {
            this.joystickHandler = joystickHandler;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            // usar binário local para isolar estado habilitado de mudanças para host do jogo
            handlerDeadzone = joystickHandler.DeadzoneThreshold.GetBoundCopy();
            localDeadzone = handlerDeadzone.GetUnboundCopy();

            Children = new Drawable[]
            {
                new SettingsCheckbox
                {
                    LabelText = CommonStrings.Enabled,
                    Current = enabled
                },

                deadzoneSlider = new SettingsSlider<float>
                {
                    LabelText = JoystickSettingsStrings.DeadzoneThreshold,
                    KeyboardStep = 0.01f,
                    DisplayAsPercentage = true,
                    Current = localDeadzone
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            enabled.BindTo(joystickHandler.Enabled);
            enabled.BindValueChanged(e => deadzoneSlider.Current.Disabled = !e.NewValue, true);

            handlerDeadzone.BindValueChanged(val =>
            {
                bool disabled = localDeadzone.Disabled;

                localDeadzone.Disabled = false;
                localDeadzone.Value = val.NewValue;
                localDeadzone.Disabled = disabled;
            }, true);

            localDeadzone.BindValueChanged(val => handlerDeadzone.Value = val.NewValue);
        }
    }
}