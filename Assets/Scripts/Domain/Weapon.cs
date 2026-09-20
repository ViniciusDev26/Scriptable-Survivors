using System;
using System.Numerics;

namespace ScriptableSurvivors.Domain
{
    /// <summary>
    /// A arma que o jogador carrega. Sabe atirar e esperar; não sabe em quem —
    /// escolher o alvo é da Arena, que é quem enxerga os inimigos.
    /// </summary>
    public sealed class Weapon
    {
        private StatModifiers modifiers;

        /// <summary>Os números como vieram do catálogo. Nunca mudam.</summary>
        public WeaponStats BaseStats { get; }

        /// <summary>Os números em uso: a base com os upgrades da run somados.</summary>
        public WeaponStats Stats => modifiers == null ? BaseStats : modifiers.ApplyTo(BaseStats);

        public float CooldownRemaining { get; private set; }

        public bool IsReady => CooldownRemaining <= 0f;

        public Weapon(WeaponStats stats)
        {
            BaseStats = stats;
        }

        /// <summary>Liga a arma à pilha de modificadores da run. A Arena faz isso ao equipar.</summary>
        public void BindModifiers(StatModifiers runModifiers) => modifiers = runModifiers;

        public void Cool(float deltaTime)
        {
            if (deltaTime < 0f)
                throw new ArgumentOutOfRangeException(nameof(deltaTime), deltaTime, "deltaTime não pode ser negativo.");

            CooldownRemaining = MathF.Max(0f, CooldownRemaining - deltaTime);
        }

        public Projectile Fire(Vector2 origin, Vector2 target)
        {
            if (!IsReady)
                throw new InvalidOperationException("A arma ainda está recarregando.");

            var toTarget = target - origin;

            // Enemy.MoveToward para exatamente em cima do jogador, então o alvo
            // pode coincidir com a origem. Qualquer direção acerta nesse caso —
            // o tiro nasce dentro do inimigo. Lançar exceção aqui travaria o
            // jogo justamente quando alguém te alcança.
            if (toTarget.LengthSquared() <= float.Epsilon)
                toTarget = Vector2.UnitX;

            CooldownRemaining = Stats.Cooldown;
            return new Projectile(
                origin, toTarget, Stats.ProjectileSpeed, Stats.Damage, Stats.Range, Stats.SplashRadius);
        }
    }
}
