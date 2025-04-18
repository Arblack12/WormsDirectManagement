using Newtonsoft.Json;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using WormsDirectManagement.Helpers;
using WormsDirectManagement.Models;

namespace WormsDirectManagement.Services
{
    internal static class InvoiceProcessor
    {
        private static readonly Logger _log = Log.Get();

        private const string BaseDir = @"D:\Sync\Businesses\Worms Direct\Invoices";
        private const string EmailPattern = @"^([a-zA-Z0-9_.+-]+@[a-zA-Z0-9-]+\.[a-zA-Z0-9-.]+)_";

        public static string Process()
        {
            var companies = LoadRules();
            var report = new List<string>();

            foreach (var file in Directory.EnumerateFiles(BaseDir))
            {
                var fileName = Path.GetFileName(file);
                var match = Regex.Match(fileName, EmailPattern);
                if (!match.Success)
                {
                    report.Add($"Email not found in {fileName}");
                    continue;
                }

                var email = match.Groups[1].Value.ToLower();
                if (!companies.TryGetValue(email, out var rule))
                {
                    report.Add($"Unknown sender {email}");
                    continue;
                }

                var targetDate = DateTime.Today
                    .AddMonths(rule.MonthOffset)
                    .AddDays(rule.DayOffset);

                var destFolder = Path.Combine(BaseDir,
                                              targetDate.Year.ToString(),
                                              targetDate.ToString("MMMM"),
                                              string.IsNullOrWhiteSpace(rule.FolderName) ? ""
                                                    : rule.FolderName);
                Directory.CreateDirectory(destFolder);

                var nextNumber = GetNextNumber(destFolder, rule.FileName);
                var destName = $"{rule.FileName}-{nextNumber}{Path.GetExtension(fileName)}";
                var destPath = Path.Combine(destFolder, destName);

                File.Move(file, destPath);
                report.Add($"Moved → {destPath}");
            }

            _log.Info("Invoice processing finished");
            return string.Join(Environment.NewLine, report);
        }

        private static int GetNextNumber(string folder, string prefix)
            => Directory.EnumerateFiles(folder, $"{prefix}-*")
                        .Select(f => Regex.Match(Path.GetFileName(f), @"-(\d+)\.").Groups[1].Value)
                        .Select(s => int.TryParse(s, out var n) ? n : 0)
                        .DefaultIfEmpty(0)
                        .Max() + 1;

        private static Dictionary<string, SenderRule> LoadRules()
        {
            var exeDir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)!;
            var path = Path.Combine(exeDir, "invoices_config.json");
            if (!File.Exists(path)) return new();

            var list = JsonConvert.DeserializeObject<List<SenderRule>>(File.ReadAllText(path))
                       ?? new List<SenderRule>();
            return list.ToDictionary(r => r.SenderEmail.ToLower());
        }
    }
}
