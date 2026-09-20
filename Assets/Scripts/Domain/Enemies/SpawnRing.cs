using System;
using System.Numerics;

namespace ScriptableSurvivors.Domain.Enemies
{
    /// <summary>
    /// O círculo onde os inimigos nascem, sempre fora do enquadramento da
    /// câmera. Onde nascer é geometria, não desenho: dá para provar que
    /// nenhum ponto cai dentro da tela com um Assert, em vez de jogar e torcer.
    /// </summary>
    public sealed class SpawnRing
    {
        public float Radius { get; }

        public SpawnRing(float radius)
        {
            if (radius <= 0f)
                throw new ArgumentOutOfRangeException(nameof(radius), radius, "O raio de spawn deve ser positivo.");

            Radius = radius;
        }

        public Vector2 PointAt(Vector2 center, float angleRadians)
        {
            return new Vector2(
                center.X + (MathF.Cos(angleRadians) * Radius),
                center.Y + (MathF.Sin(angleRadians) * Radius));
        }

        /// <summary>
        /// O Random entra por parâmetro em vez de ser criado aqui: é o que
        /// torna o sorteio reproduzível no teste e, mais tarde, permite
        /// repetir uma run inteira a partir de uma semente.
        /// </summary>
        public Vector2 NextPoint(Vector2 center, Random random)
        {
            if (random == null)
                throw new ArgumentNullException(nameof(random));

            return PointAt(center, (float)(random.NextDouble() * Math.PI * 2d));
        }
    }
}
