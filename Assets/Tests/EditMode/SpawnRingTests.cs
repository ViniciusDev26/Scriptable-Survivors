using System;
using System.Numerics;
using NUnit.Framework;
using ScriptableSurvivors.Domain;

namespace ScriptableSurvivors.Tests
{
    public sealed class SpawnRingTests
    {
        private const float Tolerance = 0.001f;

        [Test]
        public void Spawns_exactly_on_the_ring_wherever_the_player_is()
        {
            var ring = new SpawnRing(25f);
            var player = new Vector2(13f, -7f);
            var random = new Random(1234);

            for (var i = 0; i < 500; i++)
            {
                var point = ring.NextPoint(player, random);

                Assert.That((point - player).Length(), Is.EqualTo(25f).Within(Tolerance));
            }
        }

        [Test]
        public void Never_spawns_inside_the_camera_view()
        {
            // A câmera ortográfica enxerga, na pior das hipóteses, até a
            // diagonal do enquadramento. O raio maior garante que ninguém
            // aparece do nada na frente do jogador.
            const float viewDiagonal = 21f;
            var ring = new SpawnRing(25f);
            var player = Vector2.Zero;
            var random = new Random(99);

            for (var i = 0; i < 500; i++)
            {
                var point = ring.NextPoint(player, random);

                Assert.That(point.Length(), Is.GreaterThan(viewDiagonal),
                    "Um inimigo nasceu dentro do campo de visão — apareceria do nada na tela.");
            }
        }

        [Test]
        public void Spreads_around_the_whole_circle()
        {
            var ring = new SpawnRing(10f);
            var random = new Random(7);
            var quadrants = new bool[4];

            for (var i = 0; i < 500; i++)
            {
                var p = ring.NextPoint(Vector2.Zero, random);
                var index = (p.X >= 0f ? 0 : 1) + (p.Y >= 0f ? 0 : 2);
                quadrants[index] = true;
            }

            Assert.That(quadrants, Is.All.True,
                "Os inimigos se concentraram num lado só da arena.");
        }

        [Test]
        public void Same_seed_replays_the_same_run()
        {
            var ring = new SpawnRing(10f);

            var first = ring.NextPoint(Vector2.Zero, new Random(42));
            var second = ring.NextPoint(Vector2.Zero, new Random(42));

            Assert.That(first, Is.EqualTo(second),
                "Mesma semente deve dar a mesma run — é o que torna um bug reproduzível.");
        }

        [Test]
        public void Angle_zero_points_east()
        {
            var ring = new SpawnRing(10f);

            var point = ring.PointAt(Vector2.Zero, 0f);

            Assert.That(point.X, Is.EqualTo(10f).Within(Tolerance));
            Assert.That(point.Y, Is.EqualTo(0f).Within(Tolerance));
        }

        [Test]
        public void Rejects_a_ring_with_no_room()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new SpawnRing(0f));
            Assert.Throws<ArgumentOutOfRangeException>(() => new SpawnRing(-5f));
        }
    }
}
