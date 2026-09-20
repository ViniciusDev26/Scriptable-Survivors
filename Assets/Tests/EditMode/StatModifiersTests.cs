using NUnit.Framework;
using ScriptableSurvivors.Domain;

namespace ScriptableSurvivors.Tests
{
    public sealed class StatModifiersTests
    {
        private const float Tolerance = 0.001f;

        private static Upgrade Percent(StatKind stat, float value) =>
            new Upgrade("teste", stat, ModifierKind.Percent, value);

        private static Upgrade Flat(StatKind stat, float value) =>
            new Upgrade("teste", stat, ModifierKind.Flat, value);

        private static WeaponStats Pistol =>
            new WeaponStats(damage: 10f, shotsPerSecond: 2f, projectileSpeed: 20f, range: 12f);

        [Test]
        public void An_empty_stack_changes_nothing()
        {
            var stats = new StatModifiers().ApplyTo(Pistol);

            Assert.That(stats.Damage, Is.EqualTo(10f));
            Assert.That(stats.ShotsPerSecond, Is.EqualTo(2f));
        }

        [Test]
        public void Percentages_add_up_instead_of_compounding()
        {
            var modifiers = new StatModifiers();
            for (var i = 0; i < 5; i++)
                modifiers.Add(Percent(StatKind.WeaponDamage, 0.3f));

            Assert.That(modifiers.ApplyTo(Pistol).Damage, Is.EqualTo(25f).Within(Tolerance),
                "Cinco vezes +30% devem dar +150% sobre a base, não 1,3 elevado a 5.");
        }

        [Test]
        public void The_base_is_never_touched()
        {
            var modifiers = new StatModifiers();
            var original = Pistol;

            modifiers.Add(Percent(StatKind.WeaponDamage, 2f));
            var boosted = modifiers.ApplyTo(original);

            Assert.That(original.Damage, Is.EqualTo(10f),
                "É esta separação que permite mostrar o asset e a run lado a lado.");
            Assert.That(boosted.Damage, Is.EqualTo(30f).Within(Tolerance));
        }

        [Test]
        public void Flat_is_applied_before_percent()
        {
            var modifiers = new StatModifiers();
            modifiers.Add(Flat(StatKind.WeaponDamage, 10f));
            modifiers.Add(Percent(StatKind.WeaponDamage, 1f));

            Assert.That(modifiers.ApplyTo(Pistol).Damage, Is.EqualTo(40f).Within(Tolerance),
                "(10 base + 10 fixo) x 2 = 40. A ordem precisa ser estável para dar para balancear.");
        }

        [Test]
        public void Each_stat_has_its_own_pile()
        {
            var modifiers = new StatModifiers();
            modifiers.Add(Percent(StatKind.WeaponDamage, 1f));

            var stats = modifiers.ApplyTo(Pistol);

            Assert.That(stats.Damage, Is.EqualTo(20f).Within(Tolerance));
            Assert.That(stats.Range, Is.EqualTo(12f), "O alcance não foi tocado.");
        }

        [Test]
        public void A_ruinous_stack_still_makes_a_usable_weapon()
        {
            var modifiers = new StatModifiers();
            modifiers.Add(Percent(StatKind.WeaponFireRate, -5f));
            modifiers.Add(Percent(StatKind.WeaponDamage, -9f));
            modifiers.Add(Flat(StatKind.WeaponRange, -100f));

            var stats = modifiers.ApplyTo(Pistol);

            Assert.That(stats.ShotsPerSecond, Is.GreaterThan(0f));
            Assert.That(stats.Damage, Is.GreaterThanOrEqualTo(0f));
            Assert.That(stats.Range, Is.GreaterThan(0f),
                "Uma combinação ruim de upgrades não pode derrubar a run com exceção.");
        }

        [Test]
        public void Lists_the_stats_the_upgrades_touched()
        {
            var modifiers = new StatModifiers();
            modifiers.Add(Percent(StatKind.WeaponDamage, 0.3f));
            modifiers.Add(Percent(StatKind.PlayerSpeed, 0.2f));

            Assert.That(modifiers.ActiveStats,
                Is.EqualTo(new[] { StatKind.WeaponDamage, StatKind.PlayerSpeed }),
                "Na ordem em que foram escolhidos, que é como a HUD lista.");
        }

        [Test]
        public void The_same_stat_twice_is_listed_once()
        {
            var modifiers = new StatModifiers();
            modifiers.Add(Percent(StatKind.WeaponDamage, 0.3f));
            modifiers.Add(Percent(StatKind.WeaponDamage, 0.3f));
            modifiers.Add(Flat(StatKind.WeaponDamage, 5f));

            Assert.That(modifiers.ActiveStats, Has.Count.EqualTo(1),
                "Três upgrades de dano são uma linha na HUD, não três.");
            Assert.That(modifiers.PercentOn(StatKind.WeaponDamage), Is.EqualTo(0.6f).Within(0.001f));
            Assert.That(modifiers.FlatOn(StatKind.WeaponDamage), Is.EqualTo(5f));
        }

        [Test]
        public void Version_marks_every_change()
        {
            var modifiers = new StatModifiers();
            var before = modifiers.Version;

            modifiers.Add(Percent(StatKind.WeaponDamage, 0.3f));

            Assert.That(modifiers.Version, Is.GreaterThan(before),
                "É por isto que a HUD sabe quando remontar o texto.");
        }
    }
}
