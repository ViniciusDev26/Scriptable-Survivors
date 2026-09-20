using ScriptableSurvivors.Domain.Progression;


namespace ScriptableSurvivors.Domain.Combat
{
    /// <summary>
    /// Traduz a pilha de modificadores em números de arma.
    ///
    /// Mora aqui, e não em StatModifiers, porque uma pilha de modificadores é
    /// genérica: ela sabe somar valores por stat, não montar uma arma. Com o
    /// método do outro lado, Combat e Progression dependiam um do outro — um
    /// ciclo invisível enquanto tudo estava num namespace só.
    /// </summary>
    public static class WeaponStatsModifiers
    {
        /// <summary>
        /// Os limites existem para que combinações estranhas de upgrades não
        /// produzam uma arma impossível — cadência zero, alcance negativo — e
        /// derrubem a run com exceção no meio da apresentação.
        /// </summary>
        public static WeaponStats ApplyTo(this StatModifiers modifiers, WeaponStats baseStats)
        {
            return new WeaponStats(
                damage: Floor(modifiers.Apply(StatKind.WeaponDamage, baseStats.Damage), 0f),
                shotsPerSecond: Floor(modifiers.Apply(StatKind.WeaponFireRate, baseStats.ShotsPerSecond), 0.05f),
                projectileSpeed: Floor(modifiers.Apply(StatKind.WeaponProjectileSpeed, baseStats.ProjectileSpeed), 0.1f),
                range: Floor(modifiers.Apply(StatKind.WeaponRange, baseStats.Range), 0.5f),
                splashRadius: Floor(modifiers.Apply(StatKind.WeaponSplashRadius, baseStats.SplashRadius), 0f));
        }

        private static float Floor(float value, float floor) => value < floor ? floor : value;
    }
}
