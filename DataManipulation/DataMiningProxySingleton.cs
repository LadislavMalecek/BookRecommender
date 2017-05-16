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
    /// <summary>
    /// Represents represents the unique runnable operation
    /// </summary>
    class Operation
    {
        public string UniqueId { get; }
        public string name { get; }
        public MiningEntityType entityType { get; }
        public int methodNumber { get; }
        public SparqlEndPointMiner endpoint { get; }
        public MiningState state { get; }
        public Action<MiningState> ActionToRun { get; }
        /// <summary>
        /// Create a new unique operation with endpoint mining
        /// </summary>
        /// <param name="name">Name of the operation. Will be shown in the manage web interface</param>
        /// <param name="entityType">Type of the entity</param>
        /// <param name="methodNumber">Method number of the entity</param>
        /// <param name="endpoint">Endpoint to call</param>
        public Operation(string name, MiningEntityType entityType, int methodNumber, SparqlEndPointMiner endpoint)
        {
            this.UniqueId = Guid.NewGuid().ToString();
            this.name = name;
            this.entityType = entityType;
            this.methodNumber = methodNumber;
            this.endpoint = endpoint;
            this.state = new MiningState();
        }
        /// <summary>
        /// Creates a new unique operation with custom action
        /// </summary>
        /// <param name="name">Name of the operation</param>
        /// <param name="actionToRun">Action to execute when run</param>
        public Operation(string name, Action<MiningState> actionToRun)
        {
            this.UniqueId = Guid.NewGuid().ToString();
            this.name = name;
            this.ActionToRun = actionToRun;
            this.state = new MiningState();
        }
        /// <summary>
        /// Execute the operation
        /// </summary>
        public void Execute()
        {
            if (ActionToRun != null)
            {
                ActionToRun(state);
            }
            else
            {
                state.CurrentState = MiningStateType.Started;
                endpoint.Update(entityType, methodNumber, state);
            }
        }
    }

    /// <summary>
    /// Type of entity on which to call the mining operation
    /// </summary>
    enum MiningEntityType
    {
        Books, Authors, Genres, Characters
    }
    /// <summary>
    /// Multi thread safe queue class that notifies objects via assigned synch lock
    /// You can use synch lock to receive notification when item is added to the queue
    /// </summary>
    class OperationsQueue
    {
        private ConcurrentQueue<Operation> PendingOperations = new ConcurrentQueue<Operation>();

        private readonly object synchLock;
        public Operation LastOperationDequeued = null;

        public OperationsQueue(object synchLock)
        {
            this.synchLock = synchLock;
        }
        /// <summary>
        /// Add operation to the queue
        /// </summary>
        /// <param name="operation">Operation to be added</param>
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
        /// <summary>
        /// Removes operation from the queue if present.
        /// The multithreading queue does not support removing elements, so we will dequeue all and
        /// then reinsert those we don't want to remove.
        /// </summary>
        /// <param name="operation">Operation to remove</param>
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
        /// <summary>
        /// Remove all operations from the queue
        /// </summary>
        public void RemoveAll()
        {
            lock (synchLock)
            {
                Enumerable.Range(0, PendingOperations.Count).ToList().ForEach((x) =>
                {
                    Operation op;
                    PendingOperations.TryDequeue(out op);
                    op.state.CurrentState = MiningStateType.NotRunning;
                });
            }
        }
        /// <summary>
        /// Dequeue a single operation
        /// </summary>
        /// <returns>Operation that is first on the queue</returns>
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
        /// <summary>
        /// Gets the count of the elements inside queue
        /// </summary>
        /// <returns>How many elements is present inside the queue</returns>
        public int Count()
        {
            return PendingOperations.Count();
        }
    }

    /// <summary>
    /// This class proccesses tasks from assigned queue, 
    /// it knows about items added to the queue through
    /// synch lock that is the same as for queue class
    /// </summary>
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
        /// <summary>
        /// Start a new work thread
        /// </summary>
        public void Start()
        {
            new TaskFactory().StartNew(() => this.Work()
                                            , TaskCreationOptions.LongRunning);
        }
        /// <summary>
        /// Stops a thread, not yet implemented
        /// </summary>
        public void Stop()
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// This instance is used as a Proxy Singleton for use when mining, we want to have
    /// only one instance. It will be created automatically at the first run, then multiple web clients(admins)
    /// can view or manage mining operations. All Mining entities, such is Wikidata miner and Sparql endpoint miner
    /// supports using this entity as a logging mechanism. You can use it too, just pass down the mining state when designing a new operation.
    /// If a new endpoint or mining operation is implemented, it needs to be registered here for it to be accessable from the web.
    /// Just create a new operation inside the constructor of this method as shown bellow.
    /// </summary>
    sealed class DataMiningProxySingleton
    {
        SparqlEndPointMiner wikiDataEndpoint;

        public ReadOnlyCollection<Operation> Operations { get; private set; }
        OperationsQueue OperationsQueue;
        MiningWorker MiningWorker;

        /// <summary>
        /// Registers all operations.
        /// </summary>
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
            new Operation("Books - Remove duplicates", MiningEntityType.Books, 9, wikiDataEndpoint),

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
            new Operation("Genres - Books relations", MiningEntityType.Genres, 1, wikiDataEndpoint),
            
            new Operation("WikiPages - Download", (state) => new WikiPedia.WikiPageTagMiner().UpdateTags(0, state)),
            new Operation("WikiPages - Calculate ratings", (state) => new WikiPedia.WikiPageTagMiner().UpdateTags(1, state))
            
            }.AsReadOnly();
        }

        /// <summary>
        /// Add all operations to the queue
        /// </summary>
        public void MineAll()
        {
            Operations.ToList().ForEach(o => AddForProccessing(o.UniqueId));
        }
        /// <summary>
        /// Removes all operations from the queue
        /// </summary>
        public void RemoveAll()
        {
            OperationsQueue.RemoveAll();
        }

        /// <summary>
        /// Adds a single operation to the queue
        /// </summary>
        /// <param name="id">id of the operation</param>
        public void AddForProccessing(string id)
        {
            var operation = Operations.Where(o => o.UniqueId == id).FirstOrDefault();
            if (operation != null)
            {
                OperationsQueue.Add(operation);
            }
        }
        /// <summary>
        /// Removes a single operation from the queue, if the operation is already running, it will not be canceled.
        /// The cancellation can be implemented if necessary.
        /// </summary>
        /// <param name="id">Id of the operation to be removed</param>
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