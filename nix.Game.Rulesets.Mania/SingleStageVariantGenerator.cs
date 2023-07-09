// copyright (c) grvo <notgrivo@gmail.com>
// licenciado por meio da mit license.
//
// veja o arquivo LICENSE na raíz do repositório para ver licença completa.

using System.Collections.Generic;

using nix.Framework.Input.Bindings;

namespace nix.Game.Rulesets.Mania
{
    public class SingleStageVariantGenerator
    {
        private readonly int variant;

        private readonly InputKey[] leftKeys;
        private readonly InputKey[] rightKeys;

        public SingleStageVariantGenerator(int variant)
        {
            this.variant = variant;

            if (variant == 10)
            {
                leftKeys = new[] { InputKey.A, InputKey.S, InputKey.D, InputKey.F, InputKey.V };
                rightKeys = new[] { InputKey.N, InputKey.J, InputKey.K, InputKey.L, InputKey.Semicolon };
            }

            else
            {
                leftKeys = new[] { InputKey.A, InputKey.S, InputKey.D, InputKey.F };
                rightKeys = new[] { InputKey.J, InputKey.K, InputKey.L, InputKey.Semicolon };
            }
        }

        public IEnumerable<KeyBinding> GenerateMappings() => new VariantMappingGenerator
        {
            LeftKeys = leftKeys,
            RightKeys = rightKeys,
            SpecialKey = InputKey.Space,
            SpecialAction = ManiaAction.Special1,
            NormalActionStart = ManiaAction.Key1
        }.GenerateKeyBindingsFor(variant, out _);
    }
}