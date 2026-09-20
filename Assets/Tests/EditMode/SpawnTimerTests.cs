using ScriptableSurvivors.Domain.Enemies;
using System;
using NUnit.Framework;

namespace ScriptableSurvivors.Tests
{
    public sealed class SpawnTimerTests
    {
        [Test]
        public void Nothing_spawns_before_the_interval_elapses()
        {
            var timer = new SpawnTimer(1f);

            Assert.That(timer.Advance(0.4f), Is.Zero);
            Assert.That(timer.Advance(0.4f), Is.Zero);
        }

        [Test]
        public void One_spawns_when_the_interval_completes()
        {
            var timer = new SpawnTimer(1f);

            timer.Advance(0.6f);

            Assert.That(timer.Advance(0.6f), Is.EqualTo(1));
        }

        [Test]
        public void A_long_frame_does_not_swallow_spawns()
        {
            var timer = new SpawnTimer(0.5f);

            Assert.That(timer.Advance(2f), Is.EqualTo(4),
                "Um travamento de 2s não pode cancelar os 4 inimigos que deviam ter nascido.");
        }

        [Test]
        public void Spawn_rate_does_not_depend_on_frame_rate()
        {
            var sixtyFps = new SpawnTimer(1f);
            var tenFps = new SpawnTimer(1f);

            var fastTotal = 0;
            for (var i = 0; i < 600; i++)
                fastTotal += sixtyFps.Advance(1f / 60f);

            var slowTotal = 0;
            for (var i = 0; i < 100; i++)
                slowTotal += tenFps.Advance(1f / 10f);

            Assert.That(fastTotal, Is.EqualTo(slowTotal),
                "Dez segundos de jogo devem gerar a mesma quantidade de inimigos " +
                "em 60 e em 10 quadros por segundo.");
        }

        [Test]
        public void Rejects_an_interval_that_would_spawn_forever()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new SpawnTimer(0f));
            Assert.Throws<ArgumentOutOfRangeException>(() => new SpawnTimer(-1f));
        }
    }
}
