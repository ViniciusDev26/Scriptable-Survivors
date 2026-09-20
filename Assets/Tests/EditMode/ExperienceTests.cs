using System;
using NUnit.Framework;
using ScriptableSurvivors.Domain;

namespace ScriptableSurvivors.Tests
{
    public sealed class ExperienceTests
    {
        [Test]
        public void Starts_at_level_one_with_nothing()
        {
            var xp = new Experience(firstLevelCost: 5, costIncrease: 3);

            Assert.That(xp.Level, Is.EqualTo(1));
            Assert.That(xp.Current, Is.Zero);
            Assert.That(xp.Required, Is.EqualTo(5));
            Assert.That(xp.PendingLevelUps, Is.Zero);
        }

        [Test]
        public void Levels_up_on_reaching_the_requirement()
        {
            var xp = new Experience(5, 3);

            xp.Add(5);

            Assert.That(xp.Level, Is.EqualTo(2));
            Assert.That(xp.PendingLevelUps, Is.EqualTo(1));
        }

        [Test]
        public void Leftover_xp_carries_into_the_next_level()
        {
            var xp = new Experience(5, 3);

            xp.Add(7);

            Assert.That(xp.Level, Is.EqualTo(2));
            Assert.That(xp.Current, Is.EqualTo(2), "Sobraram 2 dos 7.");
        }

        [Test]
        public void Each_level_costs_more_than_the_last()
        {
            var xp = new Experience(5, 3);

            Assert.That(xp.Required, Is.EqualTo(5));
            xp.Add(5);
            Assert.That(xp.Required, Is.EqualTo(8));
            xp.Add(8);
            Assert.That(xp.Required, Is.EqualTo(11));
        }

        [Test]
        public void A_cannon_blast_can_grant_two_levels_at_once()
        {
            var xp = new Experience(5, 3);

            xp.Add(30);

            Assert.That(xp.PendingLevelUps, Is.GreaterThan(1),
                "Cada nível pendente pede sua própria carta — nenhum pode ser engolido.");
        }

        [Test]
        public void Level_ups_are_spent_one_at_a_time()
        {
            var xp = new Experience(5, 3);
            xp.Add(13);
            var granted = xp.PendingLevelUps;

            for (var i = 0; i < granted; i++)
                Assert.That(xp.TryConsumeLevelUp(), Is.True);

            Assert.That(xp.TryConsumeLevelUp(), Is.False);
        }

        [Test]
        public void Refuses_a_curve_that_makes_no_sense()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new Experience(0, 3));
            Assert.Throws<ArgumentOutOfRangeException>(() => new Experience(5, -1));
        }
    }
}
