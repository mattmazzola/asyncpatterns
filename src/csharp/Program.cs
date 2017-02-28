using csharp.Models;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace csharp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            ExecutAsync().GetAwaiter().GetResult();
        }

        public static async Task ExecutAsync()
        {
            var number1 = await GenerateNumberAsyncTaskCompletionSource();
            var number2 = await GenerateNumberAsyncTaskRun();

            var thing1 = await GetThingContinueWith();
            var thing2 = await GetThingAsyncAwait();

            var biggerThing1 = await GetBiggerThingContinueWith();
            var biggerThing2 = await GetBiggerThingAsyncAwait();

            var totalValue = await GetTotalValue();

            var tree1 = await GetTreeContinueWith();
            var tree2 = await GetTreeAwait();

            Console.WriteLine("Press any key to exist");
            Console.ReadKey(true);
        }

        // Wrapping non-async code
        // Method 1: TaskCompletionSource
        public static Task<int> GenerateNumberAsyncTaskCompletionSource()
        {
            var tcs1 = new TaskCompletionSource<int>();
            tcs1.SetResult(SimulateSynchronousApi());
            return tcs1.Task;
        }

        // Method 2: Task.Run
        public static Task<int> GenerateNumberAsyncTaskRun()
        {
            return Task.Run(() => SimulateSynchronousApi());
        }


        // Modifying return value of async call
        // Method 1: Task.ContinueWith
        public static Task<IThing> GetThingContinueWith()
        {
            return GenerateNumberAsyncTaskRun().ContinueWith<IThing>(value => new Thing()
            {
                Value = value.Result,
                Name = "SomeName"
            });
        }

        // Method 2: Async/Await
        public static async Task<IThing> GetThingAsyncAwait()
        {
            var value = await GenerateNumberAsyncTaskRun();
            return new Thing()
            {
                Value = value,
                Name = "SomeName"
            };
        }

        // Chaining Async Operations
        // Method 1: Task.ContinueWith
        public static Task<IBiggerThing> GetBiggerThingContinueWith()
        {
            return GenerateNumberAsyncTaskRun().ContinueWith<Task<IBiggerThing>>(number =>
            {
                return GetThingAsyncAwait().ContinueWith<IBiggerThing>(thing => new BiggerThing()
                {
                    Thing = thing.Result,
                    OtherValue = number.Result
                });
            }).Unwrap();
        }

        // Method 2: async/await
        public static async Task<IBiggerThing> GetBiggerThingAsyncAwait()
        {
            var number = await GenerateNumberAsyncTaskRun();
            var thing = await GetThingAsyncAwait();
            return new BiggerThing()
            {
                Thing = thing,
                OtherValue = number
            };
        }

        // Aggregate many async tasks
        // Method 1: Task.WhenAll
        public static async Task<int> GetTotalValue()
        {
            return (await Task.WhenAll(Enumerable.Range(0, 20)
                .Select(_ => GetThingAsyncAwait())))
                .Aggregate(0, (a, b) => a += b.Value);
        }

        // Branching Async Tasks
        // Method 1: ContinueWith
        public static Task<Node> GetTreeContinueWith()
        {
            return GetRemoteNodeAsync(1, "root")
                .ContinueWith(rootRemoteNode =>
                    Task.WhenAll(rootRemoteNode.Result.RelatedItemIds.Select(applicationId =>
                        GetRemoteNodeAsync(applicationId, "application")
                            .ContinueWith(applicationRemoteNode =>
                                Task.WhenAll(applicationRemoteNode.Result.RelatedItemIds.Select(serviceId =>
                                    GetRemoteNodeAsync(serviceId, "service")
                                        .ContinueWith(serviceRemoteNode =>
                                            new Node()
                                            {
                                                Id = serviceRemoteNode.Result.Id,
                                                Type = serviceRemoteNode.Result.Type,
                                                Children = Enumerable.Empty<Node>()
                                            })
                                ))
                                .ContinueWith(services =>
                                    new Node()
                                    {
                                        Id = applicationRemoteNode.Result.Id,
                                        Type = applicationRemoteNode.Result.Type,
                                        Children = services.Result
                                    })
                            ).Unwrap()
                        ))
                        .ContinueWith(applications =>
                            new Node()
                            {
                                Id = rootRemoteNode.Result.Id,
                                Type = rootRemoteNode.Result.Type,
                                Children = applications.Result
                            })
                    ).Unwrap();
        }

        // Method 2: Await
        public static async Task<Node> GetTreeAwait()
        {
            var rootRemoteNode = await GetRemoteNodeAsync(1, "root");
            var applications = await Task.WhenAll(rootRemoteNode.RelatedItemIds.Select(async applicationId =>
            {
                var applicationRemoteNode = await GetRemoteNodeAsync(applicationId, "application");
                var services = await Task.WhenAll(applicationRemoteNode.RelatedItemIds.Select(async serviceId =>
                {
                    var serviceRemoteNode = await GetRemoteNodeAsync(serviceId, "service");
                    return new Node()
                    {
                        Id = serviceRemoteNode.Id,
                        Type = serviceRemoteNode.Type,
                        Children = Enumerable.Empty<Node>()
                    };
                }));

                return new Node()
                {
                    Id = applicationRemoteNode.Id,
                    Type = applicationRemoteNode.Type,
                    Children = services
                };
            }));

            return new Node()
            {
                Id = rootRemoteNode.Id,
                Type = rootRemoteNode.Type,
                Children = applications
            };
        }

        // Method 3: Breadth First
        public static async Task<Node> GetTreeBreadthFirst()
        {
            var root = await GetRemoteNodeAsync(1, "root");
            // { id: 3, type: "root-node", relatedItemIds: [1001,1002,1003] }

            var applications = await Task.WhenAll(root.RelatedItemIds.Select(async id => await GetRemoteNodeAsync(id, "application")));
            // [ { id: 1001, name: "application-node", relatedItemIds: [2001,2002,2003] }, { id: 1002, name: "application-node", relatedItemIds: [4001,4002,4003] } ]

            var applicationsServices = await Task.WhenAll(applications.Select(async application =>
            {
                return new
                {
                    ApplicationId = application.Id,
                    Services = (await Task.WhenAll(application.RelatedItemIds.Select(async id => await GetRemoteNodeAsync(id, "service"))))
                };
            }));
            // [ { appliationId: 2001, services: [ {}, {} ] } , { applicationId: 2002, services: [ {}, {}, {} ] } ]

            return new Node()
            {
                Id = root.Id,
                Type = root.Type,
                Children = applications.Select(application => new Node()
                {
                    Id = application.Id,
                    Type = application.Type,
                    Children = applicationsServices.First(applicationServices => applicationServices.ApplicationId == application.Id).Services.Select(service => new Node()
                    {
                        Id = service.Id,
                        Type = service.Type,
                        Children = Enumerable.Empty<Node>()
                    })
                })
            };
        }

        // Helper Functions

        public static int SimulateSynchronousApi()
        {
            Thread.Sleep(TimeSpan.FromMilliseconds(100));
            var random = new Random(DateTimeOffset.UtcNow.Millisecond);
            return random.Next(0, 100);
        }

        public static async Task<RemoteNode> GetRemoteNodeAsync(int inputId, string inputType)
        {
            await Task.Delay(TimeSpan.FromMilliseconds(100));

            var random = new Random(DateTimeOffset.UtcNow.Millisecond);
            var start = random.Next(1000, 2000);
            return new RemoteNode()
            {
                Id = inputId,
                Type = $"{inputType}-node",
                RelatedItemIds = Enumerable.Range(0, random.Next(5, 10)).Select((_, i) => start + i)
            };
        }
    }
}
