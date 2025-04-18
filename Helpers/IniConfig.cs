using IniParser;
using IniParser.Model;
using System;
using System.IO;

namespace WormsDirectManagement.Helpers
{
    internal class IniConfig
    {
        private readonly IniData _data;

        private IniConfig(IniData data) => _data = data;

        public static IniConfig Load(string? path = null)
        {
            path ??= @"D:\Sync\Businesses\Worms Direct\Scripts\Downloading Attachments\config.ini";
            if (!File.Exists(path))
                throw new FileNotFoundException("Config not found", path);

            var parser = new FileIniDataParser();
            var data = parser.ReadFile(path);

            // strip comments & expand ${ENV_VAR}
            foreach (var section in data.Sections)
                foreach (var key in section.Keys)
                {
                    var raw = key.Value.Split('#')[0].Trim();
                    if (raw.StartsWith("${") && raw.EndsWith("}"))
                    {
                        var env = raw[2..^1];
                        raw = Environment.GetEnvironmentVariable(env) ?? "";
                    }

                    key.Value = raw;
                }

            return new IniConfig(data);
        }

        public string this[string section, string key] => _data[section][key];

        public int Int(string section, string key, int def = 0)
            => int.TryParse(this[section, key], out var v) ? v : def;
    }
}
