namespace JobBoard.API
{
    /// <summary>
    /// Asılılıqsız (dependency-free) sadə .env yükləyici.
    /// "KEY=VALUE" formatında sətirləri oxuyub proses mühit dəyişənlərinə yazır.
    /// Artıq təyin edilmiş mühit dəyişənləri (real env) override EDİLMİR.
    /// </summary>
    public static class DotEnv
    {
        public static void Load(string? fileName = ".env")
        {
            var path = ResolvePath(fileName!);
            if (path is null) return;

            foreach (var rawLine in File.ReadAllLines(path))
            {
                var line = rawLine.Trim();

                // Boş sətir və ya şərh
                if (line.Length == 0 || line.StartsWith('#'))
                    continue;

                var separatorIndex = line.IndexOf('=');
                if (separatorIndex <= 0)
                    continue;

                var key = line[..separatorIndex].Trim();
                var value = line[(separatorIndex + 1)..].Trim();

                // Dəyər dırnaq içindədirsə, dırnaqları sil
                if (value.Length >= 2 &&
                    ((value[0] == '"' && value[^1] == '"') ||
                     (value[0] == '\'' && value[^1] == '\'')))
                {
                    value = value[1..^1];
                }

                // Mövcud mühit dəyişəni varsa, üzərinə yazma (real env üstünlük təşkil edir)
                if (Environment.GetEnvironmentVariable(key) is null)
                    Environment.SetEnvironmentVariable(key, value);
            }
        }

        /// <summary>.env faylını bir neçə mümkün yerdə axtarır.</summary>
        private static string? ResolvePath(string fileName)
        {
            var candidates = new[]
            {
                Path.Combine(Directory.GetCurrentDirectory(), fileName),
                Path.Combine(AppContext.BaseDirectory, fileName)
            };

            return candidates.FirstOrDefault(File.Exists);
        }
    }
}
