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
    }
}
