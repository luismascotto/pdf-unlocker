# PdfUnlocker

A small Windows-friendly CLI that unlocks password-protected PDF bills and boletos using **your** known digit patterns (CPF / CNPJ / CEP prefixes).

Brazilian credit-card and payment PDFs often use a fixed open-password such as the first N digits of a CPF/CNPJ, sometimes combined with CEP digits. Chrome’s password manager cannot reuse those usefully because filenames are unstructured and the password never changes per issuer pattern—but it is unique to you. This tool keeps candidate passwords in a local `passwords.txt` and tries them until one opens the file.

**Use only on PDFs you are authorized to open.** PdfUnlocker does not crack unknown passwords; it only tries the candidates you configure.

## Features

- Single-file friendly CLI for Windows (`win-x64`)
- Default password list from `passwords.txt` beside the executable
- Works from **Windows Send To** (right-click a PDF)
- Unlocks without Aspose free-tier watermarks (uses [PDFsharp](https://docs.pdfsharp.net/), MIT)
- Optional first-N-pages export (`-n 1` for boleto/barcode only)
- Clear `--help` via [Spectre.Console.Cli](https://spectreconsole.net/)

## Install / build

Requirements: [.NET 10 SDK](https://dotnet.microsoft.com/download) (to build).

```bat
publish_here.bat
```

Or:

```bat
dotnet publish PdfUnlocker.csproj -c Release -o publish --self-contained true -r win-x64
copy passwords.example.txt publish\passwords.example.txt
```

Then create your real list:

```bat
copy publish\passwords.example.txt publish\passwords.txt
notepad publish\passwords.txt
```

## Configure passwords

Place `passwords.txt` next to `PdfUnlocker.exe` (the publish/output folder).

- One password candidate per line
- Blank lines and lines starting with `#` are ignored
- Never commit `passwords.txt` — it may contain fragments of your CPF/CNPJ/CEP

Typical Brazilian patterns (examples only):

| Pattern | Example shape |
|---|---|
| First 4 CPF digits | `1234` |
| First 5 CPF digits | `12345` |
| First 6 CPF digits | `123456` |
| First 5 CNPJ digits | `12345` |
| 4 CPF + 5 CEP | `123412345` |

Override the file location with `--passwords-file`, or skip the file and pass `-p`.

## Usage

```bat
PdfUnlocker.exe invoice.pdf
PdfUnlocker.exe -f invoice.pdf -p 1234
PdfUnlocker.exe invoice.pdf -n 1
PdfUnlocker.exe invoice.pdf -o C:\out\invoice_unlocked.pdf --no-open
PdfUnlocker.exe invoice.pdf --passwords-file D:\my\passwords.txt
PdfUnlocker.exe --help
```

Defaults:

- Output: `<same-folder>\<name>_unlocked.pdf`
- Keeps all pages unless you pass `-n|--pages` (e.g. `-n 1` for the boleto page only)
- Opens the unlocked PDF in the default viewer (`--no-open` to disable)
- Without `-p`, loads `passwords.txt` from the application folder

## Windows Send To

1. Publish or copy `PdfUnlocker.exe` to a stable folder (and keep `passwords.txt` beside it).
2. Open the Send To folder: press `Win+R`, run `shell:sendto`.
3. Create a shortcut to `PdfUnlocker.exe` there (name it e.g. `PdfUnlocker`).
4. Right-click any PDF → **Send to** → **PdfUnlocker**.

Windows passes the file path as the first argument, which matches the default CLI usage.

## Privacy

- Passwords stay on your machine.
- Do not share screenshots that show `passwords.txt`.
- Add `passwords.txt` to `.gitignore` (already listed).

## Limitations

- Built and documented first for Windows `win-x64` self-contained publish.
- Uses PDFsharp for decrypt/save. Most user-password bills work; unusual owner-password-only or exotic encryption cases may fail.
- Does not remove DRM beyond standard PDF user/owner open passwords.

## License

MIT — see [LICENSE](LICENSE).
