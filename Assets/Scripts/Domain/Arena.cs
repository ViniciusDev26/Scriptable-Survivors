using System;
using System.Collections.Generic;
using System.Numerics;

namespace ScriptableSurvivors.Domain
{
    /// <summary>
    /// A simulação inteira de uma run: o jogador, os inimigos vivos, os tiros
    /// em voo, e um único passo de tempo que atualiza tudo em ordem definida.
    ///
    /// A ordem dentro de Tick É a regra. Se os inimigos andassem antes do
    /// jogador, perseguiriam a posição do quadro anterior — um quadro de
    /// atraso, invisível a 60 fps e escancarado quando o jogo engasga.
    /// </summary>
    public sealed class Arena
    {
        private readonly List<Enemy> enemies = new List<Enemy>();
        private readonly List<Projectile> projectiles = new List<Projectile>();
        private readonly List<Weapon> weapons = new List<Weapon>();

        public Player Player { get; }

        /// <summary>A pilha de upgrades desta run. A base do catálogo fica intocada.</summary>
        public StatModifiers Modifiers { get; } = new StatModifiers();

        public Experience Xp { get; }

        private readonly EnemySpawn spawn;

        /// <summary>Todas disparam sozinhas, cada uma com seu próprio recarregamento.</summary>
        public IReadOnlyList<Weapon> Weapons => weapons;

        /// <summary>Distância a partir da qual um inimigo encosta no jogador.</summary>
        public float ContactRadius { get; }

        /// <summary>Distância a partir da qual um tiro acerta um inimigo.</summary>
        public float HitRadius { get; }

        public IReadOnlyList<Enemy> Enemies => enemies;
        public IReadOnlyList<Projectile> Projectiles => projectiles;

        public int Kills { get; private set; }

        public bool IsOver => Player.Health.IsDead;

        /// <summary>
        /// A run está parada esperando o jogador escolher uma carta. Enquanto
        /// for verdade, Tick não faz nada — é a pausa do jogo.
        /// </summary>
        public bool IsAwaitingUpgrade => Xp.PendingLevelUps > 0;

        /// <summary>
        /// Disparado quando a arma atira. É o "evento de domínio" que o
        /// adaptador traduz em visual — ele cria o corpo do projétil ao ouvir.
        /// </summary>
        public event Action<Weapon, Projectile> ProjectileFired;

        /// <summary>Disparado quando um inimigo nasce. O adaptador lhe dá corpo.</summary>
        public event Action<Enemy> EnemySpawned;

        public Arena(
            Player player,
            IEnumerable<Weapon> loadout,
            float contactRadius,
            float hitRadius,
            Experience experience = null,
            EnemySpawn enemySpawn = null)
        {
            if (loadout == null)
                throw new ArgumentNullException(nameof(loadout));
            if (contactRadius <= 0f)
                throw new ArgumentOutOfRangeException(nameof(contactRadius), contactRadius, "O raio de contato deve ser positivo.");
            if (hitRadius <= 0f)
                throw new ArgumentOutOfRangeException(nameof(hitRadius), hitRadius, "O raio de acerto deve ser positivo.");

            Player = player ?? throw new ArgumentNullException(nameof(player));
            Player.BindModifiers(Modifiers);
            Xp = experience ?? new Experience();
            spawn = enemySpawn;
            ContactRadius = contactRadius;
            HitRadius = hitRadius;

            foreach (var weapon in loadout)
                Equip(weapon);

            if (weapons.Count == 0)
                throw new ArgumentException("Uma run precisa de ao menos uma arma.", nameof(loadout));
        }

        /// <summary>
        /// Acrescenta uma arma ao arsenal. No Dia 3 um upgrade pode chamar isto.
        /// </summary>
        public void Equip(Weapon weapon)
        {
            if (weapon == null)
                throw new ArgumentNullException(nameof(weapon));

            weapon.BindModifiers(Modifiers);
            weapons.Add(weapon);
        }

        public void Add(Enemy enemy)
        {
            if (enemy == null)
                throw new ArgumentNullException(nameof(enemy));

            enemies.Add(enemy);
        }

        public void Tick(Vector2 playerInput, float cameraYawDegrees, float deltaTime)
        {
            if (deltaTime < 0f)
                throw new ArgumentOutOfRangeException(nameof(deltaTime), deltaTime, "deltaTime não pode ser negativo.");

            if (IsOver || IsAwaitingUpgrade)
                return;

            Player.Move(playerInput, cameraYawDegrees, deltaTime);

            SpawnEnemies(deltaTime);

            for (var i = 0; i < enemies.Count; i++)
                enemies[i].MoveToward(Player.Position, deltaTime);

            ResolveContacts(deltaTime);
            AdvanceProjectiles(deltaTime);
            FireWeapons(deltaTime);
            BuryTheDead();
        }

        /// <summary>
        /// Nascer faz parte da simulação, então acontece depois da guarda de
        /// pausa: durante a escolha de carta ninguém nasce, e o jogador não volta
        /// para um cerco que se formou enquanto ele lia as opções.
        ///
        /// Vem depois do jogador se mover, pela mesma razão da perseguição: o
        /// círculo de spawn precisa estar em volta de onde ele ESTÁ, não de onde
        /// estava no começo do quadro.
        /// </summary>
        private void SpawnEnemies(float deltaTime)
        {
            if (spawn == null)
                return;

            var due = spawn.Advance(deltaTime);
            for (var i = 0; i < due; i++)
            {
                var enemy = spawn.Create(Player.Position);
                Add(enemy);
                EnemySpawned?.Invoke(enemy);
            }
        }

