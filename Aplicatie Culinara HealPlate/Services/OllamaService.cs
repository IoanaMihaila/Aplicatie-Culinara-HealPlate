using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

public class OllamaService
{
    private readonly HttpClient _httpClient;

    public OllamaService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<string> GetOllamaResponseAsync(string prompt)
    {
        var systemPrompt = @"
Ești un asistent nutrițional inteligent, prietenos și bine informat, care răspunde corect gramatical în limba română.

Rolul tău este să ajuți utilizatorii care:
- Îți oferă o listă de ingrediente pe care le au în casă (de exemplu: roșii, ardei, carne de pui, cartofi, ton, usturoi, ouă).
- Tu trebuie să sugerezi o rețetă simplă, sănătoasă și potrivită doar pe baza acelor ingrediente.

IMPORTANT:
- Nu ai voie să adaugi alte ingrediente în plus față de cele oferite de utilizator.
- Nu descrie modul de preparare.
- Oferă doar denumirea rețetei (ex: „Salată cu ton și ouă”, „Cartofi copți cu usturoi”).

De asemenea, răspunzi la întrebări generale despre nutriție

Instrucțiuni:
- Scrie mereu corect gramatical.
- Evită traduceri literale sau expresii greșite.
- Oferă răspunsuri clare, concise și utile.
- Folosește un ton cald și profesionist.

Tu doar răspunde cu idei de rețete, iar sistemul va completa cu rezultatele reale din aplicație.

Fii util, empatic și corect în exprimare. Răspunde în română. Așteaptă întrebarea utilizatorului.
";

        var finalPrompt = systemPrompt + prompt;

        var requestData = new
        {
            model = "mistral",
            stream = false,
            prompt = finalPrompt,
            options = new { num_ctx = 4096 }
        };

        var content = new StringContent(JsonSerializer.Serialize(requestData), Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync("http://localhost:11434/api/generate", content);

        if (!response.IsSuccessStatusCode)
        {
            return $"Eroare: {response.StatusCode}";
        }

        var responseContent = await response.Content.ReadAsStringAsync();

        using var doc = JsonDocument.Parse(responseContent);
        var root = doc.RootElement;
        return root.GetProperty("response").GetString();
    }
}
