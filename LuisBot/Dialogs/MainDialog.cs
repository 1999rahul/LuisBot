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
using LuisBot.Helper;

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
            if (luisResult.Intents.ContainsKey("Covid19India") && luisResult.Intents["Covid19India"].Score>0.5)
            {

                return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions
                {
                    Prompt = MessageFactory.Text("India Covid19 Data : Confirmed - 49400, Active - 33565,  Recovered - 14142")
                }, cancellationToken) ;
            }
            else if (luisResult.Intents.ContainsKey("Covid19Global") && luisResult.Intents["Covid19Global"].Score > 0.5)
            {

                return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions
                {
                    Prompt = MessageFactory.Text("World Covid19 Data :Confirmed - 494700, Active - 323565,  Recovered - 214142")
                }, cancellationToken);
            }
            else
            {
                var answer = await GetCommonAnswer.GetAnswer(stepContext.Result.ToString());
                return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions
                {
                    Prompt = MessageFactory.Text(answer)
                }, cancellationToken);
            }

            return await stepContext.NextAsync(null, cancellationToken);
        }

        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.ReplaceDialogAsync(InitialDialogId, null, cancellationToken);
        }        
    }
}
