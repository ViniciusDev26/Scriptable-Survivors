# Scriptable Survivors

Roguelite de arena em Unity 6000.6.2f1 (URP). Demo de ScriptableObjects em
4 dias, entrega WebGL no itch.io. Escopo e cronograma em `resume.txt`.

O objetivo real é uma **apresentação ao vivo**. Decisões que enfraquecem a
demonstração perdem para decisões que a fortalecem, mesmo quando são
tecnicamente superiores.

## Arquitetura

Três assemblies, uma seta:

    ScriptableSurvivors.Unity  ──┐
                                 ├──▶ ScriptableSurvivors.Domain
    ScriptableSurvivors.Tests ───┘

### `Assets/Scripts/Domain` — as regras

- `noEngineReferences: true`. **`using UnityEngine` não compila aqui.**
- C# puro: dano, XP, nível, sorteio de upgrades, curva de spawn.
- Proibido: `MonoBehaviour`, `ScriptableObject`, `Vector3`, `Time.deltaTime`,
  `Random` da Unity, corrotinas.
- Precisa de um tipo da Unity? Defina o equivalente puro aqui e converta na
  fronteira. Tempo entra como `float deltaTime` por parâmetro, não como
  consulta a um relógio global.
- Toda regra nova nasce com teste.

### `Assets/Scripts/Unity` — os adaptadores

- `MonoBehaviour`s, `ScriptableObject`s, `Bootstrap`.
- Traduz evento de domínio em visual, e input em chamada de domínio.
  **Não decide regra.**
- Heurística: se um `MonoBehaviour` tem um `if` sobre regra de jogo, a regra
  está no arquivo errado.

### `Assets/Tests/EditMode` — os testes

- Referencia apenas `Domain` + NUnit. Editor-only, fora da build WebGL.
- Rodam sem abrir o jogo: `Window → General → Test Runner → Run All`.
  `EditMode` já é o modo padrão da janela.

## Composição

Um único objeto `Bootstrap` na cena `Assets/Scenes/Game.unity`, com o
`RunConfig` em `[SerializeField]`. Todo o resto nasce por código.

**Nada de `Resources.Load` por string.** A dependência é declarada e tipada,
não procurada — e arrastar o asset no Inspector é a tese da apresentação.

## ScriptableObjects

- São **catálogo de dados**, nunca estado de runtime.
- Estado mutável dentro de um SO é o mesmo erro que estado de request num
  serviço singleton do DI: vaza entre instâncias e persiste depois do Play.
- `EnemyData` diz quanta vida o tipo **tem**; cada inimigo em cena carrega a
  sua própria `Health`.
- O Dia 4 quebra essa regra **de propósito**, como demonstração. Só ali, e o
  teste `HealthTests.Two_instances_do_not_share_state` é quem denuncia.

## Git

- O `.meta` é commitado sempre junto com o arquivo: o GUID mora nele e é a
  identidade do asset. Caminho é só rótulo.
- Renomear asset = mover o arquivo **e** o `.meta` juntos, ou o GUID se perde
  e as referências quebram.
- **Feche o editor antes de renomear ou remover arquivos.** A Unity reescreve
  `ProjectSettings/` ao fechar e sobrescreve edições feitas em disco.
- Commit ao fim de cada dia, direto na `main`. Projeto solo, histórico linear.
- `Library/` é cache derivado, ignorado. Se algo parecer corrompido, apagá-la
  é seguro — a Unity reconstrói.

## Plataforma

WebGL. Recursos sem suporte devem ficar desligados:

- GPU Resident Drawer — já desligado em `Assets/Settings/PC_RPAsset.asset`
- Compute shaders, multithreading, `System.IO` em runtime

## Assets binários (Git LFS)

`.fbx`, `.png`, `.wav` e `.psd` passam pelo Git LFS: o repositório guarda um
ponteiro de ~132 bytes e o binário vive em armazenamento separado. Sem isso,
cada alteração num FBX de 3 MB gravaria 3 MB novos no histórico para sempre.

**Um clone feito sem `git lfs` instalado recebe arquivos de ponteiro em vez
dos modelos**, e a Unity falha ao importar. Em máquina nova:

```bash
git lfs install   # uma vez por máquina
git lfs pull      # traz os binários deste repo
```

`Assets/Art/Quaternius_UltimateMonsters/` — CC0 1.0, 50 modelos com 9
animações cada, todos compartilhando um único `Atlas_Monsters.png`. As
subpastas `Big/`, `Blob/` e `Flying/` são obrigatórias: dez nomes de arquivo
colidem entre categorias e são modelos diferentes.

Assets não referenciados por uma cena incluída **não entram na build**. O peso
é de repositório, não de WebGL.

## Setup local (não versionável)

O `.gitattributes` declara `merge=unityyamlmerge` para `.unity`, `.prefab` e
`.asset`, mas o Git não aceita o driver vindo do repositório — seria execução
remota de código. Refazer em cada clone:

```bash
TOOL="$HOME/Unity/Hub/Editor/6000.6.2f1/Editor/Data/Tools/UnityYAMLMerge"
git config merge.unityyamlmerge.name "Unity SmartMerge"
git config merge.unityyamlmerge.driver "'$TOOL' merge -p --force --fallback none %O %B %A %A"
git config merge.unityyamlmerge.recursive binary
```

Sem ele, um conflito em cena é resolvido linha a linha e corrompe o YAML
em silêncio.

## Contexto de quem conduz

O dono do projeto vem de sistemas web e domina arquitetura em camadas, mas
é novo no editor da Unity. Explique conceitos do editor por analogia com o
stack web, e traga decisões como trade-offs (custo, risco, impacto), não
como jargão de gamedev.
