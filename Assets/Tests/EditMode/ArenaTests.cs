using ScriptableSurvivors.Domain;
using ScriptableSurvivors.Domain.Combat;
using ScriptableSurvivors.Domain.Enemies;
using ScriptableSurvivors.Domain.Players;
using ScriptableSurvivors.Domain.Progression;
using System;
using System.Numerics;
using NUnit.Framework;

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

        private static WeaponStats Cannon =>
            new WeaponStats(damage: 10f, shotsPerSecond: 1f, projectileSpeed: 15f,
                            range: 12f, splashRadius: 3f);

        private static Arena Make(WeaponStats weapon, float playerSpeed = 10f, float maxHealth = 100f) =>
            new Arena(new Player(playerSpeed, maxHealth), new[] { new Weapon(weapon) },
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
            arena.ProjectileFired += (_, _) => announced++;

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
            arena.ProjectileFired += (_, _) => shots++;

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
                () => new Arena(null, new[] { new Weapon(Pistol) }, 1.5f, 0.8f));
        }

        [Test]
        public void Rejects_an_arena_with_no_weapon_at_all()
        {
            Assert.Throws<ArgumentNullException>(
                () => new Arena(new Player(10f, 100f), null, 1.5f, 0.8f));
            Assert.Throws<ArgumentException>(
                () => new Arena(new Player(10f, 100f), new Weapon[0], 1.5f, 0.8f));
        }

        [Test]
        public void Rejects_radii_that_make_no_sense()
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () => new Arena(new Player(10f, 100f), new[] { new Weapon(Pistol) }, contactRadius: 0f, hitRadius: 0.8f));
            Assert.Throws<ArgumentOutOfRangeException>(
                () => new Arena(new Player(10f, 100f), new[] { new Weapon(Pistol) }, contactRadius: 1.5f, hitRadius: 0f));
        }

        // ---------- arsenal e dano em área ----------

        [Test]
        public void Every_weapon_in_the_loadout_fires_on_its_own()
        {
            var arena = new Arena(
                new Player(10f, 100f),
                new[] { new Weapon(Pistol), new Weapon(Cannon) },
                contactRadius: 1.5f, hitRadius: 0.8f);
            arena.Add(new Enemy(new EnemyStats(10000f, 0f, 0f, 1), new Vector2(0f, 5f)));

            arena.Tick(Vector2.Zero, 0f, 1f / 60f);

            Assert.That(arena.Projectiles, Has.Count.EqualTo(2),
                "Pistola e canhão começam prontas e disparam no mesmo quadro.");
        }

        [Test]
        public void Each_weapon_keeps_its_own_reload()
        {
            var arena = new Arena(
                new Player(10f, 100f),
                new[] { new Weapon(Pistol), new Weapon(Cannon) },
                contactRadius: 1.5f, hitRadius: 0.8f);
            arena.Add(new Enemy(new EnemyStats(10000f, 0f, 0f, 1), new Vector2(0f, 5f)));
            var byWeapon = new System.Collections.Generic.Dictionary<Weapon, int>();
            arena.ProjectileFired += (weapon, _) =>
                byWeapon[weapon] = byWeapon.TryGetValue(weapon, out var n) ? n + 1 : 1;

            // Dois segundos: pistola 2/s, canhão 1/s.
            for (var i = 0; i < 120; i++)
                arena.Tick(Vector2.Zero, 0f, 1f / 60f);

            var counts = new System.Collections.Generic.List<int>(byWeapon.Values);
            counts.Sort();
            Assert.That(counts[0], Is.EqualTo(2).Within(1), "canhão: 1 por segundo");
            Assert.That(counts[1], Is.EqualTo(4).Within(1), "pistola: 2 por segundo");
        }

        [Test]
        public void A_plain_shot_only_hurts_what_it_hits()
        {
            var arena = Make(Pistol);
            var target = new Enemy(Frail, new Vector2(0f, 3f));
            var bystander = new Enemy(Standing, new Vector2(1.5f, 3f));
            arena.Add(target);
            arena.Add(bystander);

            // Só até o primeiro tiro acertar. A pistola recarrega em 0,5s (30
            // quadros); passar disso faria ela mirar no espectador depois que o
            // alvo cai — o que é a mira automática certa, não um vazamento de
            // dano em área.
            for (var i = 0; i < 20; i++)
                arena.Tick(Vector2.Zero, 0f, 1f / 60f);

            Assert.That(arena.Kills, Is.EqualTo(1));
            Assert.That(bystander.Health.Current, Is.EqualTo(20f),
                "A pistola tem splashRadius 0: quem está ao lado não deve sentir nada.");
        }

        [Test]
        public void An_explosive_shot_catches_the_crowd_around_the_target()
        {
            var arena = Make(Cannon);
            var target = new Enemy(Standing, new Vector2(0f, 3f));
            var nearby = new Enemy(Standing, new Vector2(2f, 3f));
            var faraway = new Enemy(Standing, new Vector2(9f, 3f));
            arena.Add(target);
            arena.Add(nearby);
            arena.Add(faraway);

            for (var i = 0; i < 60; i++)
                arena.Tick(Vector2.Zero, 0f, 1f / 60f);

            Assert.That(target.Health.Current, Is.LessThan(20f), "o alvo levou o tiro");
            Assert.That(nearby.Health.Current, Is.LessThan(20f),
                "estava a 2 unidades do alvo, dentro do raio 3 — devia ter sido pego");
            Assert.That(faraway.Health.Current, Is.EqualTo(20f),
                "estava a 9 unidades do alvo, fora do raio 3");
        }

        [Test]
        public void One_cannon_shot_can_end_a_whole_group()
        {
            var arena = Make(Cannon);
            for (var i = 0; i < 5; i++)
                arena.Add(new Enemy(Frail, new Vector2(i * 0.5f, 3f)));

            for (var i = 0; i < 60; i++)
                arena.Tick(Vector2.Zero, 0f, 1f / 60f);

            Assert.That(arena.Kills, Is.EqualTo(5),
                "Cinco inimigos frágeis dentro do raio de explosão caem no mesmo tiro.");
        }

        [Test]
        public void Rejects_a_null_enemy()
        {
            Assert.Throws<ArgumentNullException>(() => Peaceful().Add(null));
        }

        // ---------- XP, nível e upgrades ----------

        [Test]
        public void Killing_grants_the_xp_the_catalog_promises()
        {
            var arena = Make(Pistol);
            arena.Add(new Enemy(Frail, new Vector2(0f, 3f)));

            for (var i = 0; i < 20; i++)
                arena.Tick(Vector2.Zero, 0f, 1f / 60f);

            Assert.That(arena.Kills, Is.EqualTo(1));
            Assert.That(arena.Xp.Level + arena.Xp.Current, Is.GreaterThan(1),
                "Frail vale 7 de XP; algo tem de ter entrado.");
        }

        [Test]
        public void The_run_pauses_while_a_card_is_owed()
        {
            var arena = Make(Pistol);
            arena.Add(new Enemy(Frail, new Vector2(0f, 3f)));
            for (var i = 0; i < 20; i++)
                arena.Tick(Vector2.Zero, 0f, 1f / 60f);
            Assume.That(arena.IsAwaitingUpgrade, Is.True);
            var frozen = arena.Player.Position;

            arena.Tick(D, 0f, 1f);

            Assert.That(arena.Player.Position, Is.EqualTo(frozen),
                "Enquanto a carta não é escolhida, o jogo não anda.");
        }

        [Test]
        public void Choosing_a_card_releases_the_run()
        {
            var arena = Make(Pistol);
            arena.Add(new Enemy(Frail, new Vector2(0f, 3f)));
            for (var i = 0; i < 20; i++)
                arena.Tick(Vector2.Zero, 0f, 1f / 60f);

            while (arena.IsAwaitingUpgrade)
                arena.Choose(new Upgrade("dano", StatKind.WeaponDamage, ModifierKind.Percent, 0.3f));

            arena.Tick(D, 0f, 1f);

            Assert.That(arena.Player.Position.X, Is.GreaterThan(0f));
        }

        [Test]
        public void A_chosen_card_reaches_the_weapon()
        {
            var arena = Make(Pistol);
            arena.Add(new Enemy(Frail, new Vector2(0f, 3f)));
            for (var i = 0; i < 20; i++)
                arena.Tick(Vector2.Zero, 0f, 1f / 60f);
            var before = arena.Weapons[0].Stats.Damage;

            arena.Choose(new Upgrade("dano", StatKind.WeaponDamage, ModifierKind.Percent, 1f));

            Assert.That(arena.Weapons[0].Stats.Damage, Is.EqualTo(before * 2f).Within(Tolerance));
            Assert.That(arena.Weapons[0].BaseStats.Damage, Is.EqualTo(10f),
                "A base do catálogo continua intacta.");
        }

        [Test]
        public void A_chosen_card_reaches_the_player()
        {
            var arena = Make(Pistol);
            arena.Add(new Enemy(Frail, new Vector2(0f, 3f)));
            for (var i = 0; i < 20; i++)
                arena.Tick(Vector2.Zero, 0f, 1f / 60f);

            arena.Choose(new Upgrade("botas", StatKind.PlayerSpeed, ModifierKind.Percent, 0.5f));

            Assert.That(arena.Player.Speed, Is.EqualTo(15f).Within(Tolerance));
            Assert.That(arena.Player.BaseSpeed, Is.EqualTo(10f));
        }

        [Test]
        public void More_max_health_also_heals()
        {
            var arena = Make(Pistol, maxHealth: 100f);
            arena.Player.Health.TakeDamage(40f);
            arena.Add(new Enemy(Frail, new Vector2(0f, 3f)));
            for (var i = 0; i < 20; i++)
                arena.Tick(Vector2.Zero, 0f, 1f / 60f);

            arena.Choose(new Upgrade("vida", StatKind.PlayerMaxHealth, ModifierKind.Flat, 25f));

            Assert.That(arena.Player.Health.Max, Is.EqualTo(125f));
            Assert.That(arena.Player.Health.Current, Is.EqualTo(85f),
                "Só aumentar o teto deixaria a barra maior e vazia — pareceria punição.");
        }

        [Test]
        public void A_weapon_equipped_later_also_gets_the_upgrades()
        {
            var arena = Make(Pistol);
            arena.Add(new Enemy(Frail, new Vector2(0f, 3f)));
            for (var i = 0; i < 20; i++)
                arena.Tick(Vector2.Zero, 0f, 1f / 60f);
            arena.Choose(new Upgrade("dano", StatKind.WeaponDamage, ModifierKind.Percent, 1f));

            var cannon = new Weapon(Cannon);
            arena.Equip(cannon);

            Assert.That(cannon.Stats.Damage, Is.EqualTo(20f).Within(Tolerance),
                "Uma arma ganha no meio da run herda o que já foi acumulado.");
        }

        [Test]
        public void Refuses_a_card_that_was_not_earned()
        {
            var arena = Make(Pistol);

            Assert.Throws<System.InvalidOperationException>(
                () => arena.Choose(new Upgrade("dano", StatKind.WeaponDamage, ModifierKind.Percent, 1f)));
        }

        // ---------- nascimento dentro da simulação ----------

        private static EnemySpawn EverySecond(float radius = 25f) =>
            new EnemySpawn(
                new[] { new EnemyStats(20f, 0f, 5f, 1, "slime") },
                new SpawnRing(radius),
                new SpawnTimer(1f),
                new System.Random(1));

        private static Arena WithSpawn(WeaponStats weapon, EnemySpawn spawn) =>
            new Arena(new Player(10f, 100f), new[] { new Weapon(weapon) },
                      contactRadius: 1.5f, hitRadius: 0.8f,
                      experience: null, enemySpawn: spawn);

        [Test]
        public void Enemies_are_born_inside_the_tick()
        {
            var arena = WithSpawn(Unarmed, EverySecond());

            arena.Tick(Vector2.Zero, 0f, 1f);

            Assert.That(arena.Enemies, Has.Count.EqualTo(1));
        }

        [Test]
        public void Every_birth_is_announced()
        {
            var arena = WithSpawn(Unarmed, EverySecond());
            var announced = 0;
            arena.EnemySpawned += _ => announced++;

            arena.Tick(Vector2.Zero, 0f, 3f);

            Assert.That(announced, Is.EqualTo(3),
                "O adaptador só dá corpo ao que este evento anuncia.");
        }

        [Test]
        public void Nobody_is_born_while_a_card_is_owed()
        {
            var arena = WithSpawn(Pistol, EverySecond());
            arena.Add(new Enemy(Frail, new Vector2(0f, 3f)));
            for (var i = 0; i < 20; i++)
                arena.Tick(Vector2.Zero, 0f, 1f / 60f);
            Assume.That(arena.IsAwaitingUpgrade, Is.True);
            var before = arena.Enemies.Count;

            arena.Tick(Vector2.Zero, 0f, 10f);

            Assert.That(arena.Enemies.Count, Is.EqualTo(before),
                "O jogador não pode voltar da tela de cartas para um cerco que " +
                "se formou enquanto ele lia as opções.");
        }

        [Test]
        public void Nobody_is_born_after_the_run_ends()
        {
            var arena = new Arena(new Player(10f, maxHealth: 4f), new[] { new Weapon(Unarmed) },
                                  1.5f, 0.8f, null, EverySecond());
            arena.Add(new Enemy(Standing, Vector2.Zero));
            arena.Tick(Vector2.Zero, 0f, 1f);
            Assume.That(arena.IsOver, Is.True);
            var before = arena.Enemies.Count;

            arena.Tick(Vector2.Zero, 0f, 10f);

            Assert.That(arena.Enemies.Count, Is.EqualTo(before));
        }

        [Test]
        public void The_newborn_carries_the_catalog_identity()
        {
            var arena = WithSpawn(Unarmed, EverySecond());

            arena.Tick(Vector2.Zero, 0f, 1f);

            Assert.That(arena.Enemies[0].Stats.Id, Is.EqualTo("slime"),
                "É por este Id que o adaptador reencontra o prefab do EnemyData.");
        }

        [Test]
        public void Births_happen_around_the_player_not_around_the_origin()
        {
            var arena = WithSpawn(Unarmed, EverySecond(radius: 25f));
            arena.Tick(D, 0f, 1f);

            var born = arena.Enemies[0];
            var distanceToPlayer = (born.Position - arena.Player.Position).Length();

            Assert.That(distanceToPlayer, Is.EqualTo(25f).Within(0.01f));
        }

        // ---------- tempo de run ----------

        [Test]
        public void Counts_the_seconds_survived()
        {
            var arena = Peaceful();

            for (var i = 0; i < 120; i++)
                arena.Tick(Vector2.Zero, 0f, 1f / 60f);

            Assert.That(arena.Elapsed, Is.EqualTo(2f).Within(0.01f));
        }

        [Test]
        public void Reading_the_cards_does_not_count_as_surviving()
        {
            var arena = Make(Pistol);
            arena.Add(new Enemy(Frail, new Vector2(0f, 3f)));
            for (var i = 0; i < 20; i++)
                arena.Tick(Vector2.Zero, 0f, 1f / 60f);
            Assume.That(arena.IsAwaitingUpgrade, Is.True);
            var beforeReading = arena.Elapsed;

            arena.Tick(Vector2.Zero, 0f, 30f);

            Assert.That(arena.Elapsed, Is.EqualTo(beforeReading),
                "Ler as opções com calma não pode inflar o tempo de sobrevivência.");
        }

        [Test]
        public void The_clock_stops_at_death()
        {
            var arena = Peaceful(maxHealth: 4f);
            arena.Add(new Enemy(Standing, Vector2.Zero));
            arena.Tick(Vector2.Zero, 0f, 1f);
            Assume.That(arena.IsOver, Is.True);
            var atDeath = arena.Elapsed;

            arena.Tick(Vector2.Zero, 0f, 60f);

            Assert.That(arena.Elapsed, Is.EqualTo(atDeath));
        }
    }
}
