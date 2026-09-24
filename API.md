# RaidCli API 4.4.0

<details>
<summary><code>raid import</code></summary>

```text
raid import --puml <diagram.puml> [--out <directory>] [--name <diagram-id>]
```

Reads the PlantUML source through `OsLib.TextFile`, delegates in-memory parsing
to `RaiDiagram.PlantUmlModelImporter`, and writes `<id>.raid` plus `<id>.svg`
through OsLib file abstractions. `--name` overrides the output and manifest id.

</details>

<details>
<summary><code>raid validate</code></summary>

```text
raid validate <diagram.raid|diagram.svg>
```

Validates `.raid` schema/model/canvas invariants or the public `aim-*` SVG
hydration contract. Invalid content returns a non-zero process status.

</details>

<details>
<summary><code>raid --version</code></summary>

Prints the synchronized RAIkeep suite version (`4.4.0`).

</details>

The reusable public import interfaces (`IModelImporter`,
`PlantUmlModelImporter`, `RaidDiagramModel`, and `AimSvg`) live in RaiDiagram
and are documented in [RaiDiagram API.md](https://github.com/Burkhardt/RaiDiagram/blob/main/API.md).