        /// <summary>
        /// O inimigo vivo mais próximo dentro do alcance, ou null se não há
        /// nenhum. É a mira automática, e é regra — dá para testar sem jogo.
        /// </summary>
        public Enemy NearestEnemyWithin(float range)
        {
            Enemy nearest = null;
            var shortestSquared = range * range;

            for (var i = 0; i < enemies.Count; i++)
            {
                if (enemies[i].Health.IsDead)
                    continue;

                var distanceSquared = (enemies[i].Position - Player.Position).LengthSquared();
                if (distanceSquared > shortestSquared)
                    continue;

                shortestSquared = distanceSquared;
                nearest = enemies[i];
            }

            return nearest;
        }

        /// <summary>
        /// Dano por SEGUNDO em contato, multiplicado pelo deltaTime — e não
        /// por quadro. Se fosse por quadro, jogar a 120 fps seria o dobro de
        /// difícil que a 60.
        /// </summary>
        private void ResolveContacts(float deltaTime)
        {
            var radiusSquared = ContactRadius * ContactRadius;

            for (var i = 0; i < enemies.Count; i++)
            {
                if (enemies[i].Health.IsDead)
                    continue;

                var offset = enemies[i].Position - Player.Position;
                if (offset.LengthSquared() > radiusSquared)
                    continue;

                Player.Health.TakeDamage(enemies[i].Stats.Damage * deltaTime);
                if (IsOver)
                    return;
            }
        }

        private void AdvanceProjectiles(float deltaTime)
        {
            for (var i = projectiles.Count - 1; i >= 0; i--)
            {
                projectiles[i].Advance(deltaTime);
                ResolveHit(projectiles[i]);

                if (projectiles[i].IsSpent)
                    projectiles.RemoveAt(i);
            }
        }

        private void ResolveHit(Projectile projectile)
        {
            if (projectile.IsSpent)
                return;

            var radiusSquared = HitRadius * HitRadius;

            for (var i = 0; i < enemies.Count; i++)
            {
                if (enemies[i].Health.IsDead)
                    continue;

                var offset = enemies[i].Position - projectile.Position;
                if (offset.LengthSquared() > radiusSquared)
                    continue;

                Detonate(projectile, enemies[i]);
                projectile.Consume();
                return;
            }
        }

        /// <summary>
        /// O alvo atingido sempre leva o dano cheio. Se a arma for explosiva,
        /// quem estiver dentro do raio a partir DELE leva também — o centro da
        /// explosão é o corpo acertado, não o ponto exato do projétil.
        /// </summary>
        private void Detonate(Projectile projectile, Enemy struck)
        {
            struck.Health.TakeDamage(projectile.Damage);

            if (projectile.SplashRadius <= 0f)
                return;

            var splashSquared = projectile.SplashRadius * projectile.SplashRadius;

            for (var i = 0; i < enemies.Count; i++)
            {
                if (ReferenceEquals(enemies[i], struck) || enemies[i].Health.IsDead)
                    continue;

                var offset = enemies[i].Position - struck.Position;
                if (offset.LengthSquared() > splashSquared)
                    continue;

                enemies[i].Health.TakeDamage(projectile.Damage);
            }
        }

        private void FireWeapons(float deltaTime)
        {
            for (var i = 0; i < weapons.Count; i++)
            {
                var weapon = weapons[i];
                weapon.Cool(deltaTime);

                if (!weapon.IsReady)
                    continue;

                // Cada arma mira sozinha: o alcance é dela, não da run.
                var target = NearestEnemyWithin(weapon.Stats.Range);
                if (target == null)
                    continue;

                var projectile = weapon.Fire(Player.Position, target.Position);
                projectiles.Add(projectile);
                ProjectileFired?.Invoke(weapon, projectile);
            }
        }

        private void BuryTheDead()
        {
            for (var i = enemies.Count - 1; i >= 0; i--)
            {
                if (!enemies[i].Health.IsDead)
                    continue;

                Xp.Add(enemies[i].Stats.XpReward);
                enemies.RemoveAt(i);
                Kills++;
            }
        }

        /// <summary>
        /// Aplica a carta escolhida e libera a run. Um nível pendente é
        /// consumido por escolha: cinco inimigos mortos num tiro de canhão
        /// podem render dois níveis, e cada um pede sua carta.
        /// </summary>
        public void Choose(Upgrade upgrade)
        {
            if (!Xp.TryConsumeLevelUp())
                throw new InvalidOperationException("Não há nível pendente para gastar.");

            if (upgrade.Stat == StatKind.PlayerMaxHealth)
            {
                // Vida máxima não passa pela pilha: Health é estado vivo, e
                // recalcular o teto a cada leitura brigaria com o dano sofrido.
                Player.Health.RaiseMax(upgrade.Value);
                return;
            }

            Modifiers.Add(upgrade);
        }
    }
}
