using System;
using System.Numerics;
using NUnit.Framework;
using ScriptableSurvivors.Domain;

namespace ScriptableSurvivors.Tests
{
    public sealed class WeaponTests
    {
        private const float Tolerance = 0.001f;

        private static WeaponStats Pistol =>
            new WeaponStats(damage: 10f, shotsPerSecond: 2f, projectileSpeed: 20f, range: 12f);

        [Test]
        public void Cadence_becomes_the_interval_between_shots()
        {
            Assert.That(Pistol.Cooldown, Is.EqualTo(0.5f).Within(Tolerance),
                "Dois tiros por segundo é um tiro a cada meio segundo.");
        }

        [Test]
        public void Starts_ready_to_fire()
        {
            Assert.That(new Weapon(Pistol).IsReady, Is.True);
        }

        [Test]
        public void Needs_to_reload_after_firing()
        {
            var weapon = new Weapon(Pistol);

            weapon.Fire(Vector2.Zero, new Vector2(5f, 0f));

            Assert.That(weapon.IsReady, Is.False);
        }

        [Test]
        public void Is_ready_again_once_the_cadence_elapses()
        {
            var weapon = new Weapon(Pistol);
            weapon.Fire(Vector2.Zero, new Vector2(5f, 0f));

            weapon.Cool(0.5f);

            Assert.That(weapon.IsReady, Is.True);
        }

        [Test]
        public void Refuses_to_fire_while_reloading()
        {
            var weapon = new Weapon(Pistol);
            weapon.Fire(Vector2.Zero, new Vector2(5f, 0f));

            Assert.Throws<InvalidOperationException>(
                () => weapon.Fire(Vector2.Zero, new Vector2(5f, 0f)));
        }

        [Test]
        public void The_shot_carries_the_weapon_numbers()
        {
            var projectile = new Weapon(Pistol).Fire(Vector2.Zero, new Vector2(5f, 0f));

            Assert.That(projectile.Damage, Is.EqualTo(10f));
            Assert.That(projectile.Speed, Is.EqualTo(20f));
            Assert.That(projectile.RemainingRange, Is.EqualTo(12f));
        }

        [Test]
        public void The_shot_flies_toward_the_target()
        {
            var projectile = new Weapon(Pistol).Fire(Vector2.Zero, new Vector2(0f, 7f));

            Assert.That(projectile.Direction.X, Is.EqualTo(0f).Within(Tolerance));
            Assert.That(projectile.Direction.Y, Is.EqualTo(1f).Within(Tolerance));
        }

        [Test]
        public void A_target_standing_on_the_player_does_not_crash_the_game()
        {
            var weapon = new Weapon(Pistol);

            Assert.DoesNotThrow(() => weapon.Fire(Vector2.Zero, Vector2.Zero),
                "Inimigos param exatamente em cima do jogador. Qualquer direção " +
                "acerta nesse caso, e lançar aqui travaria o jogo no pior momento.");
        }

        [Test]
        public void Refuses_numbers_that_make_no_weapon()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new WeaponStats(10f, 0f, 20f, 12f));
            Assert.Throws<ArgumentOutOfRangeException>(() => new WeaponStats(10f, 2f, 0f, 12f));
            Assert.Throws<ArgumentOutOfRangeException>(() => new WeaponStats(10f, 2f, 20f, 0f));
            Assert.Throws<ArgumentOutOfRangeException>(() => new WeaponStats(-1f, 2f, 20f, 12f));
        }
    }
}
