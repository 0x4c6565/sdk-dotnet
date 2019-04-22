﻿using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UKFast.API.Client.Models;
using UKFast.API.Client.Request;
using UKFast.API.Client.Response;

namespace UKFast.API.Client.Core.Tests
{

    class TestHttpClient : HttpClient
    {
        public bool Disposed { get; set; }

        public TestHttpClient()
        {
            this.BaseAddress = new System.Uri("https://test.com");
        }

        public TestHttpClient(HttpMessageHandler handler) : base(handler)
        {
            this.BaseAddress = new System.Uri("https://test.com");
        }

        protected override void Dispose(bool disposing)
        {
            this.Disposed = true;
            base.Dispose(disposing);
        }
    }

    public class TestClientConnection : ClientConnection
    {
        public TestClientConnection() : base("testkey")
        {
            this.Client = new TestHttpClient();
        }

        public TestClientConnection(HttpMessageHandler handler) : base("testkey")
        {
            this.Client = new TestHttpClient(handler);
        }

        public string ComposeUri_Exposed(ClientRequest request)
        {
            return ComposeUri(request);
        }
    }

    public delegate void RequestHandler(HttpRequestMessage request);

    public class TestHttpMessageHandler : HttpMessageHandler
    {
        public System.Net.HttpStatusCode StatusCode { get; set; }
        public string Content { get; set; }
        public RequestHandler RequestHandler { get; set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            this.RequestHandler?.Invoke(request);

            return Task.FromResult(new HttpResponseMessage()
            {
                StatusCode = this.StatusCode,
                Content = new StringContent(this.Content ?? "", Encoding.UTF8, "application/json")
            });
        }
    }

    public class TestUKFastClient : UKFastClient
    {
        public TestUKFastClient(IConnection connection) : base(connection) { }

        public TestUKFastClient(IConnection connection, ClientConfig config) : base(connection, config) { }
    }

    public class TestClientResponsePaginatedGenerator
    {
        private int _current = 0;

        public List<List<ModelBase>> Models { get; set; }

        public ClientResponse<IList<ModelBase>> GetNext(ClientRequestParameters parameters)
        {
            var response = new ClientResponse<IList<ModelBase>>()
            {
                Body = new ClientResponseBody<IList<ModelBase>>()
                {
                    Data = this.Models[_current],
                    Metadata = new ClientResponseMetadata()
                    {
                        Pagination = new ClientResponseMetadataPagination()
                        {
                            TotalPages = this.Models.Count
                        }
                    }
                }
            };

            _current++;

            return response;
        }
    }
}
