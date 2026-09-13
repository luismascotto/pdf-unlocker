using System.ComponentModel;
using Spectre.Console;
using Spectre.Console.Cli;

namespace PdfUnlocker;

internal sealed class UnlockSettings : CommandSettings
{
    [CommandArgument(0, "[file]")]
    [Description("Path to the password-protected PDF (also works via Windows Send To).")]
    public string? FileArgument { get; init; }

    [CommandOption("-f|--file <PATH>")]
    [Description("Path to the password-protected PDF.")]
    public string? FileOption { get; init; }

    [CommandOption("-p|--password <PASSWORD>")]
    [Description("Try only this password instead of passwords.txt.")]
    public string? Password { get; init; }

    [CommandOption("-o|--output <PATH>")]
    [Description("Output path for the unlocked PDF. Default: <name>_unlocked.pdf next to the input.")]
    public string? Output { get; init; }

    [CommandOption("--passwords-file <PATH>")]
    [Description("Password list file (one per line). Default: passwords.txt beside the executable.")]
    public string? PasswordsFile { get; init; }

    [CommandOption("-n|--pages <N>")]
    [Description("Keep only the first N pages (e.g. 1 for boleto/barcode). Default: all pages.")]
    public int? Pages { get; init; }

    [CommandOption("--open|--no-open")]
    [Description("Open the unlocked PDF in the default viewer on success (default: on).")]
    [DefaultValue(true)]
    public bool Open { get; init; } = true;

    public string? ResolveInputPath() =>
        !string.IsNullOrWhiteSpace(FileOption) ? FileOption :
        !string.IsNullOrWhiteSpace(FileArgument) ? FileArgument :
        null;

    public override ValidationResult Validate()
    {
        var input = ResolveInputPath();
        if (string.IsNullOrWhiteSpace(input))
        {
            return ValidationResult.Error("Provide a PDF path as the first argument or with --file.");
        }

        if (!System.IO.File.Exists(input))
        {
            return ValidationResult.Error($"File not found: {input}");
        }

        if (Pages is <= 0)
        {
            return ValidationResult.Error("--pages must be a positive number (e.g. --pages 1).");
        }

        return ValidationResult.Success();
    }
}

internal sealed class UnlockCommand : Command<UnlockSettings>
{
    protected override int Execute(CommandContext context, UnlockSettings settings, CancellationToken cancellationToken)
    {
        var inputPath = Path.GetFullPath(settings.ResolveInputPath()!);
        var outputPath = string.IsNullOrWhiteSpace(settings.Output)
            ? BuildDefaultOutputPath(inputPath)
            : Path.GetFullPath(settings.Output);

        var passwords = ResolvePasswords(settings);
        if (passwords.Count == 0)
        {
            AnsiConsole.MarkupLine("[red]No passwords to try.[/]");
            AnsiConsole.MarkupLine(
                "Pass [cyan]-p|--password[/], or create [cyan]passwords.txt[/] beside the executable " +
                "(see [cyan]passwords.example.txt[/]).");
            return 1;
        }

        AnsiConsole.MarkupLine($"Unlocking [yellow]{Markup.Escape(inputPath)}[/]");
        if (settings.Pages is int pageLimit)
        {
            AnsiConsole.MarkupLine($"Keeping first [cyan]{pageLimit}[/] page(s) only.");
        }

        AnsiConsole.MarkupLine($"Trying [cyan]{passwords.Count}[/] password(s)...");

        foreach (var password in passwords)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var result = PdfUnlockService.TryUnlock(inputPath, password, outputPath, settings.Pages);
            switch (result.Status)
            {
                case PdfUnlockService.UnlockStatus.Success:
                    AnsiConsole.MarkupLine($"[green]Unlocked[/] with a matching password.");
                    AnsiConsole.MarkupLine(
                        $"Saved ([cyan]{result.PagesWritten}[/] page(s)): [green]{Markup.Escape(result.OutputPath!)}[/]");
                    if (settings.Open)
                    {
                        PdfUnlockService.OpenInDefaultViewer(result.OutputPath!);
                    }

                    return 0;

                case PdfUnlockService.UnlockStatus.WrongPassword:
                    AnsiConsole.MarkupLine($"[grey]… no match[/]");
                    break;

                case PdfUnlockService.UnlockStatus.Failed:
                    AnsiConsole.MarkupLine($"[red]Error:[/] {Markup.Escape(result.Message ?? "unknown error")}");
                    return 2;
            }
        }

        AnsiConsole.MarkupLine("[red]None of the passwords opened the PDF.[/]");
        return 1;
    }

    private static IReadOnlyList<string> ResolvePasswords(UnlockSettings settings)
    {
        if (!string.IsNullOrWhiteSpace(settings.Password))
        {
            return [settings.Password];
        }

        var path = string.IsNullOrWhiteSpace(settings.PasswordsFile)
            ? PasswordList.DefaultPath
            : Path.GetFullPath(settings.PasswordsFile);

        var passwords = PasswordList.Load(path);
        if (passwords.Count == 0)
        {
            AnsiConsole.MarkupLine($"[grey]Password list not found or empty:[/] {Markup.Escape(path)}");
        }

        return passwords;
    }

    private static string BuildDefaultOutputPath(string inputPath)
    {
        var directory = Path.GetDirectoryName(inputPath) ?? ".";
        var name = Path.GetFileNameWithoutExtension(inputPath);
        return Path.Combine(directory, $"{name}_unlocked.pdf");
    }
}
