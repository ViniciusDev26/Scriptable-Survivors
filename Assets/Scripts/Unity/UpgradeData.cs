using ScriptableSurvivors.Domain;
using UnityEngine;

namespace ScriptableSurvivors.Unity
{
    /// <summary>
    /// Uma carta de upgrade. Existe como arquivo .asset, criado por
    /// Assets → Create → Game → Upgrade — sem escrever código, sem recompilar.
    ///
    /// É o destaque da apresentação: acrescentar conteúdo ao jogo é criar um
    /// arquivo e preencher quatro campos.
    /// </summary>
    [CreateAssetMenu(menuName = "Game/Upgrade", fileName = "NewUpgrade")]
    public sealed class UpgradeData : ScriptableObject
    {
        [Header("Carta")]
        [Tooltip("Vazio usa o nome do arquivo.")]
        [SerializeField] private string title;

        [Tooltip("Vazio gera o texto a partir do efeito, por exemplo \"+30% de dano\".")]
        [SerializeField, TextArea(2, 4)] private string description;

        [SerializeField] private Sprite icon;

        [Header("Efeito")]
        [SerializeField] private StatKind stat = StatKind.WeaponDamage;
        [SerializeField] private ModifierKind kind = ModifierKind.Percent;

        [Tooltip("Em Percent, 0,3 significa +30%. Em Flat, é o valor absoluto.")]
        [SerializeField] private float value = 0.3f;

        public string Title => string.IsNullOrWhiteSpace(title) ? name : title;

        public string Description =>
            string.IsNullOrWhiteSpace(description) ? Describe() : description;

        public Sprite Icon => icon;

        public Upgrade ToDomain() => new Upgrade(name, stat, kind, value);

        /// <summary>
        /// Texto gerado a partir do efeito, para uma carta nova já nascer
        /// legível. Na demonstração ao vivo isso significa preencher um campo
        /// em vez de três — menos digitação na frente da plateia é menos risco.
        /// </summary>
        private string Describe() =>
            $"{StatLabels.Amount(kind, value)} de {StatLabels.For(stat)}";
    }
}
