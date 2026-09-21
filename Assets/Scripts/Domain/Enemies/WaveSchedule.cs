using System;

namespace ScriptableSurvivors.Domain.Enemies
{
    /// <summary>
    /// O ritmo da run em ondas. Cada onda tem uma cota de inimigos e um tempo;
    /// ela acaba quando a cota é limpa ou quando o tempo se esgota, o que vier
    /// primeiro. Entre uma e outra há um respiro.
    ///
    /// A cota é espalhada por uma JANELA no começo da onda, não pela duração
    /// inteira: o intervalo é janela ÷ cota, então ondas maiores cospem
    /// inimigos mais rápido e a dificuldade acelera sem nenhuma curva extra.
    ///
    /// A janela também define o tempo mínimo de uma onda — antes de o último
    /// nascer não há como limpar a arena.
    ///
    /// Quem sobra de uma onda mal resolvida continua vivo na seguinte. Limpar a
    /// arena pareceria arbitrário, e a pressão acumulada é o que dá tensão.
    /// </summary>
    public sealed class WaveSchedule
    {
        private readonly int firstWaveSize;
        private readonly int growth;
        private readonly float waveDuration;
        private readonly float spawnWindow;
        private readonly float intermission;
        private readonly float strengthGrowth;

        private SpawnTimer timer;

        public int Number { get; private set; }

        /// <summary>Quantos inimigos esta onda deve produzir.</summary>
        public int Size => firstWaveSize + ((Number - 1) * growth);

        public int Spawned { get; private set; }

        /// <summary>
        /// Multiplicador de vida e dano dos inimigos desta onda.
        ///
        /// Cresce de forma MULTIPLICATIVA, não somada, porque o poder do
        /// jogador também é: dano e cadência se multiplicam entre si, e dez
        /// upgrades de meia dúzia de porcentos viram dezenas de vezes mais
        /// dano. Crescimento linear nunca alcançaria isso — e a run acabaria
        /// naquele estado em que o inimigo nasce e morre no mesmo quadro.
        /// </summary>
        public float Strength => MathF.Pow(1f + strengthGrowth, Number - 1);

        /// <summary>Verdadeiro no respiro entre duas ondas.</summary>
        public bool IsResting { get; private set; }

        /// <summary>Segundos restantes da fase atual — onda ou respiro.</summary>
        public float Remaining { get; private set; }

        public WaveSchedule(
            int firstWaveSize = 10,
            int growth = 5,
            float waveDuration = 30f,
            float spawnWindow = 15f,
            float intermission = 5f,
            float strengthGrowth = 0.25f)
        {
            if (firstWaveSize <= 0)
                throw new ArgumentOutOfRangeException(nameof(firstWaveSize), firstWaveSize, "A primeira onda precisa de inimigos.");
            if (growth < 0)
                throw new ArgumentOutOfRangeException(nameof(growth), growth, "As ondas não podem encolher.");
            if (waveDuration <= 0f)
                throw new ArgumentOutOfRangeException(nameof(waveDuration), waveDuration, "A onda precisa durar algo.");
            if (spawnWindow <= 0f)
                throw new ArgumentOutOfRangeException(nameof(spawnWindow), spawnWindow, "A janela de nascimento precisa durar algo.");
            if (spawnWindow > waveDuration)
                throw new ArgumentException("A janela de nascimento não pode passar da duração da onda.", nameof(spawnWindow));
            if (intermission < 0f)
                throw new ArgumentOutOfRangeException(nameof(intermission), intermission, "O respiro não pode ser negativo.");
            if (strengthGrowth < 0f)
                throw new ArgumentOutOfRangeException(nameof(strengthGrowth), strengthGrowth, "Os inimigos não podem enfraquecer.");

            this.firstWaveSize = firstWaveSize;
            this.growth = growth;
            this.waveDuration = waveDuration;
            this.spawnWindow = spawnWindow;
            this.intermission = intermission;
            this.strengthGrowth = strengthGrowth;

            Begin(1);
        }

        /// <summary>
        /// Avança o relógio e devolve quantos inimigos nascem neste passo.
        ///
        /// A contagem de vivos vem de fora, do quadro anterior. O atraso de um
        /// quadro em declarar a onda limpa é invisível e evita ter que dividir
        /// isto em duas chamadas com ordem obrigatória.
        /// </summary>
        public int Advance(float deltaTime, int aliveEnemies)
        {
            if (deltaTime < 0f)
                throw new ArgumentOutOfRangeException(nameof(deltaTime), deltaTime, "deltaTime não pode ser negativo.");

            Remaining -= deltaTime;

            if (IsResting)
            {
                if (Remaining <= 0f)
                    Begin(Number + 1);

                return 0;
            }

            // Nascer vem ANTES de avaliar o fim da onda. Na ordem inversa, o
            // quadro em que o relógio zera encerrava a onda e engolia o último
            // nascimento da cota.
            var due = timer.Advance(deltaTime);
            var allowed = Math.Min(due, Size - Spawned);
            Spawned += allowed;

            // allowed == 0 importa: quem acabou de nascer está vivo, e a
            // contagem recebida é do quadro anterior.
            var cleared = Spawned >= Size && aliveEnemies == 0 && allowed == 0;
            if (cleared || Remaining <= 0f)
                Rest();

            return allowed;
        }

        private void Begin(int number)
        {
            Number = number;
            Spawned = 0;
            IsResting = false;
            Remaining = waveDuration;
            timer = new SpawnTimer(spawnWindow / Size);
        }

        private void Rest()
        {
            IsResting = true;
            Remaining = intermission;
        }
    }
}
