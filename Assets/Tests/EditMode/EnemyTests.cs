using System.Numerics;
using NUnit.Framework;
using ScriptableSurvivors.Domain;

namespace ScriptableSurvivors.Tests
{
    public sealed class EnemyTests
    {
        private static EnemyStats Slime => new EnemyStats(20f, 2f, 5f, 3);

        [Test]
        public void Starts_where_it_spawned_with_full_health()
        {
            var enemy = new Enemy(Slime, new Vector2(10f, -4f));

            Assert.That(enemy.Position, Is.EqualTo(new Vector2(10f, -4f)));
            Assert.That(enemy.Health.Current, Is.EqualTo(20f));
        }

        [Test]
        public void Two_enemies_of_the_same_type_have_separate_health()
        {
            var first = new Enemy(Slime, Vector2.Zero);
            var second = new Enemy(Slime, Vector2.One);

            first.Health.TakeDamage(20f);

            Assert.That(first.Health.IsDead, Is.True);
            Assert.That(second.Health.IsDead, Is.False,
                "Ferir um inimigo não pode ferir os outros do mesmo tipo. " +
                "É esta separação que a demonstração do Dia 4 quebra de propósito.");
        }
    }
}
