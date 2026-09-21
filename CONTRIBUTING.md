# Como contribuir

Contribuições são bem-vindas. O fluxo é o mesmo de projetos como o Node.js:
**você trabalha no seu fork e propõe a mudança por Pull Request.** Ninguém
empurra direto para a `main` deste repositório.

## O fluxo, em resumo

1. Faça um **fork** do projeto
2. Crie um **branch** para a sua mudança
3. Faça as alterações e **garanta que os testes passam**
4. Abra uma **Pull Request** explicando **o que mudou** e **por quê**

## 1 · Fork e clone

Use o botão **Fork** no GitHub e depois:

```bash
git clone https://github.com/SEU-USUARIO/Scriptable-Survivors.git
cd Scriptable-Survivors

git lfs install
git lfs pull
```

**O `git lfs pull` não é opcional.** Os 50 modelos do Quaternius são
versionados por Git LFS; sem ele você recebe arquivos de ponteiro de 132 bytes
no lugar dos modelos, e a Unity falha ao importar.

Aponte o repositório original como `upstream`, para conseguir se atualizar:

```bash
git remote add upstream https://github.com/ViniciusDev26/Scriptable-Survivors.git
```

### Ferramentas

- **Unity 6000.6.2f1** com o módulo WebGL
- **Git LFS**
- Opcionalmente, o **.NET SDK** — permite rodar os testes de domínio sem abrir
  o editor

O `.gitattributes` declara um driver de merge para cenas e prefabs, mas o Git
não aceita drivers vindos do repositório (seria execução remota de código).
Configure uma vez por máquina — o comando está no `CLAUDE.md`, na seção
**Setup local**.

## 2 · Branch

```bash
git switch -c fix/nome-curto-do-problema
```

Um branch por assunto. Mudanças sem relação entre si viram Pull Requests
separadas — é mais fácil revisar, e uma não trava a outra.

## 3 · Faça a mudança

Leia o **[`CLAUDE.md`](CLAUDE.md)** antes. Ele documenta as convenções, e as
principais são:

**A camada de domínio não conhece a Unity.** `Assets/Scripts/Domain` tem
`noEngineReferences: true` no seu assembly definition — um `using UnityEngine`
ali **não compila**. Regra de jogo vai para lá; desenho e input ficam em
`Assets/Scripts/Unity`.

**Dados vão para ScriptableObject, não para código.** Um inimigo novo, uma arma
nova ou um upgrade novo é um `.asset`, não uma classe.

**Código em inglês, comentários e mensagens em PT-BR.** Nomes de tipo, método,
campo, arquivo e asset em inglês. Comentários, `<summary>`, mensagens de exceção
e de asserção em português — elas aparecem no Test Runner.

**Arquivos `.meta` são versionados.** Eles carregam o GUID de cada asset, que é
a identidade que a Unity usa. Commitar um asset sem o `.meta` quebra todas as
referências a ele.

**Feche o editor antes de renomear ou remover assets.** A Unity reescreve
`ProjectSettings/` ao fechar e sobrescreve edições feitas em disco.

### Testes

Toda regra nova no domínio nasce com teste. Na Unity:

```
Window → General → Test Runner → EditMode → Run All
```

Os 145 testes precisam ficar verdes. Se você tiver o .NET SDK, 143 deles rodam
em ~30 ms sem abrir o editor — a exceção são os dois que atravessam a fronteira
de propósito e dependem de `UnityEngine`.

## 4 · Commits

O histórico usa prefixo convencional em inglês e corpo em português:

```
feat: dano em área e arsenal com várias armas

WeaponStats ganha splashRadius. Zero mantém o comportamento anterior de
alvo único, então a Pistol não muda em nada.

Arena.Detonate aplica o dano cheio em quem foi atingido e, se a arma for
explosiva, em todos dentro do raio a partir DELE.
```

Prefixos em uso: `feat`, `fix`, `perf`, `refactor`, `docs`, `chore`, `build`,
`balance`.

**O corpo explica o porquê, não o quê.** O diff já mostra o que mudou. O que se
perde com o tempo é a razão — qual alternativa foi descartada, qual armadilha
motivou a decisão, o que quebraria se alguém "simplificasse" aquilo depois.

## 5 · Pull Request

```bash
git push origin fix/nome-curto-do-problema
```

Abra a PR contra a `main` deste repositório. A descrição precisa responder
duas perguntas:

**O que mudou.** Em uma ou duas frases, o que o revisor vai encontrar no diff.

**Por que mudou.** O problema que existia antes, ou o que a mudança
possibilita. Se corrige um bug, descreva como reproduzi-lo.

Ajuda muito incluir:

- **Como você testou** — testes novos, o que você jogou para verificar
- **Captura de tela ou vídeo**, se a mudança é visual
- **O que você considerou e descartou**, se houver mais de um caminho razoável

PRs pequenas e focadas são revisadas muito mais rápido que grandes. Se a sua
mudança é grande, vale abrir uma issue antes para combinar a direção.

### O que esperar

A revisão pode pedir ajustes. Comentários são sobre o código, nunca sobre quem
escreveu. Se você discordar de um pedido, diga o porquê — a decisão pode mudar.

## Reportando bugs

Abra uma issue com:

- O que você esperava e o que aconteceu
- Como reproduzir
- Versão da Unity e sistema operacional
- Se der, a **semente da run** — ela aparece no Console como
  `Semente desta run: 123456789`, e colocá-la no campo `Random Seed` do
  `RunConfig` **repete a partida exatamente**

## Conteúdo, não código

Nem toda contribuição precisa de programação. Um inimigo novo, uma arma nova ou
um upgrade novo é um arquivo criado pelo menu `Assets → Create → Game`, com
campos preenchidos no Inspector — e entra na PR como qualquer outra mudança.

É, aliás, a tese do projeto.
