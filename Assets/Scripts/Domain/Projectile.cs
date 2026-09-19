using System;
using System.Numerics;

namespace ScriptableSurvivors.Domain
{
    /// <summary>
    /// Um tiro em voo. Guarda quanto alcance ainda lhe resta em vez de por
    /// quanto tempo viveu: assim o alcance da arma significa distância, e não
    /// depende da velocidade do projétil.
    /// </summary>
    public sealed class Projectile
    {
        public Vector2 Position { get; private set; }
        public Vector2 Direction { get; }
        public float Speed { get; }
        public float Damage { get; }
        public float RemainingRange { get; private set; }
        public bool IsSpent { get; private set; }

        /// <summary>Raio de dano em área no ponto de impacto. Zero é alvo único.</summary>
        public float SplashRadius { get; }

        public Projectile(
            Vector2 origin,
            Vector2 direction,
            float speed,
            float damage,
            float range,
            float splashRadius = 0f)
        {
            if (direction.LengthSquared() <= float.Epsilon)
                throw new ArgumentException("Um projétil precisa de uma direção.", nameof(direction));
            if (speed <= 0f)
                throw new ArgumentOutOfRangeException(nameof(speed), speed, "A velocidade deve ser positiva.");
            if (range <= 0f)
                throw new ArgumentOutOfRangeException(nameof(range), range, "O alcance deve ser positivo.");
            if (splashRadius < 0f)
                throw new ArgumentOutOfRangeException(nameof(splashRadius), splashRadius, "O raio de explosão não pode ser negativo.");

            Position = origin;
            Direction = Vector2.Normalize(direction);
            Speed = speed;
            Damage = damage;
            RemainingRange = range;
            SplashRadius = splashRadius;
        }

        public void Advance(float deltaTime)
        {
            if (IsSpent)
                return;
            if (deltaTime < 0f)
                throw new ArgumentOutOfRangeException(nameof(deltaTime), deltaTime, "deltaTime não pode ser negativo.");

            var step = Speed * deltaTime;
            if (step >= RemainingRange)
            {
                step = RemainingRange;
                IsSpent = true;
            }

            Position += Direction * step;
            RemainingRange -= step;
        }

        /// <summary>Gasta o projétil ao acertar. Um tiro atinge um alvo só.</summary>
        public void Consume() => IsSpent = true;
    }
}
