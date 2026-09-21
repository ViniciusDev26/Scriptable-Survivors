using ScriptableSurvivors.Domain.Enemies;
using System;
using NUnit.Framework;

namespace ScriptableSurvivors.Tests
{
    public sealed class EnemyStatsTests
    {
        [Test]
        public void Keeps_the_values_it_was_given()
        {
            var stats = new EnemyStats(maxHealth: 30f, speed: 1.5f, damage: 7f, xpReward: 4);

            Assert.That(stats.MaxHealth, Is.EqualTo(30f));
            Assert.That(stats.Speed, Is.EqualTo(1.5f));
            Assert.That(stats.Damage, Is.EqualTo(7f));
            Assert.That(stats.XpReward, Is.EqualTo(4));
        }

        [Test]
        public void Creates_health_starting_at_max()
        {
            var stats = new EnemyStats(30f, 1.5f, 7f, 4);

            var health = stats.CreateHealth();

            Assert.That(health.Max, Is.EqualTo(30f));
            Assert.That(health.Current, Is.EqualTo(30f));
        }

        [Test]
        public void Each_enemy_gets_its_own_health()
        {
            var stats = new EnemyStats(30f, 1.5f, 7f, 4);

            var first = stats.CreateHealth();
            var second = stats.CreateHealth();
            first.TakeDamage(10f);

            Assert.That(second.Current, Is.EqualTo(30f),
                "O catálogo é compartilhado; a vida não. Se este teste falhar, " +
                "ferir um inimigo estaria ferindo todos os do mesmo tipo.");
        }

        [Test]
        public void Refuses_an_enemy_that_cannot_exist()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new EnemyStats(0f, 1f, 1f, 1));
            Assert.Throws<ArgumentOutOfRangeException>(() => new EnemyStats(10f, -1f, 1f, 1));
            Assert.Throws<ArgumentOutOfRangeException>(() => new EnemyStats(10f, 1f, -1f, 1));
            Assert.Throws<ArgumentOutOfRangeException>(() => new EnemyStats(10f, 1f, 1f, -1));
        }

        [Test]
        public void A_harmless_stationary_enemy_is_allowed()
        {
            Assert.DoesNotThrow(() => new EnemyStats(10f, speed: 0f, damage: 0f, xpReward: 0));
        }

        // ---------- escala por onda ----------

        [Test]
        public void A_scaled_enemy_is_tougher_and_hits_harder()
        {
            var scaled = new EnemyStats(20f, 1.5f, 7f, 4).Scaled(2f);

            Assert.That(scaled.MaxHealth, Is.EqualTo(40f));
            Assert.That(scaled.Damage, Is.EqualTo(14f));
        }

        [Test]
        public void Scaling_never_touches_speed()
        {
            var scaled = new EnemyStats(20f, 1.5f, 7f, 4).Scaled(5f);

            Assert.That(scaled.Speed, Is.EqualTo(1.5f),
                "Velocidade escalada tornaria os rápidos impossíveis de evitar " +
                "em poucas ondas.");
        }

        [Test]
        public void Scaling_never_touches_xp()
        {
            var scaled = new EnemyStats(20f, 1.5f, 7f, 4).Scaled(5f);

            Assert.That(scaled.XpReward, Is.EqualTo(4),
                "Inimigo mais duro já demora mais para morrer, o que freia o " +
                "ritmo de subir de nível. Escalar o XP anularia esse freio.");
        }

        [Test]
        public void Scaling_keeps_the_catalog_identity()
        {
            var scaled = new EnemyStats(20f, 1.5f, 7f, 4, "brute").Scaled(3f);

            Assert.That(scaled.Id, Is.EqualTo("brute"),
                "É por este Id que o adaptador acha o prefab.");
        }

        [Test]
        public void Refuses_a_scale_that_makes_no_enemy()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new EnemyStats(20f, 1f, 1f, 1).Scaled(0f));
        }
    }
}
