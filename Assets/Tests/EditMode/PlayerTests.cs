using ScriptableSurvivors.Domain.Combat;
using ScriptableSurvivors.Domain.Players;
using System;
using System.Numerics;
using NUnit.Framework;

namespace ScriptableSurvivors.Tests
{
    public sealed class PlayerTests
    {
        private const float Tolerance = 0.0001f;
        private static readonly Vector2 W = new Vector2(0f, 1f);

        [Test]
        public void Starts_where_it_was_placed()
        {
            var movement = new Player(5f, 100f, new Vector2(3f, -2f));

            Assert.That(movement.Position, Is.EqualTo(new Vector2(3f, -2f)));
        }

        [Test]
        public void Travels_speed_times_elapsed_time()
        {
            var movement = new Player(10f, 100f);

            movement.Move(W, cameraYawDegrees: 0f, deltaTime: 0.5f);

            Assert.That(movement.Position.Y, Is.EqualTo(5f).Within(Tolerance));
        }

        [Test]
        public void Idle_input_does_not_drift()
        {
            var movement = new Player(10f, 100f, new Vector2(1f, 1f));

            movement.Move(Vector2.Zero, 45f, 1f);

            Assert.That(movement.Position, Is.EqualTo(new Vector2(1f, 1f)));
        }

        [Test]
        public void Diagonal_covers_the_same_distance_as_cardinal()
        {
            var straight = new Player(10f, 100f);
            var diagonal = new Player(10f, 100f);

            straight.Move(W, 45f, 1f);
            diagonal.Move(new Vector2(1f, 1f), 45f, 1f);

            Assert.That(diagonal.Position.Length(), Is.EqualTo(straight.Position.Length()).Within(Tolerance));
        }

        [Test]
        public void Many_small_steps_match_one_big_step()
        {
            var stuttering = new Player(10f, 100f);
            var smooth = new Player(10f, 100f);

            for (var i = 0; i < 100; i++)
                stuttering.Move(W, 45f, 0.01f);
            smooth.Move(W, 45f, 1f);

            Assert.That(stuttering.Position.X, Is.EqualTo(smooth.Position.X).Within(0.001f));
            Assert.That(stuttering.Position.Y, Is.EqualTo(smooth.Position.Y).Within(0.001f),
                "A distância percorrida não pode depender da taxa de quadros.");
        }

        [Test]
        public void Rejects_non_positive_speed()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new Player(0f, 100f));
            Assert.Throws<ArgumentOutOfRangeException>(() => new Player(-1f, 100f));
        }

        [Test]
        public void Starts_at_full_health()
        {
            var player = new Player(10f, maxHealth: 80f);

            Assert.That(player.Health.Current, Is.EqualTo(80f));
            Assert.That(player.Health.IsDead, Is.False);
        }

        [Test]
        public void Rejects_a_player_that_cannot_live()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new Player(10f, maxHealth: 0f));
        }
    }
}
