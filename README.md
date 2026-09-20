# Scriptable Survivors

Roguelite de sobrevivência em arena, feito em quatro dias para demonstrar
**ScriptableObjects** na Unity.

**▶ Jogar:** https://viniciusdev26.github.io/Scriptable-Survivors/

Você controla um personagem num plano aberto. Inimigos surgem fora da tela em
ondas e caminham na sua direção. Suas armas disparam sozinhas no alvo mais
próximo. Cada inimigo morto dá XP; ao subir de nível o jogo pausa e oferece
três upgrades sorteados. Morreu, acabou a run.

`WASD` ou arraste o dedo para andar. `1` `2` `3` ou toque para escolher a carta.

---

## A tese

Todo o conteúdo do jogo é **arquivo, não código**. Criar um inimigo novo, uma
arma nova ou um upgrade novo é criar um `.asset` e preencher campos — sem
escrever uma linha, sem recompilar.

Quatro ScriptableObjects sustentam isso:

| Asset | Guarda | O que prova |
|---|---|---|
| `EnemyData` | prefab, vida, velocidade, dano, XP, clipes de animação | catálogo de dados |
| `WeaponData` | dano, cadência, velocidade do projétil, alcance, raio de explosão | parâmetros fora do código |
| `UpgradeData` | nome, descrição, stat, tipo, valor | **conteúdo como asset** |
| `RunConfig` | jogador, arsenal, catálogo, monte de upgrades, curva de ondas | **SO que referencia SOs** |

Trocar o `RunConfig` no `Bootstrap` troca a partida inteira.

Um exemplo que apareceu sozinho: os modelos do Quaternius usam **três
vocabulários de animação diferentes** — `Walk`/`HitRecieve` nos blobs,
`Walk`/`HitReact` nos grandes, `Fast_Flying`/`HitReact` nos voadores. Como o
nome do clipe é campo do `EnemyData`, as três convivem **sem um único `if`** no
código.

## Arquitetura

As regras do jogo são C# puro. A Unity não é importada por elas.

```
Assets/Scripts/
├── Domain/          regras — a Unity é PROIBIDA aqui
│   ├── Arena.cs         a simulação: um Tick, ordem explícita
│   ├── Combat/          Health, Weapon, WeaponStats, Projectile
│   ├── Enemies/         Enemy, EnemyStats, EnemySpawn, WaveSchedule, SpawnRing
│   ├── Players/         Player, PlanarInput
│   └── Progression/     Experience, Upgrade, UpgradePool, StatModifiers
├── Unity/           adaptadores — traduzem evento de domínio em visual
└── Editor/          build WebGL por linha de comando
```

`ScriptableSurvivors.Domain` tem `noEngineReferences: true` no seu assembly
definition. **Um `using UnityEngine` ali é erro de compilação**, não questão de
disciplina — e a DLL compilada não contém a string "UnityEngine" uma única vez.

As dependências entre contextos formam um grafo acíclico, visível nos `using`
do topo de cada arquivo:

```
Progression  →  (nada)
Combat       →  Progression
Enemies      →  Combat
Players      →  Combat, Progression
Arena        →  todos
```

Consequência prática: dos **145 testes**, **143 rodam em ~30 ms fora do
editor** — os outros dois atravessam a fronteira de propósito, para garantir
que nenhum campo se perca no mapeamento de asset para domínio.

```bash
dotnet test    # com os projetos em Domain/ e Tests/
```

Ou, dentro da Unity: `Window → General → Test Runner → Run All`.

## Rodar

Requer Unity **6000.6.2f1** com o módulo WebGL.

```bash
git lfs install && git lfs pull     # os modelos vêm por LFS
```

Abra o projeto, carregue `Assets/Scenes/Game.unity` e dê Play. A cena contém um
único objeto montado à mão — `Bootstrap`. Câmera, chão, jogador, inimigos e
interface nascem por código a partir dele.

### Build

`Game → Build WebGL` no editor, ou:

```bash
Unity -batchmode -nographics -projectPath . -buildTarget WebGL \
      -executeMethod ScriptableSurvivors.Build.WebGLBuild.Build -logFile -
```

Sai em `Build/WebGL`, com Brotli e fallback de descompressão em JavaScript —
o que permite hospedar em qualquer servidor estático, inclusive o GitHub Pages,
que não manda `Content-Encoding: br`.

## Créditos

Modelos: [Quaternius](https://quaternius.com) — *Ultimate Monsters Pack*, CC0 1.0.

## Licença

MIT.
