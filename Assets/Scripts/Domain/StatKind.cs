namespace ScriptableSurvivors.Domain
{
    /// <summary>Qual número um upgrade altera.</summary>
    public enum StatKind
    {
        WeaponDamage,
        WeaponFireRate,
        WeaponRange,
        WeaponProjectileSpeed,
        WeaponSplashRadius,
        PlayerSpeed,
        PlayerMaxHealth,
    }

    /// <summary>Como ele altera.</summary>
    public enum ModifierKind
    {
        /// <summary>Soma absoluta: +5 de dano.</summary>
        Flat,

        /// <summary>Soma percentual sobre a base: +30%.</summary>
        Percent,
    }
}
