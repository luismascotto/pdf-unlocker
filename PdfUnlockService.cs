using System.Diagnostics;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;

namespace PdfUnlocker;

internal static class PdfUnlockService
{
    public enum UnlockStatus
    {
        Success,
        WrongPassword,
        Failed,
    }

    public sealed record UnlockResult(UnlockStatus Status, string? OutputPath, string? Message, int PagesWritten = 0);

    /// <param name="maxPages">
    /// When set, only the first N pages are written (boleto/barcode page is usually page 1).
    /// Null means keep every page.
    /// </param>
    public static UnlockResult TryUnlock(string inputPath, string password, string outputPath, int? maxPages = null)
    {
        if (string.IsNullOrWhiteSpace(inputPath) || string.IsNullOrWhiteSpace(password))
        {
            return new UnlockResult(UnlockStatus.Failed, null, "Input path and password are required.");
        }

        if (maxPages is <= 0)
        {
            return new UnlockResult(UnlockStatus.Failed, null, "Page count must be a positive number.");
        }

        try
        {
            // Page limiting needs Import (copy selected pages). Prefer it when --pages is set.
            if (maxPages is not null)
            {
                if (TryUnlockByImport(inputPath, password, outputPath, maxPages, out var pagesWritten))
                {
                    return new UnlockResult(UnlockStatus.Success, outputPath, null, pagesWritten);
                }

                return new UnlockResult(UnlockStatus.WrongPassword, null, "Password did not open the PDF.");
            }

            if (TryUnlockByModify(inputPath, password, outputPath, out var modifyPages))
            {
                return new UnlockResult(UnlockStatus.Success, outputPath, null, modifyPages);
            }

            if (TryUnlockByImport(inputPath, password, outputPath, maxPages: null, out var importPages))
            {
                return new UnlockResult(UnlockStatus.Success, outputPath, null, importPages);
            }

            return new UnlockResult(UnlockStatus.WrongPassword, null, "Password did not open the PDF.");
        }
        catch (PdfReaderException ex) when (IsInvalidPassword(ex))
        {
            return new UnlockResult(UnlockStatus.WrongPassword, null, ex.Message);
        }
        catch (Exception ex)
        {
            return new UnlockResult(UnlockStatus.Failed, null, ex.Message);
        }
    }

    public static void OpenInDefaultViewer(string path)
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = path,
            UseShellExecute = true,
        });
    }

    private static bool TryUnlockByModify(string inputPath, string password, string outputPath, out int pagesWritten)
    {
        pagesWritten = 0;
        try
        {
            using var document = PdfReader.Open(inputPath, password, PdfDocumentOpenMode.Modify);
            document.SecurityHandler.SetEncryptionToNoneAndResetPasswords();
            pagesWritten = document.PageCount;
            EnsureOutputDirectory(outputPath);
            document.Save(outputPath);
            return File.Exists(outputPath);
        }
        catch (PdfReaderException ex) when (IsInvalidPassword(ex))
        {
            return false;
        }
        catch (PdfReaderException)
        {
            // Modify may require the owner password; fall back to Import.
            return false;
        }
    }

    private static bool TryUnlockByImport(
        string inputPath,
        string password,
        string outputPath,
        int? maxPages,
        out int pagesWritten)
    {
        pagesWritten = 0;
        try
        {
            using var source = PdfReader.Open(inputPath, password, PdfDocumentOpenMode.Import);
            using var unlocked = new PdfDocument();

            var take = maxPages is null
                ? source.PageCount
                : Math.Min(maxPages.Value, source.PageCount);

            for (var i = 0; i < take; i++)
            {
                unlocked.AddPage(source.Pages[i]);
            }

            pagesWritten = unlocked.PageCount;
            EnsureOutputDirectory(outputPath);
            unlocked.Save(outputPath);
            return File.Exists(outputPath);
        }
        catch (PdfReaderException ex) when (IsInvalidPassword(ex))
        {
            return false;
        }
    }

    private static void EnsureOutputDirectory(string outputPath)
    {
        var directory = Path.GetDirectoryName(outputPath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }
    }

    private static bool IsInvalidPassword(Exception ex)
    {
        var message = ex.Message ?? string.Empty;
        return message.Contains("password", StringComparison.OrdinalIgnoreCase);
    }
}
