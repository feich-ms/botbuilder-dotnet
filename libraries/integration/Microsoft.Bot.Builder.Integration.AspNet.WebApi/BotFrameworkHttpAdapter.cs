﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Connector.Authentication;

namespace Microsoft.Bot.Builder.Integration.AspNet.WebApi
{
    /// <summary>
    /// A Bot Builder Adapter implementation used to handled bot Framework HTTP requests.
    /// </summary>
    public class BotFrameworkHttpAdapter : BotFrameworkAdapter, IBotFrameworkHttpAdapter
    {
        public BotFrameworkHttpAdapter(ICredentialProvider credentialProvider = null)
            : base(credentialProvider ?? new SimpleCredentialProvider())
        {
        }

        public async Task ProcessAsync(HttpRequestMessage httpRequest, HttpResponseMessage httpResponse, IBot bot, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (httpRequest == null)
            {
                throw new ArgumentNullException(nameof(httpRequest));
            }

            if (httpResponse == null)
            {
                throw new ArgumentNullException(nameof(httpResponse));
            }

            if (bot == null)
            {
                throw new ArgumentNullException(nameof(bot));
            }

            // deserialize the incoming Activity
            var activity = await HttpHelper.ReadRequestAsync(httpRequest, cancellationToken).ConfigureAwait(false);

            // grab the auth header from the inbound http request
            var authHeader = httpRequest.Headers.Authorization?.ToString();

            // process the inbound activity with the bot
            var invokeResponse = await ProcessActivityAsync(authHeader, activity, bot.OnTurnAsync, cancellationToken).ConfigureAwait(false);

            // write the response, potentially serializing the InvokeResponse
            HttpHelper.WriteResponse(httpRequest, httpResponse, invokeResponse);
        }
    }
}
