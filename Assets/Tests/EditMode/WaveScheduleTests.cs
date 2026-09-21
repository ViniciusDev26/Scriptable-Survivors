using System;
using NUnit.Framework;
using ScriptableSurvivors.Domain.Enemies;

namespace ScriptableSurvivors.Tests
{
    public sealed class WaveScheduleTests
    {
        private const float Frame = 1f / 60f;

        private static WaveSchedule Standard() =>
            new WaveSchedule(firstWaveSize: 10, growth: 5, waveDuration: 30f,
                             spawnWindow: 15f, intermission: 5f);

        /// <summary>Avança o relógio fingindo que ninguém morreu.</summary>
        private static int Run(WaveSchedule waves, float seconds, int alive = 1)
        {
            // Arredondar, não truncar: 1f/60f não é exato, e (int)(30/Frame)
            // dá 1799 quadros em vez de 1800 — um quadro a menos muda o
            // resultado de tudo que depende de fronteira de tempo.
            var frames = (int)Math.Round(seconds / Frame);

            var spawned = 0;
            for (var i = 0; i < frames; i++)
                spawned += waves.Advance(Frame, alive);

            return spawned;
        }

        [Test]
        public void Starts_on_the_first_wave()
        {
            var waves = Standard();

            Assert.That(waves.Number, Is.EqualTo(1));
            Assert.That(waves.Size, Is.EqualTo(10));
            Assert.That(waves.IsResting, Is.False);
        }

        [Test]
        public void Each_wave_is_bigger_than_the_last()
        {
            var waves = Standard();

            Assert.That(waves.Size, Is.EqualTo(10));

            // Folga nas fronteiras: somar 1/60 mil e oitocentas vezes em float
            // deixa o relógio a um fio de cabelo do zero, para qualquer lado.
            Run(waves, 31f);
            Run(waves, 6f);

            Assert.That(waves.Number, Is.EqualTo(2));
            Assert.That(waves.Size, Is.EqualTo(15));
        }

        [Test]
        public void The_whole_quota_is_born_inside_the_window()
        {
            var waves = Standard();

            var spawned = Run(waves, 15f);

            Assert.That(spawned, Is.EqualTo(10),
                "Quinze segundos é a janela: depois dela não nasce mais ninguém.");
        }

        [Test]
        public void Nothing_is_born_after_the_window_closes()
        {
            var waves = Standard();
            Run(waves, 15f);

            var afterwards = Run(waves, 14f);

            Assert.That(afterwards, Is.Zero,
                "A segunda metade da onda é só para o jogador limpar o que sobrou.");
        }

        [Test]
        public void The_quota_is_spread_over_the_window_not_dumped_at_once()
        {
            var waves = Standard();

            var firstThird = Run(waves, 5f);

            Assert.That(firstThird, Is.EqualTo(3).Within(1),
                "Dez inimigos em quinze segundos é um a cada segundo e meio.");
        }

        [Test]
        public void Bigger_waves_spawn_faster()
        {
            var waves = Standard();
            Run(waves, 31f);
            Run(waves, 6f);
            Assume.That(waves.Number, Is.EqualTo(2));

            var inFiveSeconds = Run(waves, 5f);

            Assert.That(inFiveSeconds, Is.EqualTo(5).Within(1),
                "Quinze em quinze segundos é um por segundo — a dificuldade " +
                "acelera sem nenhuma curva extra.");
        }

        [Test]
        public void Time_running_out_ends_the_wave()
        {
            var waves = Standard();

            Run(waves, 30.5f);

            Assert.That(waves.IsResting, Is.True);
        }

        [Test]
        public void Clearing_the_quota_ends_the_wave_early()
        {
            var waves = Standard();

            // 16 segundos: a janela fechou com a cota cheia, e ainda faltam
            // catorze até o relógio da onda zerar.
            Run(waves, 16f, alive: 1);
            Assume.That(waves.Spawned, Is.EqualTo(waves.Size));
            Assume.That(waves.IsResting, Is.False);

            // O jogador limpa a arena.
            waves.Advance(Frame, aliveEnemies: 0);

            Assert.That(waves.IsResting, Is.True,
                "Matar tudo encerra a onda sem esperar o relógio.");
        }

        [Test]
        public void An_empty_arena_does_not_end_a_wave_that_has_not_spawned_yet()
        {
            var waves = Standard();

            waves.Advance(Frame, aliveEnemies: 0);

            Assert.That(waves.IsResting, Is.False,
                "No primeiro quadro não nasceu ninguém ainda — a onda não pode " +
                "terminar antes de começar.");
        }

        [Test]
        public void Nobody_is_born_during_the_breather()
        {
            var waves = Standard();
            Run(waves, 30.5f);
            Assume.That(waves.IsResting, Is.True);

            var during = Run(waves, 4f);

            Assert.That(during, Is.Zero);
        }

        [Test]
        public void The_breather_lasts_what_it_promises()
        {
            var waves = Standard();
            Run(waves, 30.5f);

            Run(waves, 4f);
            Assert.That(waves.IsResting, Is.True, "Quatro segundos ainda é respiro.");

            Run(waves, 1.5f);
            Assert.That(waves.IsResting, Is.False, "Cinco e meio já é a onda seguinte.");
        }

        [Test]
        public void The_countdown_shows_how_long_the_phase_still_has()
        {
            var waves = Standard();

            Run(waves, 10f);

            Assert.That(waves.Remaining, Is.EqualTo(20f).Within(0.1f));
        }

        [Test]
        public void Refuses_a_schedule_that_makes_no_sense()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new WaveSchedule(firstWaveSize: 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => new WaveSchedule(growth: -1));
            Assert.Throws<ArgumentOutOfRangeException>(() => new WaveSchedule(waveDuration: 0f));
            Assert.Throws<ArgumentOutOfRangeException>(() => new WaveSchedule(intermission: -1f));
            Assert.Throws<ArgumentOutOfRangeException>(() => new WaveSchedule(spawnWindow: 0f));
            Assert.Throws<ArgumentException>(
                () => new WaveSchedule(waveDuration: 10f, spawnWindow: 20f));
        }

        // ---------- força da onda ----------

        [Test]
        public void The_first_wave_is_the_baseline()
        {
            Assert.That(Standard().Strength, Is.EqualTo(1f));
        }

        [Test]
        public void Enemies_get_stronger_every_wave()
        {
            var waves = new WaveSchedule(strengthGrowth: 0.25f);

            Run(waves, 31f);
            Run(waves, 6f);

            Assert.That(waves.Strength, Is.EqualTo(1.25f).Within(0.001f));
        }

        [Test]
        public void Strength_compounds_instead_of_adding_up()
        {
            var waves = new WaveSchedule(strengthGrowth: 0.25f);

            for (var i = 0; i < 4; i++)
            {
                Run(waves, 31f);
                Run(waves, 6f);
            }

            Assume.That(waves.Number, Is.EqualTo(5));
            Assert.That(waves.Strength, Is.EqualTo(2.441f).Within(0.01f),
                "1,25 elevado a 4, não 1 + 4 x 0,25. O poder do jogador é " +
                "multiplicativo — dano vezes cadência — e crescimento somado " +
                "nunca alcançaria isso.");
        }

        [Test]
        public void Growth_can_be_turned_off()
        {
            var waves = new WaveSchedule(strengthGrowth: 0f);

            Run(waves, 31f);
            Run(waves, 6f);

            Assert.That(waves.Strength, Is.EqualTo(1f));
        }

        [Test]
        public void Refuses_enemies_that_would_weaken()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new WaveSchedule(strengthGrowth: -0.1f));
        }
    }
}
