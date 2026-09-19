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

        public Player Player { get; }
        public Weapon Weapon { get; }

        /// <summary>Distância a partir da qual um inimigo encosta no jogador.</summary>
        public float ContactRadius { get; }

        /// <summary>Distância a partir da qual um tiro acerta um inimigo.</summary>
        public float HitRadius { get; }

        public IReadOnlyList<Enemy> Enemies => enemies;
        public IReadOnlyList<Projectile> Projectiles => projectiles;

        public int Kills { get; private set; }

        public bool IsOver => Player.Health.IsDead;

        /// <summary>
        /// Disparado quando a arma atira. É o "evento de domínio" que o
        /// adaptador traduz em visual — ele cria o corpo do projétil ao ouvir.
        /// </summary>
        public event Action<Projectile> ProjectileFired;

        public Arena(Player player, Weapon weapon, float contactRadius, float hitRadius)
        {
            if (contactRadius <= 0f)
                throw new ArgumentOutOfRangeException(nameof(contactRadius), contactRadius, "O raio de contato deve ser positivo.");
            if (hitRadius <= 0f)
                throw new ArgumentOutOfRangeException(nameof(hitRadius), hitRadius, "O raio de acerto deve ser positivo.");

            Player = player ?? throw new ArgumentNullException(nameof(player));
            Weapon = weapon ?? throw new ArgumentNullException(nameof(weapon));
            ContactRadius = contactRadius;
            HitRadius = hitRadius;
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

            if (IsOver)
                return;

            Player.Move(playerInput, cameraYawDegrees, deltaTime);

            for (var i = 0; i < enemies.Count; i++)
                enemies[i].MoveToward(Player.Position, deltaTime);

            ResolveContacts(deltaTime);
            AdvanceProjectiles(deltaTime);
            FireWeapon(deltaTime);
            BuryTheDead();
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

                enemies[i].Health.TakeDamage(projectile.Damage);
                projectile.Consume();
                return;
            }
        }

        private void FireWeapon(float deltaTime)
        {
            Weapon.Cool(deltaTime);
            if (!Weapon.IsReady)
                return;

            var target = NearestEnemyWithin(Weapon.Stats.Range);
            if (target == null)
                return;

            var projectile = Weapon.Fire(Player.Position, target.Position);
            projectiles.Add(projectile);
            ProjectileFired?.Invoke(projectile);
        }

        private void BuryTheDead()
        {
            for (var i = enemies.Count - 1; i >= 0; i--)
            {
                if (!enemies[i].Health.IsDead)
                    continue;

                enemies.RemoveAt(i);
                Kills++;
            }
        }
    }
}
