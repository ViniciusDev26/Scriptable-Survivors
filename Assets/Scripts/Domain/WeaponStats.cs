using System;

namespace ScriptableSurvivors.Domain
{
    /// <summary>
    /// Os números de uma arma, já fora da Unity. Como EnemyStats, é um retrato
    /// imutável do catálogo — o domínio nunca segura a referência do asset.
    /// </summary>
    public readonly struct WeaponStats
    {
        public float Damage { get; }
        public float ShotsPerSecond { get; }
        public float ProjectileSpeed { get; }
        public float Range { get; }

        /// <summary>Segundos entre dois tiros. Derivado, não guardado.</summary>
        public float Cooldown => 1f / ShotsPerSecond;

        public WeaponStats(float damage, float shotsPerSecond, float projectileSpeed, float range)
        {
            if (damage < 0f)
                throw new ArgumentOutOfRangeException(nameof(damage), damage, "Dano não pode ser negativo.");
            if (shotsPerSecond <= 0f)
                throw new ArgumentOutOfRangeException(nameof(shotsPerSecond), shotsPerSecond, "A cadência deve ser positiva.");
            if (projectileSpeed <= 0f)
                throw new ArgumentOutOfRangeException(nameof(projectileSpeed), projectileSpeed, "O projétil precisa se mover.");
            if (range <= 0f)
                throw new ArgumentOutOfRangeException(nameof(range), range, "O alcance deve ser positivo.");

            Damage = damage;
            ShotsPerSecond = shotsPerSecond;
            ProjectileSpeed = projectileSpeed;
            Range = range;
        }
    }
}
