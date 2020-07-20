using Grpc.Core;
using Grpc.Net.Client;
using GrpcService;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace GrpcClient
{
    class Program
    {
        static async Task Main(string[] args)
        {
            try
            {
                Console.WriteLine("Please provide username.");
                var username = Console.ReadLine();

                Console.WriteLine("Please provide password.");
                var password = Console.ReadLine();

                var token = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{password}"));

                var headers = new Metadata();
                headers.Add("Authorization", $"Basic {token}");

                AppContext.SetSwitch(
                    "System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

                // The port number(5001) must match the port of the gRPC server.
                using var channel = GrpcChannel.ForAddress("http://localhost:5000");

                var client = new Greeter.GreeterClient(channel);

                Console.WriteLine("Sending unary call...");

                var reply = await client.SayHelloAsync(
                                  new HelloRequest { Name = "GreeterClient" }, headers);
                Console.WriteLine("Unary response: " + reply.Message);

                Console.WriteLine("Sending request for server stream...");

                using (var call = client.SayManyHellos(new HelloRequest { Name = "GreeterClient" }, headers))
                {
                    await foreach (var response in call.ResponseStream.ReadAllAsync())
                    {
                        Console.WriteLine("New element from response stream: " + response.Message);
                    }
                }

                Console.WriteLine("Sending client stream...");

                var listOfNames = new List<string> { "John", "James", "Freddy", "David" };

                Console.WriteLine("Names about to be sent: " + string.Join(", ", listOfNames));

                using (var call = client.SayHelloToLastRequest(headers))
                {
                    foreach (var name in listOfNames)
                    {
                        await call.RequestStream.WriteAsync(new HelloRequest { Name = name });
                    }

                    await call.RequestStream.CompleteAsync();

                    Console.WriteLine("Response from client stream: " + (await call.ResponseAsync).Message);
                }

                Console.WriteLine("Sending bi-directional call...");

                using (var call = client.SayHelloToEveryRequest(headers))
                {
                    foreach (var name in listOfNames)
                    {
                        await call.RequestStream.WriteAsync(new HelloRequest { Name = name });
                    }

                    await call.RequestStream.CompleteAsync();

                    await foreach (var response in call.ResponseStream.ReadAllAsync())
                    {
                        Console.WriteLine("Individual item from bi-directional call: " + response.Message);
                    }
                }

                Console.WriteLine("Press any key to exit...");
            }
            catch (Exception ex)
            {
                Console.Write(ex);
            }

            Console.ReadKey();
        }
    }
}
