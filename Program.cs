using Spectre.Console.Cli;

namespace PdfUnlocker;

public static class Program
{
    public static int Main(string[] args)
    {
        var app = new CommandApp<UnlockCommand>();
        app.Configure(config =>
        {
            config.SetApplicationName("PdfUnlocker");
            config.SetApplicationVersion("1.0.0");
            config.ValidateExamples();
            config.AddExample("invoice.pdf");
            config.AddExample("-f", "invoice.pdf", "-p", "1234");
            config.AddExample("invoice.pdf", "-n", "1");
            config.AddExample("invoice.pdf", "-o", "C:\\out\\invoice_unlocked.pdf", "--no-open");
        });

        return app.Run(args);
    }
}
