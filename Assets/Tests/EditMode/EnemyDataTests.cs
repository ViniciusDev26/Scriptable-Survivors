using NUnit.Framework;
using ScriptableSurvivors.Unity;
using UnityEngine;

namespace ScriptableSurvivors.Tests
{
    /// <summary>
    /// Único teste que atravessa a fronteira: verifica que o mapeamento do
    /// asset para o domínio não perde campo pelo caminho.
    ///
    /// Sem isto, adicionar um campo ao EnemyData e esquecer de incluí-lo em
    /// ToDomain() compilaria normalmente, e o valor digitado no Inspector
    /// simplesmente não chegaria ao jogo.
    /// </summary>
    public sealed class EnemyDataTests
    {
        private EnemyData data;

        [SetUp]
        public void SetUp()
        {
            data = ScriptableObject.CreateInstance<EnemyData>();

            // Preenche os campos privados pela mesma serialização que o
            // Inspector usa — é o equivalente a digitar os valores na janela.
            JsonUtility.FromJsonOverwrite(
                "{\"maxHealth\":42.0,\"speed\":3.5,\"damage\":7.0,\"xpReward\":9}",
                data);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(data);
        }

        [Test]
        public void Carries_every_field_across_the_boundary()
        {
            var stats = data.ToDomain();

            Assert.That(stats.MaxHealth, Is.EqualTo(42f), "maxHealth não chegou ao domínio.");
            Assert.That(stats.Speed, Is.EqualTo(3.5f), "speed não chegou ao domínio.");
            Assert.That(stats.Damage, Is.EqualTo(7f), "damage não chegou ao domínio.");
            Assert.That(stats.XpReward, Is.EqualTo(9), "xpReward não chegou ao domínio.");
        }

        [Test]
        public void Hands_out_copies_never_the_asset()
        {
            var first = data.ToDomain();
            var second = data.ToDomain();

            Assert.That(first, Is.Not.SameAs(data));
            Assert.That(first.MaxHealth, Is.EqualTo(second.MaxHealth),
                "Duas leituras do mesmo catálogo devem dar o mesmo número — " +
                "o asset não muda por ser lido.");
        }
    }
}
