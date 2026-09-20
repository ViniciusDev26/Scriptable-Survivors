using ScriptableSurvivors.Domain.Combat;
using ScriptableSurvivors.Domain.Enemies;
using System.Numerics;
using NUnit.Framework;

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

        [Test]
        public void Walks_toward_the_player()
        {
            var enemy = new Enemy(Slime, new Vector2(0f, 10f));

            enemy.MoveToward(Vector2.Zero, deltaTime: 1f);

            Assert.That(enemy.Position.Y, Is.EqualTo(8f).Within(0.001f),
                "Velocidade 2 por um segundo deve cobrir 2 unidades.");
            Assert.That(enemy.Position.X, Is.EqualTo(0f).Within(0.001f));
        }

        [Test]
        public void Never_overshoots_the_player()
        {
            var enemy = new Enemy(Slime, new Vector2(0f, 1f));

            // Velocidade 2 por 10 segundos: 20 unidades para andar 1.
            enemy.MoveToward(Vector2.Zero, deltaTime: 10f);

            Assert.That(enemy.Position, Is.EqualTo(Vector2.Zero),
                "Sem esta guarda o inimigo passaria do alvo e tremeria em volta dele.");
        }

        [Test]
        public void Stands_still_when_it_has_arrived()
        {
            var enemy = new Enemy(Slime, Vector2.Zero);

            enemy.MoveToward(Vector2.Zero, deltaTime: 1f);

            Assert.That(enemy.Position, Is.EqualTo(Vector2.Zero));
        }

        [Test]
        public void Chase_speed_does_not_depend_on_frame_rate()
        {
            var stuttering = new Enemy(Slime, new Vector2(0f, 100f));
            var smooth = new Enemy(Slime, new Vector2(0f, 100f));

            for (var i = 0; i < 60; i++)
                stuttering.MoveToward(Vector2.Zero, 1f / 60f);
            smooth.MoveToward(Vector2.Zero, 1f);

            Assert.That(stuttering.Position.Y, Is.EqualTo(smooth.Position.Y).Within(0.001f));
        }
    }
}
