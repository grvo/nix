// copyright (c) grvo <notgrivo@gmail.com>
// licenciado por meio da mit license.
//
// veja o arquivo LICENSE na raíz do repositório para ver licença completa.

using System.ComponentModel;

using nix.Framework.Allocation;
using nix.Framework.Input.Bindings;
using nix.Game.Rulesets.UI;

namespace nix.Game.Rulesets.Catch
{
    [Cached]
    public partial class CatchInputManager : RulesetInputManager<CatchAction>
    {
        public CatchInputManager(RulesetInfo ruleset)
            : base(ruleset, 0, SimultaneousBindingMode.Unique)
        {
        }
    }

    public enum CatchAction
    {
        [Description("mover para esquerda")]
        MoveLeft,

        [Description("mover para direita")]
        MoveRight,

        [Description("engajar dash")]
        Dash
    }
}