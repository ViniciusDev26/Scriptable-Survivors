using System;
using System.Collections.Generic;

namespace ScriptableSurvivors.Domain
{
    /// <summary>
    /// O monte de upgrades da run. Sortear as cartas é regra de jogo, não
    /// desenho: dá para provar que as três oferecidas nunca se repetem sem
    /// precisar subir de nível cinquenta vezes no editor.
    /// </summary>
    public sealed class UpgradePool
    {
        private readonly List<Upgrade> available;
        private readonly List<Upgrade> scratch = new List<Upgrade>();

        public int Count => available.Count;

        public UpgradePool(IEnumerable<Upgrade> upgrades)
        {
            if (upgrades == null)
                throw new ArgumentNullException(nameof(upgrades));

            available = new List<Upgrade>(upgrades);

            if (available.Count == 0)
                throw new ArgumentException("O monte não pode estar vazio.", nameof(upgrades));
        }

        /// <summary>
        /// Tira até <paramref name="count"/> cartas distintas. Se o monte for
        /// menor que isso, devolve o que tem — oferecer a mesma carta duas vezes
        /// na mesma tela faria o jogador achar que é bug.
        /// </summary>
        public IReadOnlyList<Upgrade> Draw(int count, Random random)
        {
            if (count <= 0)
                throw new ArgumentOutOfRangeException(nameof(count), count, "É preciso oferecer ao menos uma carta.");
            if (random == null)
                throw new ArgumentNullException(nameof(random));

            scratch.Clear();
            scratch.AddRange(available);

            var drawn = new List<Upgrade>(Math.Min(count, scratch.Count));

            while (drawn.Count < count && scratch.Count > 0)
            {
                var index = random.Next(scratch.Count);
                drawn.Add(scratch[index]);
                scratch.RemoveAt(index);
            }

            return drawn;
        }
    }
}
