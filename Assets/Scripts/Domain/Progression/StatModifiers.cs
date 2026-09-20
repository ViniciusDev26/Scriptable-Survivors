using System.Collections.Generic;

namespace ScriptableSurvivors.Domain.Progression
{
    /// <summary>
    /// A pilha de modificadores de uma run. Os números do catálogo ficam
    /// intactos como base; o que os upgrades somam vive aqui, separado.
    ///
    /// Por isso é sempre possível responder de onde cada ponto de dano veio —
    /// que é a mesma lição do bug do Dia 4, aplicada ao motor do jogo.
    ///
    /// Percentuais somam entre si em vez de compor: três upgrades de +30% dão
    /// +90%, não 2,2x. Previsível o bastante para balancear de cabeça.
    /// </summary>
    public sealed class StatModifiers
    {
        private readonly Dictionary<StatKind, float> flat = new Dictionary<StatKind, float>();
        private readonly Dictionary<StatKind, float> percent = new Dictionary<StatKind, float>();
        private readonly List<StatKind> touched = new List<StatKind>();

        /// <summary>
        /// Os stats que algum upgrade encostou, na ordem em que foram escolhidos.
        /// É o que a HUD lista como bônus ativos.
        /// </summary>
        public IReadOnlyList<StatKind> ActiveStats => touched;

        /// <summary>
        /// Sobe a cada upgrade aplicado. A HUD usa para saber que precisa
        /// remontar o texto, em vez de refazê-lo todo quadro.
        /// </summary>
        public int Version { get; private set; }

        public void Add(Upgrade upgrade)
        {
            var bucket = upgrade.Kind == ModifierKind.Flat ? flat : percent;
            bucket.TryGetValue(upgrade.Stat, out var accumulated);
            bucket[upgrade.Stat] = accumulated + upgrade.Value;

            if (!touched.Contains(upgrade.Stat))
                touched.Add(upgrade.Stat);

            Version++;
        }

        public float FlatOn(StatKind stat) =>
            flat.TryGetValue(stat, out var value) ? value : 0f;

        public float PercentOn(StatKind stat) =>
            percent.TryGetValue(stat, out var value) ? value : 0f;

        public float Apply(StatKind stat, float baseValue) =>
            (baseValue + FlatOn(stat)) * (1f + PercentOn(stat));
    }
}
