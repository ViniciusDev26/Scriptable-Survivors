using System;
using System.Collections.Generic;
using System.Numerics;

namespace ScriptableSurvivors.Domain
{
    /// <summary>
    /// A simulação inteira de uma run: o jogador, os inimigos vivos, e um
    /// único passo de tempo que os atualiza em ordem definida.
    ///
    /// A ordem dentro de Tick É a regra. Se os inimigos andassem antes do
    /// jogador, perseguiriam a posição do quadro anterior — um quadro de
    /// atraso, invisível a 60 fps e escancarado quando o jogo engasga.
    /// </summary>
    public sealed class Arena
    {
        private readonly List<Enemy> enemies = new List<Enemy>();

        public Player Player { get; }

        /// <summary>Distância a partir da qual um inimigo encosta no jogador.</summary>
        public float ContactRadius { get; }

        public IReadOnlyList<Enemy> Enemies => enemies;

        public bool IsOver => Player.Health.IsDead;

        public Arena(Player player, float contactRadius)
        {
            if (contactRadius <= 0f)
                throw new ArgumentOutOfRangeException(nameof(contactRadius), contactRadius, "O raio de contato deve ser positivo.");

            Player = player ?? throw new ArgumentNullException(nameof(player));
            ContactRadius = contactRadius;
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
        }

        /// <summary>
        /// Dano é por SEGUNDO em contato, não por toque. Multiplicar pelo
        /// deltaTime faz o estrago depender do tempo encostado, e não da taxa
        /// de quadros — a 120 fps o jogo seria o dobro de difícil se o dano
        /// fosse aplicado por quadro.
        /// </summary>
        private void ResolveContacts(float deltaTime)
        {
            var radiusSquared = ContactRadius * ContactRadius;

            for (var i = 0; i < enemies.Count; i++)
            {
                var offset = enemies[i].Position - Player.Position;
                if (offset.LengthSquared() > radiusSquared)
                    continue;

                Player.Health.TakeDamage(enemies[i].Stats.Damage * deltaTime);
                if (IsOver)
                    return;
            }
        }
    }
}
