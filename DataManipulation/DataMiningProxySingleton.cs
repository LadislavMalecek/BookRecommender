using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using BookRecommender.Models;
using BookRecommender.Models.Database;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Collections.Concurrent;
using System.Linq;
using BookRecommender.DataManipulation.WikiData;
using System.Threading;

namespace BookRecommender.DataManipulation
{
    class Operation
    {
        public string UniqueId { get; }
        public string name { get; }
        public MiningEntityType entityType { get; }
        public int methodNumber { get; }
        public SparqlEndPointMiner endpoint { get; }
        public MiningState state { get; }
        public Operation(string name, MiningEntityType entityType, int methodNumber, SparqlEndPointMiner endpoint)
        {
            this.UniqueId = Guid.NewGuid().ToString();
            this.name = name;
            this.entityType = entityType;
            this.methodNumber = methodNumber;
            this.endpoint = endpoint;
            this.state = new MiningState();
        }
        public void Execute()
        {
            state.CurrentState = MiningStateType.Started;
            endpoint.Update(entityType, methodNumber, state);
        }
    }
    enum MiningEntityType
    {
        Books, Authors, Genres, Characters
    }

    // multi thread safe queue class that notifies objects via assigned synch lock
    // you can use synch lock to recieve notification when item is added to the queue
    class OperationsQueue
    {
        private ConcurrentQueue<Operation> PendingOperations = new ConcurrentQueue<Operation>();

        private readonly object synchLock;
        public Operation LastOperationDequeued = null;

        public OperationsQueue(object synchLock)
        {
            this.synchLock = synchLock;
        }
        public void Add(Operation operation)
        {
            lock (synchLock)
            {
                if (!operation.state.IsInactive())
                {
                    return;
                }
                if (!PendingOperations.Contains(operation))
                {
                    operation.state.CurrentState = MiningStateType.Waiting;
                    PendingOperations.Enqueue(operation);
                    Monitor.Pulse(synchLock);
                }
            }
        }

        public void Remove(Operation operation)
        {
            lock (synchLock)
            {
                var operationsToKeep = PendingOperations.Where(o => o != operation).ToList();

                if (operationsToKeep.Count == PendingOperations.Count)
                {
                    return;
                }
                //remove all
                Enumerable.Range(0, PendingOperations.Count).ToList().ForEach((x) =>
                            PendingOperations.TryDequeue(out var op)
                );

                // reinsert valid items
                operationsToKeep.ForEach(o => PendingOperations.Enqueue(o));
                operation.state.CurrentState = MiningStateType.NotRunning;
            }
        }
        public void RemoveAll()
        {
            lock (synchLock)
            {
                System.Console.WriteLine("ahoj");
                Enumerable.Range(0, PendingOperations.Count).ToList().ForEach((x) =>
                {
                    System.Console.WriteLine("ahoj2");
                    Operation op;
                    PendingOperations.TryDequeue(out op);
                    op.state.CurrentState = MiningStateType.NotRunning;
                });
            }
        }
        public Operation Get()
        {
            Operation operation;
            bool successful = PendingOperations.TryDequeue(out operation);
            if (successful)
            {
                LastOperationDequeued = operation;
                return operation;
            }
            return null;
        }
        public int Count()
        {
            return PendingOperations.Count();
        }
    }


    // this class proccesses tasks from asigned queue, 
    // it knows about items added to the queue through
    // synch lock that is the same as for queue class
    class MiningWorker
    {
        private object synchLock;
        private OperationsQueue queue;
        public MiningWorker(object synchLock, OperationsQueue queue)
        {
            this.synchLock = synchLock;
            this.queue = queue;
        }
        void Work()
        {
            while (true)
            {
                Operation operation = null;
                lock (synchLock)
                {
                    if (queue.Count() == 0)
                    {
                        Monitor.Wait(synchLock);
                    }
                    operation = queue.Get();
                }
                operation.Execute();
            }
        }
        public void Start()
        {
            new TaskFactory().StartNew(() => this.Work()
                                            , TaskCreationOptions.LongRunning);
        }
        public void Stop()
        {
            throw new NotImplementedException();
        }
    }

