using System;
using System.Numerics;
using ScriptableSurvivors.Domain.Combat;
using ScriptableSurvivors.Domain.Progression;

namespace ScriptableSurvivors.Domain.Players
{
    /// <summary>
    /// O jogador: onde está e quanta vida ainda tem. A posição é estado da
    /// simulação, não do Transform — o MonoBehaviour apenas lê e desenha.
    /// </summary>
    public sealed class Player
    {
        private StatModifiers modifiers;

        public Vector2 Position { get; private set; }

        /// <summary>A velocidade como veio da configuração. Nunca muda.</summary>
        public float BaseSpeed { get; }

        /// <summary>A velocidade em uso: a base com os upgrades da run somados.</summary>
        public float Speed => modifiers == null
            ? BaseSpeed
            : Math.Max(0.1f, modifiers.Apply(StatKind.PlayerSpeed, BaseSpeed));

        public Health Health { get; }

        public Player(float speed, float maxHealth, Vector2 startPosition = default)
        {
            if (speed <= 0f)
                throw new ArgumentOutOfRangeException(nameof(speed), speed, "Velocidade deve ser positiva.");

            BaseSpeed = speed;
            Health = new Health(maxHealth);
            Position = startPosition;
        }

        public void BindModifiers(StatModifiers runModifiers) => modifiers = runModifiers;

        public void Move(Vector2 screenInput, float cameraYawDegrees, float deltaTime)
        {
            if (deltaTime < 0f)
                throw new ArgumentOutOfRangeException(nameof(deltaTime), deltaTime, "deltaTime não pode ser negativo.");

            var direction = PlanarInput.ToWorldDirection(screenInput, cameraYawDegrees);
            Position += direction * (Speed * deltaTime);
        }
    }
}
