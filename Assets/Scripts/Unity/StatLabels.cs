using ScriptableSurvivors.Domain.Progression;
using System.Text;

namespace ScriptableSurvivors.Unity
{
    /// <summary>
    /// Os nomes dos stats em português. É texto de apresentação, então mora
    /// deste lado da fronteira — o domínio não sabe em que idioma será lido.
    /// </summary>
    internal static class StatLabels
    {
        public static string For(StatKind stat)
        {
            switch (stat)
            {
                case StatKind.WeaponDamage: return "dano";
                case StatKind.WeaponFireRate: return "cadência";
                case StatKind.WeaponRange: return "alcance";
                case StatKind.WeaponProjectileSpeed: return "velocidade do projétil";
                case StatKind.WeaponSplashRadius: return "raio de explosão";
                case StatKind.PlayerSpeed: return "velocidade";
                case StatKind.PlayerMaxHealth: return "vida máxima";
                default: return stat.ToString();
            }
        }

        public static string Amount(ModifierKind kind, float value) =>
            kind == ModifierKind.Percent
                ? value.ToString("+0%;-0%")
                : value.ToString("+0.##;-0.##");

        /// <summary>
        /// Uma linha de bônus, somando o que a pilha tem para aquele stat.
        /// Exemplos: "+90% dano", "+5 dano", "+5 e +90% dano".
        /// </summary>
        public static string Describe(StatModifiers modifiers, StatKind stat, StringBuilder into)
        {
            var flat = modifiers.FlatOn(stat);
            var percent = modifiers.PercentOn(stat);

            into.Clear();

            if (flat != 0f)
                into.Append(Amount(ModifierKind.Flat, flat));

            if (percent != 0f)
            {
                if (into.Length > 0)
                    into.Append(" e ");

                into.Append(Amount(ModifierKind.Percent, percent));
            }

            into.Append(' ').Append(For(stat));
            return into.ToString();
        }
    }
}
