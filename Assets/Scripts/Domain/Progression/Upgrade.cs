using System;

namespace ScriptableSurvivors.Domain.Progression
{
    /// <summary>
    /// Um upgrade, já fora da Unity. Nome, descrição e ícone ficam no asset —
    /// são apresentação. Aqui mora só o efeito.
    ///
    /// O Id vem do nome do arquivo e serve para o adaptador reencontrar o asset
    /// na hora de desenhar a carta.
    /// </summary>
    public readonly struct Upgrade
    {
        public string Id { get; }
        public StatKind Stat { get; }
        public ModifierKind Kind { get; }
        public float Value { get; }

        public Upgrade(string id, StatKind stat, ModifierKind kind, float value)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("Um upgrade precisa de identificador.", nameof(id));

            Id = id;
            Stat = stat;
            Kind = kind;
            Value = value;
        }
    }
}
