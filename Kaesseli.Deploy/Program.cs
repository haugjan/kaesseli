using System.Diagnostics;

var composeFile = Path.Combine(AppContext.BaseDirectory, "docker-compose.yml");

Console.WriteLine("Starting CosmosDB local emulator...");

var process = Process.Start(new ProcessStartInfo
{
    FileName = "docker",
    Arguments = $"compose -f \"{composeFile}\" up -d",
    UseShellExecute = false,
    RedirectStandardOutput = true,
    RedirectStandardError = true,
});

if (process is null)
{
    Console.Error.WriteLine("Failed to start docker. Is Docker Desktop running?");
    return 1;
}

var readOutput = process.StandardOutput.BaseStream.CopyToAsync(Console.OpenStandardOutput());
var readError  = process.StandardError.BaseStream.CopyToAsync(Console.OpenStandardError());
await Task.WhenAll(readOutput, readError, process.WaitForExitAsync());

if (process.ExitCode != 0)
{
    Console.Error.WriteLine($"docker compose exited with code {process.ExitCode}");
    return process.ExitCode;
}

Console.WriteLine();
Console.WriteLine("CosmosDB emulator is running.");
Console.WriteLine("  Endpoint : https://localhost:8081");
Console.WriteLine("  Explorer : https://localhost:8081/_explorer/index.html");
return 0;
