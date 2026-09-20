using ScriptableSurvivors.Domain.Combat;
using System;
using NUnit.Framework;

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

        [Test]
        public void Announces_the_damage_it_took()
        {
            var health = new Health(100f);
            var announced = 0f;
            health.Damaged += amount => announced += amount;

            health.TakeDamage(30f);

            Assert.That(announced, Is.EqualTo(30f),
                "É este aviso que faz o corpo se contorcer na tela.");
        }

        [Test]
        public void Announces_only_what_was_actually_taken()
        {
            var health = new Health(10f);
            var announced = 0f;
            health.Damaged += amount => announced += amount;

            health.TakeDamage(999f);

            Assert.That(announced, Is.EqualTo(10f),
                "Sobrou dano, mas só 10 foram tirados.");
        }

        [Test]
        public void Hitting_a_corpse_announces_nothing()
        {
            var health = new Health(10f);
            health.TakeDamage(10f);
            var announced = 0;
            health.Damaged += _ => announced++;

            health.TakeDamage(50f);

            Assert.That(announced, Is.Zero);
        }

        [Test]
        public void Zero_damage_announces_nothing()
        {
            var health = new Health(100f);
            var announced = 0;
            health.Damaged += _ => announced++;

            health.TakeDamage(0f);

            Assert.That(announced, Is.Zero);
        }
    }
}
