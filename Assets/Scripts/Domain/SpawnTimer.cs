using System;

namespace ScriptableSurvivors.Domain
{
    /// <summary>
    /// Conta o tempo entre nascimentos e diz quantos inimigos devem nascer
    /// neste quadro — mais de um, se o quadro demorou demais.
    ///
    /// O resto do tempo é guardado em vez de descartado: sem isso, a taxa de
    /// spawn dependeria da taxa de quadros, e o jogo ficaria mais fácil em
    /// máquinas lentas.
    /// </summary>
    public sealed class SpawnTimer
    {
        // double, não float: somar 1/60 seiscentas vezes em float acumula
        // erro suficiente para engolir um inimigo a cada dez segundos.
        private double elapsed;

        public float Interval { get; }

        public SpawnTimer(float interval)
        {
            if (interval <= 0f)
                throw new ArgumentOutOfRangeException(nameof(interval), interval, "O intervalo deve ser positivo.");

            Interval = interval;
        }

        public int Advance(float deltaTime)
        {
            if (deltaTime < 0f)
                throw new ArgumentOutOfRangeException(nameof(deltaTime), deltaTime, "deltaTime não pode ser negativo.");

            elapsed += deltaTime;

            var due = (int)(elapsed / Interval);
            elapsed -= due * (double)Interval;
            return due;
        }
    }
}
