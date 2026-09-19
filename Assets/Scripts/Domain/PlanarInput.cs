using System;
using System.Numerics;

namespace ScriptableSurvivors.Domain
{
    /// <summary>
    /// Converte input de tela em direção no plano do chão.
    ///
    /// A câmera é angulada, então "para cima na tela" não é "para o norte do
    /// mundo": W precisa andar na diagonal. Isso é regra de jogo — se estivesse
    /// no MonoBehaviour, só daria para conferir jogando.
    ///
    /// Convenção: X é o eixo X do mundo, Y é o eixo Z. O plano do chão.
    /// </summary>
    public static class PlanarInput
    {
        public static Vector2 ToWorldDirection(Vector2 screenInput, float cameraYawDegrees)
        {
            if (screenInput == Vector2.Zero)
                return Vector2.Zero;

            // Teclado dá (1,1) na diagonal, que tem comprimento 1.41.
            // Sem isto, andar na diagonal seria 41% mais rápido.
            var clamped = screenInput.LengthSquared() > 1f
                ? Vector2.Normalize(screenInput)
                : screenInput;

            var radians = cameraYawDegrees * (MathF.PI / 180f);
            var sin = MathF.Sin(radians);
            var cos = MathF.Cos(radians);

            return new Vector2(
                (clamped.X * cos) + (clamped.Y * sin),
                (clamped.Y * cos) - (clamped.X * sin));
        }
    }
}
