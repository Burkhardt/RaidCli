using OsLib;

namespace RaidCli.Tests;

public sealed class RaidCliTests : IDisposable
{
	private readonly RaiPath root = Os.TempDir / "RAIkeep" / "raid-cli-tests";

	public RaidCliTests()
	{
		Cleanup();
		root.mkdir();
	}

	public void Dispose() => Cleanup();

	[Fact]
	public void Version_IsSuiteVersion()
	{
		var output = new StringWriter();
		var exitCode = Program.Run(["--version"], output, new StringWriter());
		Assert.Equal(0, exitCode);
		Assert.Equal("4.4.0", output.ToString().Trim());
	}

	[Fact]
	public void Import_Cr036Fixture_WritesCanonicalRaidAndHydratableSvg()
	{
		var fixture = new TextFile(new RaiPath(AppContext.BaseDirectory) / "Fixtures",
			"ChangeRequestWorkflow", "puml");
		var output = new StringWriter();
		var exitCode = Program.Run(
			["import", "--puml", fixture.FullName, "--out", root.FullPath],
			output,
			new StringWriter());

		Assert.Equal(0, exitCode);
		var raid = new RaiDiagram.RaidFile(root, "ChangeRequestWorkflow");
		var svg = new TextFile(root, "ChangeRequestWorkflow", "svg");
		Assert.True(raid.Exists());
		Assert.True(svg.Exists());
		Assert.Equal(RaiDiagram.DiagramKind.Activity, raid.LoadManifest().Diagram.Kind);
		RaiDiagram.AimSvg.Validate(svg.ReadAllText());
		Assert.Contains(raid.FullName, output.ToString());
		Assert.Contains(svg.FullName, output.ToString());
	}

	[Fact]
	public void Import_NameOverrideControlsOutputAndManifestIdentity()
	{
		var fixture = new TextFile(new RaiPath(AppContext.BaseDirectory) / "Fixtures",
			"ChangeRequestWorkflow", "puml");
		var exitCode = Program.Run(
			["import", "--puml", fixture.FullName, "--out", root.FullPath, "--name", "Workflow_AIA"],
			new StringWriter(),
			new StringWriter());

		Assert.Equal(0, exitCode);
		var raid = new RaiDiagram.RaidFile(root, "Workflow_AIA");
		Assert.Equal("Workflow_AIA", raid.LoadManifest().Diagram.Id);
		Assert.True(new TextFile(root, "Workflow_AIA", "svg").Exists());
	}

	[Fact]
	public void Validate_RejectsMalformedAimSvg()
	{
		var svg = new TextFile(root, "broken", "svg")
		{
			Lines = ["<svg xmlns=\"http://www.w3.org/2000/svg\" />"],
			Changed = true
		};
		svg.Save();
		var error = new StringWriter();
		var exitCode = Program.Run(["validate", svg.FullName], new StringWriter(), error);
		Assert.Equal(1, exitCode);
		Assert.Contains("no hydratable aim-node", error.ToString());
	}

	private void Cleanup()
	{
		if (root.Exists())
			new RaiFile(root.Path).rmdir(depth: 8, deleteFiles: true);
	}
}
