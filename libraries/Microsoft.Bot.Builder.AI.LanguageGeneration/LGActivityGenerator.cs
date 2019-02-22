using System;
using System.Collections.Generic;
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

            var cardValue = this.templateEngine.EvaluateTemplate(adaptiveCardTemplate, cardScope);
            var card = AdaptiveCard.FromJson(cardValue).Card;
            var cardObj = JsonConvert.DeserializeObject(JsonConvert.SerializeObject(card));
            var attachment = new Attachment(AdaptiveCard.ContentType, content: cardObj);
            activity.Attachments = new List<Attachment>();
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

            var cardValue = this.templateEngine.EvaluateTemplate(nonAdaptiveCardTemplate, cardScope);
            var attachment = GenerateNonAdaptiveCard(cardValue);
            activity.Attachments = new List<Attachment>();
            activity.Attachments.Add(attachment);
            return activity;
        }

        private Attachment GenerateNonAdaptiveCard(string card)
        {
            card = card.Trim();
            card = card.Substring(1, card.Length - 2);
            var splits = card.Split("\r\n");
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
                        var urlObj = new JObject() { { "url", value } };
                        cardObj.Add(property, urlObj);
                        break;
                    case "images":
                        if (cardObj[property] == null)
                        {
                            cardObj[property] = new JArray();
                        }

                        urlObj = new JObject() { { "url", value } };
                        ((JArray)cardObj[property]).Add(urlObj);
                        break;
                    case "media":
                        if (cardObj[property] == null)
                        {
                            cardObj[property] = new JArray();
                        }
                        var mediaObj = new JObject() { { "url", value } };
                        ((JArray)cardObj[property]).Add(mediaObj);
                        break;
                    case "buttons":
                        if (cardObj[property] == null)
                        {
                            cardObj[property] = new JArray();
                        }
                        foreach (var button in value.Split('|'))
                        {
                            var buttonObj = new JObject() { { "title", button.Trim() }, { "type", "imBack" }, { "value", button.Trim() } };
                            ((JArray)cardObj[property]).Add(buttonObj);
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
                case "Herocard":
                    attachment = new Attachment(HeroCard.ContentType, content: cardObj);
                    break;
                case "Thumbnailcard":
                    attachment = new Attachment(ThumbnailCard.ContentType, content: cardObj);
                    break;
                case "Audiocard":
                    attachment = new Attachment(AudioCard.ContentType, content: cardObj);
                    break;
                case "Videocard":
                    attachment = new Attachment(VideoCard.ContentType, content: cardObj);
                    break;
                case "Animationcard":
                    attachment = new Attachment(AnimationCard.ContentType, content: cardObj);
                    break;
                case "Mediacard":
                    attachment = new Attachment(MediaCard.ContentType, content: cardObj);
                    break;
                case "Signincard":
                    attachment = new Attachment(SigninCard.ContentType, content: cardObj);
                    break;
                case "Oauthcard":
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
