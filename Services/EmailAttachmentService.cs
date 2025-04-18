// Services/EmailAttachmentService.cs  (top of file)
using System.Windows.Forms;        // ✅ NotifyIcon / MessageBox
using MailKit.Net.Imap;
using MailKit;
using MimeKit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using WormsDirectManagement.Helpers;
using MailKit.Search;
using NLog;

namespace WormsDirectManagement.Services
{
    public sealed class EmailAttachmentService
    {
        private readonly Logger _log = Log.Get();
        private readonly IniConfig _cfg;

        private readonly int _initialBackoff;
        private readonly int _backoffFactor;
        private readonly int _maxBackoff;

        private readonly Timer _timer;
        private int _failures;
        private int _pollInterval;

        public string DownloadFolder { get; }

        public EmailAttachmentService(IniConfig cfg)
        {
            _cfg = cfg;
            _pollInterval = cfg.Int("SETTINGS", "POLLING_INTERVAL", 600);
            _initialBackoff = cfg.Int("SETTINGS", "INITIAL_BACKOFF", 60);
            _backoffFactor = cfg.Int("SETTINGS", "BACKOFF_FACTOR", 2);
            _maxBackoff = cfg.Int("SETTINGS", "MAX_BACKOFF", 3600);

            DownloadFolder = cfg["DOWNLOAD", "FOLDER_PATH"];
            Directory.CreateDirectory(DownloadFolder);

            _timer = new Timer(async _ => await CheckEmailsAsync(),
                               null, _pollInterval * 1000, Timeout.Infinite);
        }

        public void Pause() => _timer.Change(Timeout.Infinite, Timeout.Infinite);
        public void Resume() => _timer.Change(_pollInterval * 1000, Timeout.Infinite);

        public async Task CheckEmailsAsync()
        {
            if (_timer.Change(Timeout.Infinite, Timeout.Infinite) != null) { }

            _log.Info("Checking for new emails…");

            try
            {
                using var client = new ImapClient();
                await client.ConnectAsync(_cfg["IMAP", "SERVER"],
                                          int.Parse(_cfg["IMAP", "PORT"]), true);
                await client.AuthenticateAsync(_cfg["EMAIL", "ACCOUNT"],
                                               _cfg["EMAIL", "PASSWORD"]);

                var inbox = client.GetFolder(_cfg["IMAP", "FOLDER"]);
                await inbox.OpenAsync(FolderAccess.ReadWrite);

                var uids = (await inbox.SearchAsync(SearchQuery.All)).OrderByDescending(u => u.Id);

                int downloaded = 0;
                foreach (var uid in uids)
                {
                    var msg = await inbox.GetMessageAsync(uid);
                    if (!msg.Attachments.Any()) continue;

                    foreach (var att in msg.Attachments.OfType<MimePart>())
                    {
                        var safeFile = Sanitize(att.FileName ?? "unnamed");
                        var newName = BuildFileName(msg, uid.Id, safeFile);
                        var path = Path.Combine(DownloadFolder, newName);

                        await using var stream = File.Create(path);
                        await att.Content.DecodeToAsync(stream);
                        downloaded++;
                        _log.Info($"Downloaded {newName}");
                    }
                }

                if (downloaded > 0)
                    Notify($"Downloaded {downloaded} new attachment(s).");

                // success → reset back‑off
                _failures = 0;
                _timer.Change(_pollInterval * 1000, Timeout.Infinite);
            }
            catch (Exception ex)
            {
                _log.Error(ex);
                Notify($"Error: {ex.Message}", true);

                _failures++;
                var delay = Math.Min(_initialBackoff * (int)Math.Pow(_backoffFactor, _failures), _maxBackoff);
                _log.Info($"Applying back‑off: {delay}s");
                _timer.Change(delay * 1000, Timeout.Infinite);
            }
        }

        private void Notify(string msg, bool error = false)
        {
            System.Windows.Forms.MessageBox.Show(msg,
                "Worms Direct Management",
                error ? System.Windows.Forms.MessageBoxButtons.OK
                      : System.Windows.Forms.MessageBoxButtons.OK,
                error ? System.Windows.Forms.MessageBoxIcon.Error
                      : System.Windows.Forms.MessageBoxIcon.Information);
        }

        private static string Sanitize(string s)
            => Regex.Replace(s, @"[<>:""/\\|?*]", "_");

        private string BuildFileName(MimeMessage msg, uint uid, string original)
        {
            var sender = Sanitize(msg.From.Mailboxes.First().Address);
            var subject = Sanitize(msg.Subject ?? "No_Subject");
            var dateStr = DateTime.Now.ToString("yyyyMMdd");

            var composed =
                $"{sender}_{subject}_{dateStr}_{uid}_{original}";

            // avoid collision
            var path = Path.Combine(DownloadFolder, composed);
            if (!File.Exists(path)) return composed;

            var ts = DateTime.Now.ToString("yyyyMMddHHmmss");
            var name = Path.GetFileNameWithoutExtension(composed);
            var ext = Path.GetExtension(composed);
            return $"{name}_{ts}{ext}";
        }
    }
}
