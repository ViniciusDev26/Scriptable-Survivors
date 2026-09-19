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
        private static EnemyStats Standing => new EnemyStats(20f, speed: 0f, damage: 5f, xpReward: 3);

        private static Arena ArenaWithPlayerSpeed(float speed) =>
            new Arena(new Player(speed, 100f), contactRadius: 1.5f);

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
            Assert.Throws<ArgumentNullException>(() => new Arena(null, contactRadius: 1.5f));
        }

        [Test]
        public void Rejects_a_null_enemy()
        {
            var arena = ArenaWithPlayerSpeed(10f);

            Assert.Throws<ArgumentNullException>(() => arena.Add(null));
        }

        [Test]
        public void An_enemy_in_contact_wears_the_player_down()
        {
            var arena = new Arena(new Player(10f, 100f), contactRadius: 1.5f);
            arena.Add(new Enemy(Slime, new Vector2(1f, 0f)));

            arena.Tick(Vector2.Zero, 0f, deltaTime: 1f);

            // Dano 5 por segundo, um segundo encostado.
            Assert.That(arena.Player.Health.Current, Is.EqualTo(95f).Within(Tolerance));
        }

        [Test]
        public void An_enemy_out_of_reach_does_nothing()
        {
            var arena = new Arena(new Player(10f, 100f), contactRadius: 1.5f);
            arena.Add(new Enemy(new EnemyStats(20f, speed: 0f, damage: 5f, xpReward: 3),
                                new Vector2(40f, 0f)));

            arena.Tick(Vector2.Zero, 0f, 1f);

            Assert.That(arena.Player.Health.Current, Is.EqualTo(100f));
        }

        [Test]
        public void Damage_depends_on_time_touched_not_on_frame_rate()
        {
            var sixtyFps = new Arena(new Player(10f, 100f), contactRadius: 1.5f);
            var tenFps = new Arena(new Player(10f, 100f), contactRadius: 1.5f);
            sixtyFps.Add(new Enemy(Standing, Vector2.Zero));
            tenFps.Add(new Enemy(Standing, Vector2.Zero));

            for (var i = 0; i < 60; i++)
                sixtyFps.Tick(Vector2.Zero, 0f, 1f / 60f);
            for (var i = 0; i < 10; i++)
                tenFps.Tick(Vector2.Zero, 0f, 1f / 10f);

            Assert.That(sixtyFps.Player.Health.Current,
                Is.EqualTo(tenFps.Player.Health.Current).Within(0.01f),
                "Se o dano fosse por quadro, jogar a 120 fps seria o dobro de difícil.");
        }

        [Test]
        public void A_crowd_hurts_more_than_one()
        {
            var alone = new Arena(new Player(10f, 100f), contactRadius: 1.5f);
            var swarmed = new Arena(new Player(10f, 100f), contactRadius: 1.5f);
            alone.Add(new Enemy(Standing, Vector2.Zero));
            for (var i = 0; i < 3; i++)
                swarmed.Add(new Enemy(Standing, Vector2.Zero));

            alone.Tick(Vector2.Zero, 0f, 1f);
            swarmed.Tick(Vector2.Zero, 0f, 1f);

            Assert.That(100f - swarmed.Player.Health.Current,
                Is.EqualTo((100f - alone.Player.Health.Current) * 3f).Within(Tolerance));
        }

        [Test]
        public void The_run_ends_when_the_player_dies()
        {
            var arena = new Arena(new Player(10f, maxHealth: 4f), contactRadius: 1.5f);
            arena.Add(new Enemy(Standing, Vector2.Zero));

            arena.Tick(Vector2.Zero, 0f, deltaTime: 1f);

            Assert.That(arena.IsOver, Is.True);
        }

        [Test]
        public void Nothing_moves_after_the_run_is_over()
        {
            var arena = new Arena(new Player(10f, maxHealth: 4f), contactRadius: 1.5f);
            arena.Add(new Enemy(Standing, new Vector2(0.5f, 0f)));
            arena.Tick(Vector2.Zero, 0f, 1f);
            var restingPlace = arena.Enemies[0].Position;

            arena.Tick(D, 0f, 5f);

            Assert.That(arena.Player.Position, Is.EqualTo(Vector2.Zero),
                "O jogador morto não anda.");
            Assert.That(arena.Enemies[0].Position, Is.EqualTo(restingPlace));
        }

        [Test]
        public void Rejects_an_arena_with_no_contact_radius()
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () => new Arena(new Player(10f, 100f), contactRadius: 0f));
        }
    }
}
