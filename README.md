# `raid` — RAI Diagram CLI

`raid` is the first-class RAIkeep command for importing external diagram
sources into canonical `.raid` manifests and hydratable `aim-*` SVG.

```bash
dotnet tool install --global RaidCli --version 4.4.0

raid import --puml ChangeRequestWorkflow.puml --out artifacts
raid import --puml model.puml --out artifacts --name DomainModel
raid validate artifacts/DomainModel.raid
raid validate artifacts/DomainModel.svg
raid --version
```

Version 4.4.0 accepts modern PlantUML activity diagrams (`start`, `stop`,
`detach`, activities, conditionals, and forks) and class diagrams (classes,
interfaces, members, generalization, association, composition, and
aggregation). The generated SVG follows the public `@dr2rai/raid-canvas`
hydration contract.

Filesystem access stays on the OsLib boundary. Import parsers receive an
in-memory `TextReader`; future OPM (`.otw`) and OMG UML/XMI importers implement
RaiDiagram's `IModelImporter` without changing the CLI harness.

Foldable command reference: [API.md](https://github.com/Burkhardt/RaidCli/blob/main/API.md).

Foldable API documentation for the underlying model/import library is in
[RaiDiagram API.md](https://github.com/Burkhardt/RaiDiagram/blob/main/API.md).

Release notes: [RaidCli_RELEASE_NOTES_4.4.0.md](https://github.com/Burkhardt/RAIkeep/blob/main/doc/RaidCli_RELEASE_NOTES_4.4.0.md).