    sealed class DataMiningProxySingleton
    {
        SparqlEndPointMiner wikiDataEndpoint;

        public ReadOnlyCollection<Operation> Operations { get; private set; }
        OperationsQueue OperationsQueue;
        MiningWorker MiningWorker;
        private DataMiningProxySingleton()
        {
            wikiDataEndpoint = new WikiDataEndpointMiner();

            var synchLock = new object();

            OperationsQueue = new OperationsQueue(synchLock);
            MiningWorker = new MiningWorker(synchLock, OperationsQueue);
            MiningWorker.Start();

            Operations = new List<Operation>(){

            new Operation("Books - URIs", MiningEntityType.Books, 0, wikiDataEndpoint),
            new Operation("Books - Labels en, cs", MiningEntityType.Books, 1, wikiDataEndpoint),
            new Operation("Books - Original names", MiningEntityType.Books, 2, wikiDataEndpoint),
            new Operation("Books - Original names 2", MiningEntityType.Books, 3, wikiDataEndpoint),
            new Operation("Books - Labels other", MiningEntityType.Books, 4, wikiDataEndpoint),
            new Operation("Books - Identifiers", MiningEntityType.Books, 5, wikiDataEndpoint),
            new Operation("Books - Images", MiningEntityType.Books, 6, wikiDataEndpoint),
            new Operation("Books - Descriptions", MiningEntityType.Books, 7, wikiDataEndpoint),
            new Operation("Books - WikiPages en", MiningEntityType.Books, 8, wikiDataEndpoint),

            new Operation("Authors - URIs", MiningEntityType.Authors, 0, wikiDataEndpoint),
            new Operation("Authors - Data 1", MiningEntityType.Authors, 1, wikiDataEndpoint),
            new Operation("Authors - Data 2", MiningEntityType.Authors, 2, wikiDataEndpoint),
            new Operation("Authors - Books relations", MiningEntityType.Authors, 3, wikiDataEndpoint),
            new Operation("Authors - Images", MiningEntityType.Authors, 4, wikiDataEndpoint),
            new Operation("Authors - Descriptions", MiningEntityType.Authors, 5, wikiDataEndpoint),
            new Operation("Authors - WikiPages en", MiningEntityType.Authors, 6, wikiDataEndpoint),

            new Operation("Characters - URIs and Labels", MiningEntityType.Characters, 0, wikiDataEndpoint),
            new Operation("Characters - Books relations", MiningEntityType.Characters, 1, wikiDataEndpoint),

            new Operation("Genres - URIs and Labels", MiningEntityType.Genres, 0, wikiDataEndpoint),
            new Operation("Genres - Books relations", MiningEntityType.Genres, 1, wikiDataEndpoint)
            }.AsReadOnly();
        }

        public void MineAll()
        {
            Operations.ToList().ForEach(o => AddForProccessing(o.UniqueId));
        }
        public void RemoveAll()
        {
            OperationsQueue.RemoveAll();
        }

        private void TickMethod()
        {
            var operation = OperationsQueue.LastOperationDequeued;
            if (operation != null)
            {
                System.Console.Write("\rMINING STATE: {0}, {1}/{2}, message:{3} --- NAME: {4}",
                                        operation.state.CurrentState,
                                        operation.state.CurrentPosition,
                                        operation.state.Count,
                                        operation.state.Message,
                                        operation.name);
            }
        }





        public void AddForProccessing(string id)
        {
            var operation = Operations.Where(o => o.UniqueId == id).FirstOrDefault();
            if (operation != null)
            {
                OperationsQueue.Add(operation);
            }
        }

        public void RemoveFromProccessing(string id)
        {
            var operation = Operations.Where(o => o.UniqueId == id).FirstOrDefault();
            if (operation != null)
            {
                OperationsQueue.Remove(operation);
            }
        }


        // methods to guarantee Singleton
        private static volatile DataMiningProxySingleton instance;
        private static object syncRoot = new Object();
        public static DataMiningProxySingleton Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                        {
                            instance = new DataMiningProxySingleton();
                        }
                    }
                }
                return instance;
            }
        }
    }
}