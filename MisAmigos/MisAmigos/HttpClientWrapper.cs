// <copyright file="HttpClientWrapper.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace MisAmigos.Helpers
{
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Net.Http.Formatting;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;
    using System.Web;

    public class HttpClientWrapper
    {
        public HttpClientWrapper(Uri baseUrl)
        {
            this.ClientHandler = new HttpClientHandler();
            this.Client = new HttpClient(this.ClientHandler)
            {
                BaseAddress = baseUrl
            };

        }

        static readonly HttpClient client = new HttpClient();

        private HttpClient Client { get; set; }

        private HttpClientHandler ClientHandler { get; set; }

        public async Task<T> GetAsync<T>(Uri uri)
        {
            using (HttpResponseMessage response = await this.Client.GetAsync(uri))
            {
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsAsync<T>();
                }
            }
            return await Task.FromResult<T>(default(T));
        }

    }
}