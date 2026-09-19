using System;
using System.Numerics;
using NUnit.Framework;
using ScriptableSurvivors.Domain;

namespace ScriptableSurvivors.Tests
{
    public sealed class ArenaTests
    {
        private const float Tolerance = 0.001f;

        private static readonly Vector2 D = new Vector2(1f, 0f);
        private static EnemyStats Slime => new EnemyStats(20f, speed: 5f, damage: 5f, xpReward: 3);

        private static Arena ArenaWithPlayerSpeed(float speed) =>
            new Arena(new PlayerMovement(speed));

        [Test]
        public void Starts_empty()
        {
            var arena = ArenaWithPlayerSpeed(10f);

            Assert.That(arena.Enemies, Is.Empty);
        }

        [Test]
        public void Tick_moves_the_player()
        {
            var arena = ArenaWithPlayerSpeed(10f);

            arena.Tick(D, cameraYawDegrees: 0f, deltaTime: 1f);

            Assert.That(arena.Player.Position.X, Is.EqualTo(10f).Within(Tolerance));
        }

        [Test]
        public void Tick_walks_every_enemy_toward_the_player()
        {
            var arena = ArenaWithPlayerSpeed(10f);
            arena.Add(new Enemy(Slime, new Vector2(0f, 20f)));
            arena.Add(new Enemy(Slime, new Vector2(0f, 30f)));

            arena.Tick(Vector2.Zero, 0f, 1f);

            Assert.That(arena.Enemies[0].Position.Y, Is.EqualTo(15f).Within(Tolerance));
            Assert.That(arena.Enemies[1].Position.Y, Is.EqualTo(25f).Within(Tolerance));
        }

        [Test]
        public void Enemies_chase_where_the_player_is_now_not_where_he_was()
        {
            var arena = ArenaWithPlayerSpeed(10f);
            arena.Add(new Enemy(Slime, new Vector2(0f, 20f)));

            // O jogador sai de (0,0) e termina o Tick em (10,0).
            arena.Tick(D, cameraYawDegrees: 0f, deltaTime: 1f);

            // Perseguindo (10,0): o inimigo desvia para a direita.
            // Perseguindo (0,0), a posição antiga: desceria reto, X continuaria 0.
            Assert.That(arena.Player.Position.X, Is.EqualTo(10f).Within(Tolerance));
            Assert.That(arena.Enemies[0].Position.X, Is.GreaterThan(0.5f),
                "O inimigo perseguiu a posição do quadro anterior. A ordem dentro " +
                "de Tick é a regra: o jogador se move primeiro.");
        }

        [Test]
        public void Rejects_an_arena_without_a_player()
        {
            Assert.Throws<ArgumentNullException>(() => new Arena(null));
        }

        [Test]
        public void Rejects_a_null_enemy()
        {
            var arena = ArenaWithPlayerSpeed(10f);

            Assert.Throws<ArgumentNullException>(() => arena.Add(null));
        }
    }
}
