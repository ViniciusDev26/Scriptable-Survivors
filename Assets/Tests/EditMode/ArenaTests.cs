using System;
using System.Numerics;
using NUnit.Framework;
using ScriptableSurvivors.Domain;

namespace ScriptableSurvivors.Tests
{
    public sealed class ArenaTests
    {
        private const float Tolerance = 0.001f;

        private static readonly Vector2 D = new Vector2(1f, 0f);

        private static EnemyStats Slime => new EnemyStats(20f, speed: 5f, damage: 5f, xpReward: 3);
        private static EnemyStats Standing => new EnemyStats(20f, speed: 0f, damage: 5f, xpReward: 3);
        private static EnemyStats Frail => new EnemyStats(1f, speed: 0f, damage: 0f, xpReward: 7);

        /// <summary>Arma inofensiva e de alcance nulo, para não interferir nos
        /// testes que não são sobre ela.</summary>
        private static WeaponStats Unarmed =>
            new WeaponStats(damage: 0f, shotsPerSecond: 1f, projectileSpeed: 1f, range: 0.001f);

        private static WeaponStats Pistol =>
            new WeaponStats(damage: 10f, shotsPerSecond: 2f, projectileSpeed: 20f, range: 12f);

        private static Arena Make(WeaponStats weapon, float playerSpeed = 10f, float maxHealth = 100f) =>
            new Arena(new Player(playerSpeed, maxHealth), new Weapon(weapon),
                      contactRadius: 1.5f, hitRadius: 0.8f);

        private static Arena Peaceful(float playerSpeed = 10f, float maxHealth = 100f) =>
            Make(Unarmed, playerSpeed, maxHealth);

        // ---------- movimento ----------

        [Test]
        public void Starts_empty()
        {
            var arena = Peaceful();

            Assert.That(arena.Enemies, Is.Empty);
            Assert.That(arena.Projectiles, Is.Empty);
            Assert.That(arena.Kills, Is.Zero);
        }

        [Test]
        public void Tick_moves_the_player()
        {
            var arena = Peaceful();

            arena.Tick(D, cameraYawDegrees: 0f, deltaTime: 1f);

            Assert.That(arena.Player.Position.X, Is.EqualTo(10f).Within(Tolerance));
        }

        [Test]
        public void Tick_walks_every_enemy_toward_the_player()
        {
            var arena = Peaceful();
            arena.Add(new Enemy(Slime, new Vector2(0f, 20f)));
            arena.Add(new Enemy(Slime, new Vector2(0f, 30f)));

            arena.Tick(Vector2.Zero, 0f, 1f);

            Assert.That(arena.Enemies[0].Position.Y, Is.EqualTo(15f).Within(Tolerance));
            Assert.That(arena.Enemies[1].Position.Y, Is.EqualTo(25f).Within(Tolerance));
        }

        [Test]
        public void Enemies_chase_where_the_player_is_now_not_where_he_was()
        {
            var arena = Peaceful();
            arena.Add(new Enemy(Slime, new Vector2(0f, 20f)));

            arena.Tick(D, cameraYawDegrees: 0f, deltaTime: 1f);

            Assert.That(arena.Player.Position.X, Is.EqualTo(10f).Within(Tolerance));
            Assert.That(arena.Enemies[0].Position.X, Is.GreaterThan(0.5f),
                "O inimigo perseguiu a posição do quadro anterior. A ordem dentro " +
                "de Tick é a regra: o jogador se move primeiro.");
        }

        // ---------- contato ----------

        [Test]
        public void An_enemy_in_contact_wears_the_player_down()
        {
            var arena = Peaceful();
            arena.Add(new Enemy(Slime, new Vector2(1f, 0f)));

            arena.Tick(Vector2.Zero, 0f, deltaTime: 1f);

            Assert.That(arena.Player.Health.Current, Is.EqualTo(95f).Within(Tolerance));
        }

        [Test]
        public void An_enemy_out_of_reach_does_nothing()
        {
            var arena = Peaceful();
            arena.Add(new Enemy(Standing, new Vector2(40f, 0f)));

            arena.Tick(Vector2.Zero, 0f, 1f);

            Assert.That(arena.Player.Health.Current, Is.EqualTo(100f));
        }

        [Test]
        public void Damage_depends_on_time_touched_not_on_frame_rate()
        {
            var sixtyFps = Peaceful();
            var tenFps = Peaceful();
            sixtyFps.Add(new Enemy(Standing, Vector2.Zero));
            tenFps.Add(new Enemy(Standing, Vector2.Zero));

            for (var i = 0; i < 60; i++)
                sixtyFps.Tick(Vector2.Zero, 0f, 1f / 60f);
            for (var i = 0; i < 10; i++)
                tenFps.Tick(Vector2.Zero, 0f, 1f / 10f);

            Assert.That(sixtyFps.Player.Health.Current,
                Is.EqualTo(tenFps.Player.Health.Current).Within(0.01f),
                "Se o dano fosse por quadro, jogar a 120 fps seria o dobro de difícil.");
        }

        [Test]
        public void A_crowd_hurts_more_than_one()
        {
            var alone = Peaceful();
            var swarmed = Peaceful();
            alone.Add(new Enemy(Standing, Vector2.Zero));
            for (var i = 0; i < 3; i++)
                swarmed.Add(new Enemy(Standing, Vector2.Zero));

            alone.Tick(Vector2.Zero, 0f, 1f);
            swarmed.Tick(Vector2.Zero, 0f, 1f);

            Assert.That(100f - swarmed.Player.Health.Current,
                Is.EqualTo((100f - alone.Player.Health.Current) * 3f).Within(Tolerance));
        }

