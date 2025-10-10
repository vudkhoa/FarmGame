using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Utils.CsvTool
{
    public static class CsvTool
    {
        // Simple parser: each cell has no commas or quotation marks.
        public static IEnumerable<Dictionary<string, string>> Read(string path)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException($"CSV not found: {path}");

            var lines = File.ReadAllLines(path, Encoding.UTF8);
            if (lines.Length == 0) yield break;

            var header = lines[0].Trim();
            var keys = header.Split(',');

            for (int i = 1; i < lines.Length; i++)
            {
                var line = lines[i].Trim();
                if (string.IsNullOrWhiteSpace(line)) continue;

                var parts = line.Split(',');
                var row = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                for (int c = 0; c < keys.Length && c < parts.Length; c++)
                    row[keys[c]] = parts[c].Trim();
                yield return row;
            }
        }
    }
}