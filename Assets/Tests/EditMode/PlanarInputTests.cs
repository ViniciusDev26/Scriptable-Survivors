using System.Numerics;
using NUnit.Framework;
using ScriptableSurvivors.Domain;

namespace ScriptableSurvivors.Tests
{
    public sealed class PlanarInputTests
    {
        private const float Tolerance = 0.0001f;
        private static readonly float Diagonal = 0.70710678f;

        private static readonly Vector2 W = new Vector2(0f, 1f);
        private static readonly Vector2 D = new Vector2(1f, 0f);

        [Test]
        public void No_input_means_no_direction()
        {
            Assert.That(PlanarInput.ToWorldDirection(Vector2.Zero, 45f), Is.EqualTo(Vector2.Zero));
        }

        [Test]
        public void With_camera_unrotated_W_points_north()
        {
            var direction = PlanarInput.ToWorldDirection(W, 0f);

            Assert.That(direction.X, Is.EqualTo(0f).Within(Tolerance));
            Assert.That(direction.Y, Is.EqualTo(1f).Within(Tolerance));
        }

        [Test]
        public void With_camera_at_45_W_points_northeast()
        {
            var direction = PlanarInput.ToWorldDirection(W, 45f);

            Assert.That(direction.X, Is.EqualTo(Diagonal).Within(Tolerance));
            Assert.That(direction.Y, Is.EqualTo(Diagonal).Within(Tolerance));
        }

        [Test]
        public void With_camera_at_45_D_points_southeast()
        {
            var direction = PlanarInput.ToWorldDirection(D, 45f);

            Assert.That(direction.X, Is.EqualTo(Diagonal).Within(Tolerance));
            Assert.That(direction.Y, Is.EqualTo(-Diagonal).Within(Tolerance));
        }

        [Test]
        public void Rotation_never_changes_speed()
        {
            foreach (var yaw in new[] { 0f, 30f, 45f, 90f, 180f, 270f, -45f })
            {
                var direction = PlanarInput.ToWorldDirection(W, yaw);

                Assert.That(direction.Length(), Is.EqualTo(1f).Within(Tolerance),
                    $"A rotação da câmera alterou a velocidade com yaw {yaw}.");
            }
        }

        [Test]
        public void Diagonal_on_keyboard_is_not_faster_than_cardinal()
        {
            var cardinal = PlanarInput.ToWorldDirection(W, 45f);
            var diagonal = PlanarInput.ToWorldDirection(new Vector2(1f, 1f), 45f);

            Assert.That(diagonal.Length(), Is.EqualTo(cardinal.Length()).Within(Tolerance),
                "W+D somam (1,1), de comprimento 1,41. Sem normalizar, a diagonal " +
                "seria 41% mais rápida — o bug clássico de movimentação em 8 direções.");
        }

        [Test]
        public void Analog_input_keeps_its_magnitude()
        {
            var halfPush = new Vector2(0f, 0.5f);

            var direction = PlanarInput.ToWorldDirection(halfPush, 45f);

            Assert.That(direction.Length(), Is.EqualTo(0.5f).Within(Tolerance),
                "Normalizar sempre quebraria o analógico: meio empurrão viraria corrida.");
        }
    }
}
