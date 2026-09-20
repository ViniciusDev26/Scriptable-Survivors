using System;

namespace ScriptableSurvivors.Domain.Combat
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

        /// <summary>
        /// Raio em que o tiro machuca quem estava perto do alvo atingido.
        /// Zero é arma de alvo único — a pistola.
        /// </summary>
        public float SplashRadius { get; }

        /// <summary>Segundos entre dois tiros. Derivado, não guardado.</summary>
        public float Cooldown => 1f / ShotsPerSecond;

        public bool IsExplosive => SplashRadius > 0f;

        public WeaponStats(
            float damage,
            float shotsPerSecond,
            float projectileSpeed,
            float range,
            float splashRadius = 0f)
        {
            if (damage < 0f)
                throw new ArgumentOutOfRangeException(nameof(damage), damage, "Dano não pode ser negativo.");
            if (shotsPerSecond <= 0f)
                throw new ArgumentOutOfRangeException(nameof(shotsPerSecond), shotsPerSecond, "A cadência deve ser positiva.");
            if (projectileSpeed <= 0f)
                throw new ArgumentOutOfRangeException(nameof(projectileSpeed), projectileSpeed, "O projétil precisa se mover.");
            if (range <= 0f)
                throw new ArgumentOutOfRangeException(nameof(range), range, "O alcance deve ser positivo.");
            if (splashRadius < 0f)
                throw new ArgumentOutOfRangeException(nameof(splashRadius), splashRadius, "O raio de explosão não pode ser negativo.");

            Damage = damage;
            ShotsPerSecond = shotsPerSecond;
            ProjectileSpeed = projectileSpeed;
            Range = range;
            SplashRadius = splashRadius;
        }
    }
}
