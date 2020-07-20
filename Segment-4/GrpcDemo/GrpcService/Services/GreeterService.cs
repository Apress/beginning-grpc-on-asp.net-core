using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

namespace GrpcService
{
    [Authorize]
    public class GreeterService : Greeter.GreeterBase
    {
        private readonly ILogger<GreeterService> _logger;
        public GreeterService(ILogger<GreeterService> logger)
        {
            _logger = logger;
        }

        public override Task<HelloReply> SayHello(HelloRequest request, ServerCallContext context)
        {
            return Task.FromResult(new HelloReply
            {
                Message = "Hello " + request.Name
            });
        }

        public override async Task SayManyHellos(HelloRequest request, IServerStreamWriter<HelloReply> responseStream, ServerCallContext context)
        {
            for (var i = 0; i < 10; i++)
            {
                await responseStream.WriteAsync(new HelloReply
                {
                    Message = $"Hello {request.Name} number {(i + 1)}"
                });
            }
        }

        public override async Task<HelloReply> SayHelloToLastRequest(IAsyncStreamReader<HelloRequest> requestStream, ServerCallContext context)
        {
            var name = string.Empty;

            await foreach (var request in requestStream.ReadAllAsync())
            {
                name = request.Name;
            }

            return new HelloReply
            {
                Message = $"Hello {name}"
            };
        }

        [Authorize(Roles="admin")]
        public override async Task SayHelloToEveryRequest(IAsyncStreamReader<HelloRequest> requestStream, IServerStreamWriter<HelloReply> responseStream, ServerCallContext context)
        {
            await foreach (var request in requestStream.ReadAllAsync())
            {
                await responseStream.WriteAsync(new HelloReply
                {
                    Message = $"Hello {request.Name}"
                });
            }
        }
    }
}