        [Test]
        public void The_run_ends_when_the_player_dies()
        {
            var arena = Peaceful(maxHealth: 4f);
            arena.Add(new Enemy(Standing, Vector2.Zero));

            arena.Tick(Vector2.Zero, 0f, deltaTime: 1f);

            Assert.That(arena.IsOver, Is.True);
        }

        [Test]
        public void Nothing_moves_after_the_run_is_over()
        {
            var arena = Peaceful(maxHealth: 4f);
            arena.Add(new Enemy(Standing, new Vector2(0.5f, 0f)));
            arena.Tick(Vector2.Zero, 0f, 1f);
            var restingPlace = arena.Enemies[0].Position;

            arena.Tick(D, 0f, 5f);

            Assert.That(arena.Player.Position, Is.EqualTo(Vector2.Zero), "O jogador morto não anda.");
            Assert.That(arena.Enemies[0].Position, Is.EqualTo(restingPlace));
        }

        // ---------- mira e tiro ----------

        [Test]
        public void Aims_at_the_nearest_enemy()
        {
            var arena = Make(Pistol);
            var far = new Enemy(Standing, new Vector2(0f, 9f));
            var near = new Enemy(Standing, new Vector2(0f, 3f));
            arena.Add(far);
            arena.Add(near);

            Assert.That(arena.NearestEnemyWithin(12f), Is.SameAs(near));
        }

        [Test]
        public void Ignores_enemies_beyond_range()
        {
            var arena = Make(Pistol);
            arena.Add(new Enemy(Standing, new Vector2(0f, 40f)));

            Assert.That(arena.NearestEnemyWithin(12f), Is.Null);
        }

        [Test]
        public void Holds_fire_when_nothing_is_in_range()
        {
            var arena = Make(Pistol);
            arena.Add(new Enemy(Standing, new Vector2(0f, 40f)));

            arena.Tick(Vector2.Zero, 0f, 1f / 60f);

            Assert.That(arena.Projectiles, Is.Empty);
        }

        [Test]
        public void Fires_on_its_own_at_an_enemy_in_range()
        {
            var arena = Make(Pistol);
            arena.Add(new Enemy(Standing, new Vector2(0f, 5f)));

            arena.Tick(Vector2.Zero, 0f, 1f / 60f);

            Assert.That(arena.Projectiles, Has.Count.EqualTo(1));
            Assert.That(arena.Projectiles[0].Direction.Y, Is.EqualTo(1f).Within(Tolerance));
        }

        [Test]
        public void Announces_every_shot_it_fires()
        {
            var arena = Make(Pistol);
            arena.Add(new Enemy(Standing, new Vector2(0f, 5f)));
            var announced = 0;
            arena.ProjectileFired += _ => announced++;

            arena.Tick(Vector2.Zero, 0f, 1f / 60f);

            Assert.That(announced, Is.EqualTo(1),
                "O adaptador cria o corpo do projétil ao ouvir este evento.");
        }

        [Test]
        public void Respects_the_cadence()
        {
            var arena = Make(Pistol);
            arena.Add(new Enemy(new EnemyStats(10000f, 0f, 0f, 1), new Vector2(0f, 5f)));
            var shots = 0;
            arena.ProjectileFired += _ => shots++;

            // Dois tiros por segundo, durante dois segundos.
            for (var i = 0; i < 120; i++)
                arena.Tick(Vector2.Zero, 0f, 1f / 60f);

            Assert.That(shots, Is.EqualTo(4).Within(1),
                "Cadência 2 por segundo em 2 segundos são 4 tiros. Sem o cooldown " +
                "a arma dispararia uma vez por quadro: 120.");
        }

        // ---------- morte e contagem ----------

        [Test]
        public void A_shot_that_connects_kills_a_frail_enemy()
        {
            var arena = Make(Pistol);
            arena.Add(new Enemy(Frail, new Vector2(0f, 3f)));

            for (var i = 0; i < 60; i++)
                arena.Tick(Vector2.Zero, 0f, 1f / 60f);

            Assert.That(arena.Kills, Is.EqualTo(1));
        }

        [Test]
        public void The_dead_leave_the_arena()
        {
            var arena = Make(Pistol);
            arena.Add(new Enemy(Frail, new Vector2(0f, 3f)));

            for (var i = 0; i < 60; i++)
                arena.Tick(Vector2.Zero, 0f, 1f / 60f);

            Assert.That(arena.Enemies, Is.Empty,
                "Inimigo morto continua sendo perseguidor e alvo se não sair da lista.");
        }

        // ---------- construção ----------

        [Test]
        public void Rejects_an_arena_without_a_player()
        {
            Assert.Throws<ArgumentNullException>(
                () => new Arena(null, new Weapon(Pistol), 1.5f, 0.8f));
        }

        [Test]
        public void Rejects_an_arena_without_a_weapon()
        {
            Assert.Throws<ArgumentNullException>(
                () => new Arena(new Player(10f, 100f), null, 1.5f, 0.8f));
        }

        [Test]
        public void Rejects_radii_that_make_no_sense()
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () => new Arena(new Player(10f, 100f), new Weapon(Pistol), contactRadius: 0f, hitRadius: 0.8f));
            Assert.Throws<ArgumentOutOfRangeException>(
                () => new Arena(new Player(10f, 100f), new Weapon(Pistol), contactRadius: 1.5f, hitRadius: 0f));
        }

        [Test]
        public void Rejects_a_null_enemy()
        {
            Assert.Throws<ArgumentNullException>(() => Peaceful().Add(null));
        }
    }
}
