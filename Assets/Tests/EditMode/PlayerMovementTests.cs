using System;
using System.Numerics;
using NUnit.Framework;
using ScriptableSurvivors.Domain;

namespace ScriptableSurvivors.Tests
{
    public sealed class PlayerMovementTests
    {
        private const float Tolerance = 0.0001f;
        private static readonly Vector2 W = new Vector2(0f, 1f);

        [Test]
        public void Starts_where_it_was_placed()
        {
            var movement = new PlayerMovement(5f, new Vector2(3f, -2f));

            Assert.That(movement.Position, Is.EqualTo(new Vector2(3f, -2f)));
        }

        [Test]
        public void Travels_speed_times_elapsed_time()
        {
            var movement = new PlayerMovement(10f);

            movement.Move(W, cameraYawDegrees: 0f, deltaTime: 0.5f);

            Assert.That(movement.Position.Y, Is.EqualTo(5f).Within(Tolerance));
        }

        [Test]
        public void Idle_input_does_not_drift()
        {
            var movement = new PlayerMovement(10f, new Vector2(1f, 1f));

            movement.Move(Vector2.Zero, 45f, 1f);

            Assert.That(movement.Position, Is.EqualTo(new Vector2(1f, 1f)));
        }

        [Test]
        public void Diagonal_covers_the_same_distance_as_cardinal()
        {
            var straight = new PlayerMovement(10f);
            var diagonal = new PlayerMovement(10f);

            straight.Move(W, 45f, 1f);
            diagonal.Move(new Vector2(1f, 1f), 45f, 1f);

            Assert.That(diagonal.Position.Length(), Is.EqualTo(straight.Position.Length()).Within(Tolerance));
        }

        [Test]
        public void Many_small_steps_match_one_big_step()
        {
            var stuttering = new PlayerMovement(10f);
            var smooth = new PlayerMovement(10f);

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
            Assert.Throws<ArgumentOutOfRangeException>(() => new PlayerMovement(0f));
            Assert.Throws<ArgumentOutOfRangeException>(() => new PlayerMovement(-1f));
        }
    }
}
