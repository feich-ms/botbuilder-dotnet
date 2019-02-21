using System;
using System.Linq;
using AdaptiveCards;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.AI.LanguageGeneration
{
    public class LGActivityGenerator
    {
        private TemplateEngine templateEngine;

        public LGActivityGenerator(TemplateEngine templateEngine)
        {
            this.templateEngine = templateEngine;
        }

        public Activity GenerateActivity(string textAndSpeakTemplateId, object scope, string textAndSpeakSepartor = "||")
        {
            var value = this.templateEngine.EvaluateTemplate(textAndSpeakTemplateId, scope);
            var valueList = value.Split(textAndSpeakSepartor);
            var activity = new Activity();
            if (valueList.Length == 1)
            {
                activity.Text = valueList[0];
                activity.Speak = valueList[0];
            }
            else if (valueList.Length == 2)
            {
                activity.Text = valueList[0].Last().Equals(' ') ? valueList[0].Remove(valueList[0].Length - 1) : valueList[0];
                activity.Speak = valueList[1].TrimStart();
            }
            else
            {
                throw new Exception(string.Format("The format of LG template {0} is wrong.", textAndSpeakTemplateId));
            }

            return activity;
        }

        public Activity GenerateAdaptiveCardActivity(string adaptiveCardTemplate, object cardScope, string textAndSpeakTemplateId = null, string textAndSpeakScope = null, string textAndSpeakSepartor = "||")
        {
            Activity activity;
            if (!string.IsNullOrEmpty(textAndSpeakTemplateId))
            {
                activity = this.GenerateActivity(textAndSpeakTemplateId, textAndSpeakScope, textAndSpeakSepartor);
            }
            else
            {
                activity = new Activity();
            }

            var cardValue = this.templateEngine.Evaluate(adaptiveCardTemplate, cardScope);
            var card = AdaptiveCard.FromJson(cardValue).Card;
            var cardObj = JsonConvert.DeserializeObject(JsonConvert.SerializeObject(card));
            var attachment = new Attachment(AdaptiveCard.ContentType, content: cardObj);
            activity.Attachments.Add(attachment);
            return activity;
        }

        public Activity GenerateNonAdaptiveCardActivity(string nonAdaptiveCardTemplate, object cardScope, string textAndSpeakTemplateId = null, string textAndSpeakScope = null, string textAndSpeakSepartor = "||")
        {
            Activity activity;
            if (!string.IsNullOrEmpty(textAndSpeakTemplateId))
            {
                activity = this.GenerateActivity(textAndSpeakTemplateId, textAndSpeakScope, textAndSpeakSepartor);
            }
            else
            {
                activity = new Activity();
            }

            var cardValue = this.templateEngine.Evaluate(nonAdaptiveCardTemplate, cardScope);
            var attachment = GenerateNonAdaptiveCard(cardValue);
            activity.Attachments.Add(attachment);
            return activity;
        }

        private Attachment GenerateNonAdaptiveCard(string card)
        {
            card = card.Substring(1, card.Length - 2);
            var splits = card.Split('\n');
            var lines = splits.OfType<string>().ToList();
            var cardType = lines[0].Trim();
            lines.RemoveAt(0);
            var cardObj = new JObject();
            foreach (var line in lines)
            {
                var start = line.IndexOf('=');
                var property = line.Substring(0, start).Trim().ToLower();
                var value = line.Substring(start + 1).Trim();
                switch (property)
                {
                    case "title":
                    case "subtitle":
                    case "text":
                    case "aspect":
                    case "value":
                    case "connectionName":
                        cardObj[property] = value;
                        break;
                    case "image":
                        object urlObj = new { url = value};
                        cardObj.Add(property, (JToken)urlObj);
                        break;
                    case "images":
                        if (cardObj[property] == null)
                        {
                            cardObj[property] = new JArray();
                        }
                        urlObj = new { url = value };
                        cardObj[property].AddAfterSelf(urlObj);
                        break;
                    case "media":
                        if (cardObj[property] == null)
                        {
                            cardObj[property] = new JArray();
                        }
                        var mediaObj = new { url = value };
                        cardObj[property].AddAfterSelf(mediaObj);
                        break;
                    case "buttons":
                        if (cardObj[property] == null)
                        {
                            cardObj[property] = new JArray();
                        }
                        foreach (var button in value.Split('|'))
                        {
                            var buttonObj = new { title = button.Trim(), type = "imBack", value = button.Trim() };
                            cardObj[property].AddAfterSelf(buttonObj);
                        }
                        break;
                    case "autostart":
                    case "sharable":
                    case "autoloop":
                        cardObj[property] = value.ToLower() == "true";
                        break;
                    case "":
                        break;
                    default:
                        System.Diagnostics.Debug.WriteLine(string.Format("Skipping unknown card property {0}", property));
                        break;
                }
            }

            // ToDo: generate Card Type
            Attachment attachment;
            switch (cardType)
            {
                case "HeroCard":
                    attachment = new Attachment(HeroCard.ContentType, content: cardObj);
                    break;
                case "ThumbnailCard":
                    attachment = new Attachment(ThumbnailCard.ContentType, content: cardObj);
                    break;
                case "AudioCard":
                    attachment = new Attachment(AudioCard.ContentType, content: cardObj);
                    break;
                case "VideoCard":
                    attachment = new Attachment(VideoCard.ContentType, content: cardObj);
                    break;
                case "AnimationCard":
                    attachment = new Attachment(AnimationCard.ContentType, content: cardObj);
                    break;
                case "MediaCard":
                    attachment = new Attachment(MediaCard.ContentType, content: cardObj);
                    break;
                case "SigninCard":
                    attachment = new Attachment(SigninCard.ContentType, content: cardObj);
                    break;
                case "OauthCard":
                    attachment = new Attachment(OAuthCard.ContentType, content: cardObj);
                    break;
                default:
                    System.Diagnostics.Debug.WriteLine(string.Format("Card type {0} is not support!", cardType));
                    attachment = new Attachment();
                    break;
            }

            return attachment;
        }
    }
}
