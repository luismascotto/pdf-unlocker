# Contributing

Thanks for helping improve PdfUnlocker.

## Guidelines

- Keep the first release focused on a solid Windows CLI experience.
- Never commit real CPF/CNPJ/CEP digits or a personal `passwords.txt`.
- Prefer small, focused pull requests with a short “why” in the description.
- Match existing C# style: nullable enabled, clear error messages, no parallel password races.

## Development

```bat
dotnet build PdfUnlocker.csproj
dotnet run --project PdfUnlocker.csproj -- --help
```

Use a disposable sample PDF you own when testing unlock flows.

## Ideas welcome

- Extra password-file locations (user profile)
- Batch unlock of a folder
- Non-Windows publish profiles
- Better diagnostics when PDFsharp cannot open a given encryption scheme
