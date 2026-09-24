using System.Reflection;
using OsLib;
using RaiDiagram;

namespace RaidCli;

public static class Program
{
	public static int Main(string[] args) => Run(args, Console.Out, Console.Error);

	public static int Run(string[] args, TextWriter output, TextWriter error)
	{
		ArgumentNullException.ThrowIfNull(args);
		ArgumentNullException.ThrowIfNull(output);
		ArgumentNullException.ThrowIfNull(error);
		try
		{
			if (args.Length == 0 || Has(args, "-h", "--help"))
			{
				WriteHelp(output);
				return 0;
			}
			if (Has(args, "-v", "--version"))
			{
				output.WriteLine(Version());
				return 0;
			}
			return args[0] switch
			{
				"import" => Import(args[1..], output),
				"validate" => Validate(args[1..], output),
				_ => Fail(error, $"Unknown command '{args[0]}'. Run raid --help.")
			};
		}
		catch (Exception exception)
		{
			error.WriteLine($"raid: {exception.Message}");
			return 1;
		}
	}

	private static int Import(string[] args, TextWriter output)
	{
		var sourceName = Value(args, "--puml")
			?? throw new ArgumentException("import requires --puml <diagram.puml>.");
		var source = new TextFile(sourceName);
		if (!source.Exists())
			throw new RaiPathNotFoundException($"The PlantUML source does not exist: {source.FullName}", source.FullName);

		var importer = new PlantUmlModelImporter();
		if (!importer.CanImport(source.FullName))
			throw new ArgumentException($"Unsupported import source '{source.FullName}'. Expected a .puml file.");
		var manifest = importer.Import(new StringReader(source.ReadAllText())).Manifest;
		var requestedName = Value(args, "--name");
		if (!string.IsNullOrWhiteSpace(requestedName))
		{
			ValidateName(requestedName);
			manifest.Diagram.Id = requestedName;
			manifest.Model.ModelId = requestedName;
		}

		var destination = Value(args, "--out") is { Length: > 0 } outName
			? new RaiPath(outName)
			: source.Path;
		var raidFile = new RaidFile(destination, manifest.Diagram.Id);
		raidFile.SaveManifest(manifest);
		var svgFile = new TextFile(destination, manifest.Diagram.Id, "svg");
		svgFile.DeleteAll().Append(AimSvg.Emit(manifest)).Save();

		output.WriteLine(raidFile.FullName);
		output.WriteLine(svgFile.FullName);
		return 0;
	}

	private static int Validate(string[] args, TextWriter output)
	{
		if (args.Length != 1 || args[0].StartsWith('-'))
			throw new ArgumentException("validate requires exactly one .raid or .svg path.");
		var file = new TextFile(args[0]);
		if (!file.Exists())
			throw new RaiPathNotFoundException($"The validation target does not exist: {file.FullName}", file.FullName);
		if (file.Ext.Equals("raid", StringComparison.OrdinalIgnoreCase))
			RaidJson5.Parse(file.ReadAllText());
		else if (file.Ext.Equals("svg", StringComparison.OrdinalIgnoreCase))
			AimSvg.Validate(file.ReadAllText());
		else
			throw new ArgumentException("validate supports .raid and .svg files.");
		output.WriteLine($"valid: {file.FullName}");
		return 0;
	}

	private static string? Value(string[] args, string option)
	{
		var found = Array.FindIndex(args, item => item.Equals(option, StringComparison.Ordinal));
		if (found < 0)
			return null;
		if (found + 1 >= args.Length || args[found + 1].StartsWith('-'))
			throw new ArgumentException($"{option} requires a value.");
		return args[found + 1];
	}

	private static bool Has(string[] args, params string[] options)
		=> args.Any(argument => options.Contains(argument, StringComparer.Ordinal));

	private static string Version()
	{
		var informational = Assembly.GetExecutingAssembly()
			.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
		return (informational ?? "0.0.0").Split('+')[0];
	}

	private static void ValidateName(string value)
	{
		if (value.Length == 0 || value.Any(character =>
			!(char.IsLetterOrDigit(character) || character is '-' or '_')))
			throw new ArgumentException("--name accepts only letters, numbers, '-' and '_'.");
	}

	private static int Fail(TextWriter error, string message)
	{
		error.WriteLine($"raid: {message}");
		return 2;
	}

	private static void WriteHelp(TextWriter output)
	{
		output.WriteLine("RAI Diagram CLI");
		output.WriteLine("raid --version");
		output.WriteLine("raid import --puml <diagram.puml> [--out <directory>] [--name <diagram-id>]");
		output.WriteLine("raid validate <diagram.raid|diagram.svg>");
	}
}
