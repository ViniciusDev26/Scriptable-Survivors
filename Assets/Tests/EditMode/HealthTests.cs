using System;
using NUnit.Framework;
using ScriptableSurvivors.Domain;

namespace ScriptableSurvivors.Tests
{
    public sealed class HealthTests
    {
        [Test]
        public void Starts_full()
        {
            var health = new Health(100f);

            Assert.That(health.Current, Is.EqualTo(100f));
            Assert.That(health.IsDead, Is.False);
        }

        [Test]
        public void Damage_reduces_current_but_not_max()
        {
            var health = new Health(100f);

            health.TakeDamage(30f);

            Assert.That(health.Current, Is.EqualTo(70f));
            Assert.That(health.Max, Is.EqualTo(100f));
        }

        [Test]
        public void Damage_never_pushes_current_below_zero()
        {
            var health = new Health(10f);

            health.TakeDamage(999f);

            Assert.That(health.Current, Is.EqualTo(0f));
            Assert.That(health.IsDead, Is.True);
        }

        [Test]
        public void Heal_never_pushes_current_above_max()
        {
            var health = new Health(100f);
            health.TakeDamage(10f);

            health.Heal(999f);

            Assert.That(health.Current, Is.EqualTo(100f));
        }

        [Test]
        public void Two_instances_do_not_share_state()
        {
            var first = new Health(100f);
            var second = new Health(100f);

            first.TakeDamage(40f);

            Assert.That(second.Current, Is.EqualTo(100f),
                "Vida é estado de instância. Se este teste falhar, o estado virou compartilhado — " +
                "exatamente o bug que a demonstração do Dia 4 provoca de propósito.");
        }

        [Test]
        public void Rejects_non_positive_max()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new Health(0f));
            Assert.Throws<ArgumentOutOfRangeException>(() => new Health(-1f));
        }

        [Test]
        public void Rejects_negative_damage()
        {
            var health = new Health(100f);

            Assert.Throws<ArgumentOutOfRangeException>(() => health.TakeDamage(-5f));
        }
    }
}
