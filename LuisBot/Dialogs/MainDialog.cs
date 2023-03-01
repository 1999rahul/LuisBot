using LuisQnABot;
using Microsoft.Bot.Builder;
//using Microsoft.Bot.Builder.AI.QnA;
using Microsoft.Bot.Builder.Dialogs;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using System.Collections.Generic;
using System.Text;

namespace LuisBot.Dialogs
{
    public class MainDialog: ComponentDialog
    {
        private readonly LusRecognizer _luisRecognizer;
        //public QnAMaker qnAMakerObject { get; private set; }

        public MainDialog(LusRecognizer luisRecognizer)//, QnAMakerEndpoint endpoint)
           : base(nameof(MainDialog))
        {
            _luisRecognizer = luisRecognizer;
            //qnAMakerObject = new QnAMaker(endpoint);

            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {

               IntroStepAsync,
               ActStepAsync,
               FinalStepAsync,
            }));

            // The initial child Dialog to run.  
            InitialDialogId = nameof(WaterfallDialog);
        }
        private async Task<DialogTurnResult> IntroStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { }, cancellationToken);
        }

        private async Task<DialogTurnResult> ActStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var luisResult = await _luisRecognizer.RecognizeAsync(stepContext.Context, cancellationToken);
            if (luisResult.Intents.ContainsKey("Covid19India"))
            {
                return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions
                {
                    Prompt = MessageFactory.Text("India Covid19 Data : Confirmed - 49400, Active - 33565,  Recovered - 14142")
                }, cancellationToken) ;
            }
            else if (luisResult.Intents.ContainsKey("Covid19Global"))
            {

                return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions
                {
                    Prompt = MessageFactory.Text("World Covid19 Data :Confirmed - 494700, Active - 323565,  Recovered - 214142")
                }, cancellationToken);
            }
            else
            {
               
                var client = new HttpClient();
                var body = "{" + $"question:{stepContext.Result.ToString()}" + "}";
                var httpRequestMessage = new HttpRequestMessage
                {
                    Method = HttpMethod.Post,
                    RequestUri = new Uri("https://covid19languagesevice.cognitiveservices.azure.com/language/:query-knowledgebases?projectName=LeaveRequest&api-version=2021-10-01&deploymentName=production"),
                    Headers = {
                        { "Ocp-Apim-Subscription-Key", "876ee52635ef4e87b553bdd6881ee599" }
                    },

                    Content = new StringContent(body, Encoding.UTF8, "application/json")
                };
                var response = client.SendAsync(httpRequestMessage).Result;

                /* var client1 = new HttpClient();
                 var url = "https://covid19languagesevice.cognitiveservices.azure.com/language/:query-knowledgebases?projectName=LeaveRequest&api-version=2021-10-01&deploymentName=production";
                 var headers = new Dictionary<string, string>();
                 //headers.Add("Content-Type", "application/json;charset=UTF-8");
                 headers.Add("Ocp-Apim-Subscription-Key", "876ee52635ef4e87b553bdd6881ee599");
                 var request = new HttpRequestMessage(HttpMethod.Post, url)
                 {
                     Content = new StringContent(body, Encoding.UTF8, "application/json")
                 };
                 //request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                 foreach (var header in headers)
                 {
                     request.Headers.Add(header.Key, header.Value);
                 }
                 var response = await client.SendAsync(request);*/
            }

            return await stepContext.NextAsync(null, cancellationToken);
        }

        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.ReplaceDialogAsync(InitialDialogId, null, cancellationToken);
        }        
    }
}
