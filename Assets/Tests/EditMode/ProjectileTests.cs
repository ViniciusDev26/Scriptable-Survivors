using System.Numerics;
using NUnit.Framework;
using ScriptableSurvivors.Domain;

namespace ScriptableSurvivors.Tests
{
    public sealed class ProjectileTests
    {
        private const float Tolerance = 0.001f;

        private static Projectile Shot(float range = 10f) =>
            new Projectile(Vector2.Zero, new Vector2(1f, 0f), speed: 5f, damage: 3f, range: range);

        [Test]
        public void Travels_speed_times_elapsed_time()
        {
            var shot = Shot();

            shot.Advance(1f);

            Assert.That(shot.Position.X, Is.EqualTo(5f).Within(Tolerance));
        }

        [Test]
        public void Direction_is_normalized_on_creation()
        {
            var shot = new Projectile(Vector2.Zero, new Vector2(99f, 0f), 5f, 3f, 10f);

            Assert.That(shot.Direction.Length(), Is.EqualTo(1f).Within(Tolerance),
                "Sem normalizar, um alvo distante faria o tiro voar mais rápido.");
        }

        [Test]
        public void Dies_at_the_edge_of_its_range()
        {
            var shot = Shot(range: 10f);

            shot.Advance(1.9f);
            Assert.That(shot.IsSpent, Is.False);

            shot.Advance(0.2f);
            Assert.That(shot.IsSpent, Is.True);
        }

        [Test]
        public void Never_flies_past_its_range()
        {
            var shot = Shot(range: 10f);

            shot.Advance(100f);

            Assert.That(shot.Position.X, Is.EqualTo(10f).Within(Tolerance),
                "Alcance é distância, não tempo de vida.");
        }

        [Test]
        public void A_spent_shot_stays_put()
        {
            var shot = Shot(range: 10f);
            shot.Consume();

            shot.Advance(1f);

            Assert.That(shot.Position, Is.EqualTo(Vector2.Zero));
        }

        [Test]
        public void Range_does_not_depend_on_frame_rate()
        {
            var stuttering = Shot(range: 10f);
            var smooth = Shot(range: 10f);

            for (var i = 0; i < 120; i++)
                stuttering.Advance(1f / 60f);
            smooth.Advance(2f);

            Assert.That(stuttering.Position.X, Is.EqualTo(smooth.Position.X).Within(Tolerance));
        }
    }
}
