import re

with open('Backend/Controllers/IAController.cs', 'r', encoding='utf-8') as f:
    code = f.read()

# Replace using
code = code.replace("using System.Linq;", "using System.Linq;\nusing Google.Apis.Auth.OAuth2;")

# Replace AnalizarFactura
target1 = """                var apiKey = "AIzaSyC6M26s6rDMecZpX-GDKwzzvPQSRM18OxM";
                var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key={apiKey}";

                var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(url, content);"""
replacement1 = """                var auth = await GetVertexAuthAsync();
                
                var requestMsg = new HttpRequestMessage(HttpMethod.Post, auth.Url);
                requestMsg.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
                requestMsg.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", auth.Token);

                var response = await _httpClient.SendAsync(requestMsg);"""
code = code.replace(target1.replace('\n', '\r\n'), replacement1.replace('\n', '\r\n'))

# Replace Chatbot URL
target2 = """            var apiKey = "AIzaSyC6M26s6rDMecZpX-GDKwzzvPQSRM18OxM";
            var geminiUrl = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key={apiKey}";"""
replacement2 = """            var auth = await GetVertexAuthAsync();"""
code = code.replace(target2.replace('\n', '\r\n'), replacement2.replace('\n', '\r\n'))

# Replace Chatbot SQL post
target3 = """                var sqlPayload = new { contents = new[] { new { parts = new[] { new { text = promptSQL } } } } };
                var sqlContent = new StringContent(JsonSerializer.Serialize(sqlPayload), Encoding.UTF8, "application/json");
                var sqlResponse = await _httpClient.PostAsync(geminiUrl, sqlContent);"""
replacement3 = """                var sqlPayload = new { contents = new[] { new { parts = new[] { new { text = promptSQL } } } } };
                var sqlReq = new HttpRequestMessage(HttpMethod.Post, auth.Url) 
                { 
                    Content = new StringContent(JsonSerializer.Serialize(sqlPayload), Encoding.UTF8, "application/json") 
                };
                sqlReq.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", auth.Token);
                var sqlResponse = await _httpClient.SendAsync(sqlReq);"""
code = code.replace(target3.replace('\n', '\r\n'), replacement3.replace('\n', '\r\n'))

# Replace Chatbot Final post
target4 = """                var finalPayload = new { contents = new[] { new { parts = new[] { new { text = promptFinal } } } } };
                var finalContent = new StringContent(JsonSerializer.Serialize(finalPayload), Encoding.UTF8, "application/json");
                var finalResponse = await _httpClient.PostAsync(geminiUrl, finalContent);"""
replacement4 = """                var finalPayload = new { contents = new[] { new { parts = new[] { new { text = promptFinal } } } } };
                var finalReq = new HttpRequestMessage(HttpMethod.Post, auth.Url)
                {
                    Content = new StringContent(JsonSerializer.Serialize(finalPayload), Encoding.UTF8, "application/json")
                };
                finalReq.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", auth.Token);
                var finalResponse = await _httpClient.SendAsync(finalReq);"""
code = code.replace(target4.replace('\n', '\r\n'), replacement4.replace('\n', '\r\n'))

# Insert helper method
target5 = """        }
    }

    public class ChatRequest"""
replacement5 = """        }

        private async Task<(string Url, string Token)> GetVertexAuthAsync()
        {
            string projectId = "ambient-glazing-462000-f4";
            string location = "us-central1"; 
            string model = "gemini-2.5-flash";
            string url = $"https://{location}-aiplatform.googleapis.com/v1/projects/{projectId}/locations/{location}/publishers/google/models/{model}:generateContent";

            string jsonPath = Path.Combine(Directory.GetCurrentDirectory(), "google-credentials.json");
            using var stream = new FileStream(jsonPath, FileMode.Open, FileAccess.Read);
            var credential = GoogleCredential.FromStream(stream).CreateScoped("https://www.googleapis.com/auth/cloud-platform");
            var token = await ((ITokenAccess)credential).GetAccessTokenForRequestAsync();
            return (url, token);
        }
    }

    public class ChatRequest"""
code = code.replace(target5.replace('\n', '\r\n'), replacement5.replace('\n', '\r\n'))

with open('Backend/Controllers/IAController.cs', 'w', encoding='utf-8') as f:
    f.write(code)

print("Patched successfully")
