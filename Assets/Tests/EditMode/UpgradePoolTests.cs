using ScriptableSurvivors.Domain.Progression;
using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace ScriptableSurvivors.Tests
{
    public sealed class UpgradePoolTests
    {
        private static UpgradePool PoolOf(int size)
        {
            var upgrades = new List<Upgrade>();
            for (var i = 0; i < size; i++)
                upgrades.Add(new Upgrade($"upgrade-{i}", StatKind.WeaponDamage, ModifierKind.Percent, 0.1f));

            return new UpgradePool(upgrades);
        }

        [Test]
        public void Draws_the_requested_number_of_cards()
        {
            Assert.That(PoolOf(12).Draw(3, new Random(1)), Has.Count.EqualTo(3));
        }

        [Test]
        public void Never_offers_the_same_card_twice_on_one_screen()
        {
            var pool = PoolOf(12);
            var random = new Random(7);

            for (var attempt = 0; attempt < 200; attempt++)
            {
                var drawn = pool.Draw(3, random);
                var ids = new HashSet<string>();

                foreach (var upgrade in drawn)
                    Assert.That(ids.Add(upgrade.Id), Is.True,
                        "Duas cartas iguais na mesma tela parecem bug para quem joga.");
            }
        }

        [Test]
        public void A_small_pool_gives_what_it_has()
        {
            Assert.That(PoolOf(2).Draw(3, new Random(1)), Has.Count.EqualTo(2));
        }

        [Test]
        public void Drawing_does_not_consume_the_pool()
        {
            var pool = PoolOf(5);
            var random = new Random(3);

            pool.Draw(3, random);
            pool.Draw(3, random);

            Assert.That(pool.Count, Is.EqualTo(5),
                "Uma carta já oferecida pode aparecer de novo no próximo nível.");
        }

        [Test]
        public void The_same_seed_offers_the_same_cards()
        {
            var pool = PoolOf(12);

            var first = pool.Draw(3, new Random(42));
            var second = pool.Draw(3, new Random(42));

            for (var i = 0; i < first.Count; i++)
                Assert.That(second[i].Id, Is.EqualTo(first[i].Id));
        }

        [Test]
        public void Spreads_across_the_whole_pool()
        {
            var pool = PoolOf(12);
            var random = new Random(11);
            var seen = new HashSet<string>();

            for (var attempt = 0; attempt < 300; attempt++)
                foreach (var upgrade in pool.Draw(3, random))
                    seen.Add(upgrade.Id);

            Assert.That(seen, Has.Count.EqualTo(12),
                "Um upgrade que nunca sai é conteúdo morto no projeto.");
        }

        [Test]
        public void Refuses_a_pool_or_a_draw_that_makes_no_sense()
        {
            Assert.Throws<ArgumentNullException>(() => new UpgradePool(null));
            Assert.Throws<ArgumentException>(() => new UpgradePool(new List<Upgrade>()));
            Assert.Throws<ArgumentOutOfRangeException>(() => PoolOf(5).Draw(0, new Random(1)));
            Assert.Throws<ArgumentNullException>(() => PoolOf(5).Draw(3, null));
        }
    }
}
