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
            string url;
            
            if (!string.IsNullOrWhiteSpace(query))
            {
                url = $"https://www.myinstants.com/en/search/?name={Uri.EscapeDataString(query.Trim())}";
                if (page > 1) url += $"&page={page}";
            }
            else if (!string.IsNullOrWhiteSpace(category))
            {
                url = $"https://www.myinstants.com/en/categories/{category}/it/";
                if (page > 1) url += $"?page={page}";
            }
            else
            {
                url = "https://www.myinstants.com/en/index/it/";
                if (page > 1) url += $"?page={page}";
            }

            try
            {
                var html = await _http.GetStringAsync(url);
                var items = ParseHtml(html);
                
                string? nextUrl = items.Count >= 8 ? (url.Contains("?") ? $"{url}&page={page + 1}" : $"{url}?page={page + 1}") : null;
                return new MyInstantsResponse(items.Count, nextUrl, null, items);
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
                var html = await _http.GetStringAsync(url);
                var items = ParseHtml(html);
                
                // Estrae il numero di pagina corrente dall'URL della pagina caricata per impostare il link alla pagina successiva
                int page = 1;
                var uri = new Uri(url);
                var queryParams = System.Web.HttpUtility.ParseQueryString(uri.Query);
                string? pageStr = queryParams.Get("page");
                if (string.IsNullOrEmpty(pageStr))
                {
                    // Controlla se la pagina è nel path del tipo ?page= o simile
                    int pageIndex = url.IndexOf("page=");
                    if (pageIndex != -1 && int.TryParse(url.Substring(pageIndex + 5), out int p))
                        page = p;
                }
                else
                {
                    int.TryParse(pageStr, out page);
                }

                // Genera l'URL della pagina successiva rimuovendo eventuali parametri di pagina vecchi
                string cleanUrl = url;
                int idx = url.IndexOf("page=");
                if (idx != -1)
                {
                    cleanUrl = url.Substring(0, idx - 1); // Rimuove "&page=" o "?page="
                }
                string nextUrl = cleanUrl.Contains("?") ? $"{cleanUrl}&page={page + 1}" : $"{cleanUrl}?page={page + 1}";

                return new MyInstantsResponse(items.Count, items.Count >= 8 ? nextUrl : null, null, items);
            }
            catch
            {
                return null;
            }
        }

        private List<MyInstantItem> ParseHtml(string html)
        {
            var list = new List<MyInstantItem>();
            if (string.IsNullOrEmpty(html)) return list;

            int index = 0;
            while (true)
            {
                index = html.IndexOf("<div class=\"instant\"", index);
                if (index == -1) break;

                int nextIndex = html.IndexOf("<div class=\"instant\"", index + 20);
                string block = nextIndex == -1 ? html.Substring(index) : html.Substring(index, nextIndex - index);
                index = nextIndex == -1 ? html.Length : nextIndex;

                try
                {
                    // 1. Estrae Colore
                    string color = "FF0000";
                    int colorIdx = block.IndexOf("background-color:");
                    if (colorIdx != -1)
                    {
                        int start = colorIdx + "background-color:".Length;
                        int end = block.IndexOfAny(new char[] { ';', '"', '\'' }, start);
                        if (end != -1)
                        {
                            color = block.Substring(start, end - start).Trim().Replace("#", "");
                        }
                    }

                    // 2. Estrae Sound URL
                    string soundUrl = "";
                    int playIdx = block.IndexOf("play('");
                    if (playIdx != -1)
                    {
                        int start = playIdx + "play('".Length;
                        int end = block.IndexOf("'", start);
                        if (end != -1)
                        {
                            string path = block.Substring(start, end - start);
                            if (path.StartsWith("/"))
                                soundUrl = "https://www.myinstants.com" + path;
                            else
                                soundUrl = path;
                        }
                    }

                    // 3. Estrae Slug e Nome
                    string slug = "";
                    string name = "";
                    int hrefIdx = block.IndexOf("href=\"/en/instant/");
                    if (hrefIdx == -1)
                        hrefIdx = block.IndexOf("href=\"/instant/");

                    if (hrefIdx != -1)
                    {
                        int startHref = block.IndexOf("/", hrefIdx + 6);
                        startHref = block.IndexOf("/", startHref + 1);
                        startHref = block.IndexOf("/", startHref + 1);
                        int endHref = block.IndexOf("/", startHref + 1);
                        if (endHref != -1)
                        {
                            slug = block.Substring(startHref + 1, endHref - startHref - 1);
                        }

                        // Nome
                        int closeTagIdx = block.IndexOf(">", hrefIdx);
                        if (closeTagIdx != -1)
                        {
                            int endTagIdx = block.IndexOf("</a>", closeTagIdx);
                            if (endTagIdx != -1)
                            {
                                name = block.Substring(closeTagIdx + 1, endTagIdx - closeTagIdx - 1).Trim();
                                name = System.Net.WebUtility.HtmlDecode(name);
                            }
                        }
                    }

                    if (!string.IsNullOrEmpty(soundUrl) && !string.IsNullOrEmpty(name))
                    {
                        list.Add(new MyInstantItem(name, slug, soundUrl, color, ""));
                    }
                }
                catch { }
            }

            return list;
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
