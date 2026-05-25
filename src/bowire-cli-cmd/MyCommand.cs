// Bowire CLI command scaffold.
//
// This type contributes a new subcommand to the `bowire` root via the
// IBowireCliCommand contract. BowireCliCommandRegistry picks it up at
// startup as long as this assembly is loaded — for a sibling-plugin
// repo, that means the package landed in ~/.bowire/plugins/ (CLI
// install) or the host project took a PackageReference on it
// (embedded mode).
//
// Wiring:
//   1. Reference Kuestenlogik.Bowire.Cli from your csproj.
//   2. Build + publish your plugin to NuGet (the Bowire CLI ships
//      `bowire plugin install <id>` for users).
//   3. After install, `bowire MY_VERB --help` shows your command.
//
// See https://github.com/Kuestenlogik/Bowire/blob/main/docs/architecture/plugin-architecture.md#ibowireclicommand
// for the contract details.

using System.CommandLine;
using Kuestenlogik.Bowire.Cli;

namespace Bowire.Plugin1;

/// <summary>
/// Adds the <c>bowire MY_VERB</c> subcommand. Auto-discovered by
/// <see cref="BowireCliCommandRegistry"/> at startup — no manual
/// registration in the host needed.
/// </summary>
public sealed class MyCommand : IBowireCliCommand
{
    /// <summary>
    /// Stable identifier used by <c>--disable-cli-command MY_VERB</c>
    /// to skip this command without rebuilding. Should match the verb
    /// you return from <see cref="Build"/>.
    /// </summary>
    public string Id => "MY_VERB";

    /// <summary>
    /// Build the System.CommandLine surface — options, arguments, and
    /// the handler that runs when the user types <c>bowire MY_VERB</c>.
    /// Called once at startup; the returned <see cref="Command"/> is
    /// attached to the Bowire root and reused for every invocation.
    /// </summary>
    public Command Build()
    {
        var cmd = new Command("MY_VERB", "One-line help text shown in `bowire --help`.");

        var inputOpt = new Option<string>("--input")
        {
            Description = "Example option. Pull this in via pr.GetValue(inputOpt) inside SetAction.",
            Required = false,
        };
        cmd.Add(inputOpt);

        cmd.SetAction(async (pr, ct) =>
        {
            var input = pr.GetValue(inputOpt) ?? "(none)";
            Console.WriteLine($"MY_VERB ran with --input={input}");
            // TODO: do the work. Return non-zero on failure so the CLI
            // exit code surfaces it to the caller's shell / CI.
            await Task.CompletedTask;
            return 0;
        });

        return cmd;
    }
}
