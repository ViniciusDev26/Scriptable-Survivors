using System;
using System.Numerics;

namespace ScriptableSurvivors.Domain
{
    /// <summary>
    /// Um inimigo vivo na arena. Os números vêm do catálogo e são
    /// compartilhados por todos do mesmo tipo; a vida e a posição são desta
    /// instância e de mais ninguém.
    /// </summary>
    public sealed class Enemy
    {
        public EnemyStats Stats { get; }
        public Health Health { get; }
        public Vector2 Position { get; private set; }

        public Enemy(EnemyStats stats, Vector2 position)
        {
            Stats = stats;
            Health = stats.CreateHealth();
            Position = position;
        }

        public void MoveToward(Vector2 target, float deltaTime)
        {
            if (deltaTime < 0f)
                throw new ArgumentOutOfRangeException(nameof(deltaTime), deltaTime, "deltaTime não pode ser negativo.");

            var toTarget = target - Position;
            var distance = toTarget.Length();
            if (distance <= float.Epsilon)
                return;

            var step = Stats.Speed * deltaTime;

            // Sem esta guarda, um inimigo rápido passaria do alvo e voltaria
            // no quadro seguinte, tremendo em volta do jogador para sempre.
            if (step >= distance)
            {
                Position = target;
                return;
            }

            Position += (toTarget / distance) * step;
        }
    }
}
