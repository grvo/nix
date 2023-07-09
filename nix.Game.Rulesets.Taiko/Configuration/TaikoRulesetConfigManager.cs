// copyright (c) grvo <notgrivo@gmail.com>
// licenciado por meio da mit license.
//
// veja o arquivo LICENSE na raíz do repositório para ver licença completa.

using nix.Game.Configuration;
using nix.Game.Rulesets.Configuration;

namespace nix.Game.Rulesets.Taiko.Configuration
{
    public class TaikoRulesetConfigManager : RulesetConfigManager<TaikoRulesetSetting>
    {
        public TaikoRulesetConfigManager(SettingsStore? settings, RulesetInfo ruleset, int? variant = null)
            : base(settings, ruleset, variant)
        {
        }

        protected override void InitialiseDefaults()
        {
            base.InitialiseDefaults();

            SetDefault(TaikoRulesetSetting.TouchControlScheme, TaikoTouchControlScheme.KDDK);
        }
    }

    public enum TaikoRulesetSetting
    {
        TouchControlScheme
    }
}