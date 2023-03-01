using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Net.Http.Json;
using System.Linq;

namespace LuisBot.Helper
{
    public class GetCommonAnswer
    {
        public static async Task<string> GetAnswer(string question)
        {
            var body = JsonConvert.SerializeObject(new Question()
            {
                question = question,
            });

            var baseUrl = "https://covid19languagesevice.cognitiveservices.azure.com/language/:query-knowledgebases?projectName=LeaveRequest&api-version=2021-10-01&deploymentName=production";
            var content = new StringContent(body, Encoding.UTF8, "application/json");
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(baseUrl);
                client.DefaultRequestHeaders.Add("Accept", "application/json");
                client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", "876ee52635ef4e87b553bdd6881ee599");
                var result = await client.PostAsync(baseUrl, content);
                var data = await result.Content.ReadFromJsonAsync<Rootobject>();
                var answer = data?.answers?.FirstOrDefault()?.answer;

                return answer;
                //result.EnsureSuccessStatusCode();
            }


        }
    }
    
    public class Question
    {
        public string question { get; set; } = null!;
    }
        public class Rootobject
    {
        public Answer[] answers { get; set; }
    }

    public class Answer
    {
        public string[] questions { get; set; }
        public string answer { get; set; }
        public float confidenceScore { get; set; }
        public int id { get; set; }
        public string source { get; set; }
        public Metadata metadata { get; set; }
        public Dialog dialog { get; set; }
    }

    public class Metadata
    {
        public string system_metadata_qna_edited_manually { get; set; }
    }

    public class Dialog
    {
        public bool isContextOnly { get; set; }
        public object[] prompts { get; set; }
    }
}
