using System;
using System.Numerics;

namespace ScriptableSurvivors.Domain
{
    /// <summary>
    /// O jogador: onde está e quanta vida ainda tem. A posição é estado da
    /// simulação, não do Transform — o MonoBehaviour apenas lê e desenha.
    /// </summary>
    public sealed class Player
    {
        public Vector2 Position { get; private set; }
        public float Speed { get; }
        public Health Health { get; }

        public Player(float speed, float maxHealth, Vector2 startPosition = default)
        {
            if (speed <= 0f)
                throw new ArgumentOutOfRangeException(nameof(speed), speed, "Velocidade deve ser positiva.");

            Speed = speed;
            Health = new Health(maxHealth);
            Position = startPosition;
        }

        public void Move(Vector2 screenInput, float cameraYawDegrees, float deltaTime)
        {
            if (deltaTime < 0f)
                throw new ArgumentOutOfRangeException(nameof(deltaTime), deltaTime, "deltaTime não pode ser negativo.");

            var direction = PlanarInput.ToWorldDirection(screenInput, cameraYawDegrees);
            Position += direction * (Speed * deltaTime);
        }
    }
}
