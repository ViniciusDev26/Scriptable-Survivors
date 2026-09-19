using System;
using System.Numerics;

namespace ScriptableSurvivors.Domain
{
    /// <summary>
    /// Posição do jogador no plano do chão. A posição é estado da simulação,
    /// não do Transform — o MonoBehaviour apenas lê e desenha.
    /// </summary>
    public sealed class PlayerMovement
    {
        public Vector2 Position { get; private set; }
        public float Speed { get; }

        public PlayerMovement(float speed, Vector2 startPosition = default)
        {
            if (speed <= 0f)
                throw new ArgumentOutOfRangeException(nameof(speed), speed, "Velocidade deve ser positiva.");

            Speed = speed;
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
