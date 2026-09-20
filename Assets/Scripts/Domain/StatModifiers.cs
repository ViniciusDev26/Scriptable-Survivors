using System.Collections.Generic;

namespace ScriptableSurvivors.Domain
{
    /// <summary>
    /// A pilha de modificadores de uma run. Os números do catálogo ficam
    /// intactos como base; o que os upgrades somam vive aqui, separado.
    ///
    /// Por isso é sempre possível responder de onde cada ponto de dano veio —
    /// que é a mesma lição do bug do Dia 4, aplicada ao motor do jogo.
    ///
    /// Percentuais somam entre si em vez de compor: três upgrades de +30% dão
    /// +90%, não 2,2x. Previsível o bastante para balancear de cabeça.
    /// </summary>
    public sealed class StatModifiers
    {
        private readonly Dictionary<StatKind, float> flat = new Dictionary<StatKind, float>();
        private readonly Dictionary<StatKind, float> percent = new Dictionary<StatKind, float>();

        public void Add(Upgrade upgrade)
        {
            var bucket = upgrade.Kind == ModifierKind.Flat ? flat : percent;
            bucket.TryGetValue(upgrade.Stat, out var accumulated);
            bucket[upgrade.Stat] = accumulated + upgrade.Value;
        }

        public float FlatOn(StatKind stat) =>
            flat.TryGetValue(stat, out var value) ? value : 0f;

        public float PercentOn(StatKind stat) =>
            percent.TryGetValue(stat, out var value) ? value : 0f;

        public float Apply(StatKind stat, float baseValue) =>
            (baseValue + FlatOn(stat)) * (1f + PercentOn(stat));

        /// <summary>
        /// Os limites existem para que combinações estranhas de upgrades não
        /// produzam uma arma impossível — cadência zero, alcance negativo — e
        /// derrubem a run com exceção no meio da apresentação.
        /// </summary>
        public WeaponStats ApplyTo(WeaponStats baseStats)
        {
            return new WeaponStats(
                damage: Max(Apply(StatKind.WeaponDamage, baseStats.Damage), 0f),
                shotsPerSecond: Max(Apply(StatKind.WeaponFireRate, baseStats.ShotsPerSecond), 0.05f),
                projectileSpeed: Max(Apply(StatKind.WeaponProjectileSpeed, baseStats.ProjectileSpeed), 0.1f),
                range: Max(Apply(StatKind.WeaponRange, baseStats.Range), 0.5f),
                splashRadius: Max(Apply(StatKind.WeaponSplashRadius, baseStats.SplashRadius), 0f));
        }

        private static float Max(float value, float floor) => value < floor ? floor : value;
    }
}
