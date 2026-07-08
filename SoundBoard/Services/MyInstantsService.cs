using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using SoundBoard.Models;

namespace SoundBoard.Services
{
    public class MyInstantsService
    {
        private static readonly HttpClient _http = new HttpClient();
        private const string BaseUrl = "https://www.myinstants.com/api/v1/instants/";

        static MyInstantsService()
        {
            _http.DefaultRequestHeaders.Add("User-Agent",
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
            _http.DefaultRequestHeaders.Add("Referer", "https://www.myinstants.com/");
            _http.DefaultRequestHeaders.Add("Origin", "https://www.myinstants.com");
        }

        public async Task<MyInstantsResponse?> SearchAsync(string query = "", int page = 1, string category = "")
        {
            var url = BaseUrl + "?format=json";
            
            string effectiveQuery = query;

            if (string.IsNullOrWhiteSpace(effectiveQuery) && !string.IsNullOrWhiteSpace(category))
            {
                var categoryLower = category.ToLower().Trim();
                switch (categoryLower)
                {
                    case "politics":
                    case "politica":
                        effectiveQuery = "politica";
                        break;
                    case "meme":
                    case "memes":
                        effectiveQuery = "meme";
                        break;
                    case "games":
                    case "giochi":
                        effectiveQuery = "game";
                        break;
                    case "sports":
                    case "sport":
                        effectiveQuery = "sport";
                        break;
                    case "television":
                    case "televisione":
                        effectiveQuery = "tv";
                        break;
                    case "music":
                    case "musica":
                        effectiveQuery = "music";
                        break;
                    case "sound-effects":
                    case "effetti-sonori":
                        effectiveQuery = "effect";
                        break;
                    case "movie":
                    case "film":
                        effectiveQuery = "movie";
                        break;
                    case "anime":
                        effectiveQuery = "anime";
                        break;
                    default:
                        effectiveQuery = categoryLower;
                        break;
                }
            }

            if (!string.IsNullOrWhiteSpace(effectiveQuery))
                url += $"&name={Uri.EscapeDataString(effectiveQuery)}";
            
            if (page > 1)
                url += $"&page={page}";

            try
            {
                var json = await _http.GetStringAsync(url);
                return JsonSerializer.Deserialize<MyInstantsResponse>(json);
            }
            catch
            {
                return null;
            }
        }

        public async Task<MyInstantsResponse?> FetchPageAsync(string url)
        {
            try
            {
                var json = await _http.GetStringAsync(url);
                return JsonSerializer.Deserialize<MyInstantsResponse>(json);
            }
            catch
            {
                return null;
            }
        }

        public async Task<string> DownloadSoundAsync(MyInstantItem item, string destFolder)
        {
            Directory.CreateDirectory(destFolder);
            var ext = Path.GetExtension(new Uri(item.SoundUrl).LocalPath);
            if (string.IsNullOrEmpty(ext)) ext = ".mp3";
            var safeName = string.Concat(item.Name.Split(Path.GetInvalidFileNameChars()));
            var destPath = Path.Combine(destFolder, safeName + ext);

            // Avoid duplicate filenames
            int n = 1;
            while (File.Exists(destPath))
                destPath = Path.Combine(destFolder, safeName + $"_{n++}" + ext);

            var bytes = await _http.GetByteArrayAsync(item.SoundUrl);
            await File.WriteAllBytesAsync(destPath, bytes);
            return destPath;
        }
    }
}
