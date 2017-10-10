using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Yc.Sql.Entity.Data.Core.Framework.Helper
{
    public delegate T Method<T>(params dynamic[] methodArguments);

    public class ConcurrentProcessor : IConcurrentProcessor
    {
        private ILogger<ConcurrentProcessor> logger;

        public ConcurrentProcessor(ILogger<ConcurrentProcessor> logger)
        {
            this.logger = logger;
        }

        public Dictionary<dynamic, dynamic> ExecuteInThreads<T>(Method<T> methodToCall, Dictionary<dynamic, dynamic[]> threadClassifierCollection,
                                                            bool enableConcurrentProcessing)
        {
            var returnDataCollection = new Dictionary<dynamic, dynamic>();
            var threadDefinitions = new Dictionary<Thread, State<T>>();
            var waitHandles = new List<WaitHandle>();

            foreach (var classifier in threadClassifierCollection)
            {
                var threadState = BuildThreadState(classifier.Key, methodToCall, classifier.Value, ref returnDataCollection);
                var thread = new Thread(ExecuteSingleThread<T>);
                threadDefinitions.Add(thread, threadState);
            }

            foreach (var threadDefinition in threadDefinitions)
            {
                returnDataCollection.Add(threadDefinition.Value.ThreadKey, new object());
                threadDefinition.Key.Start(threadDefinition.Value);
                waitHandles.Add(threadDefinition.Value.ThreadResetEvent);

                if (!enableConcurrentProcessing)
                    WaitHandle.WaitAll(new WaitHandle[] { threadDefinition.Value.ThreadResetEvent });
            }

            foreach (var waitHandle in waitHandles)
            {
                waitHandle.WaitOne();
            }

            return returnDataCollection;
        }

        private void ExecuteSingleThread<T>(dynamic threadState)
        {
            var resetEvent = ((State<T>)threadState).ThreadResetEvent;
            var methodToCall = ((State<T>)threadState).MethodToCallInThreads;
            var methodArguments = ((State<T>)threadState).MethodArguments;
            var threadKey = ((State<T>)threadState).ThreadKey;

            try
            {
                ((State<T>)threadState).ReturnDataCollection[threadKey] = methodToCall.Invoke(methodArguments);
            }
            catch (Exception exception)
            {
                logger.LogError($"Thread failed: Thread Key-{threadKey}, Method Name-{methodToCall.Method}, Exception-{exception}", exception);
            }
            finally
            {
                resetEvent.Set();
            }
        }

        private static State<T> BuildThreadState<T>(dynamic threadKey, Method<T> methodToCall, dynamic[] methodArguments,
                                                ref Dictionary<dynamic, dynamic> returnDataCollection)
        {
            var threadState = new State<T>
            {
                ThreadResetEvent = new ManualResetEvent(false),
                MethodArguments = methodArguments,
                ThreadKey = threadKey,
                MethodToCallInThreads = methodToCall,
                ReturnDataCollection = returnDataCollection
            };

            return threadState;
        }

        private class State<T>
        {
            public dynamic[] MethodArguments;
            public dynamic ThreadKey;
            public Method<T> MethodToCallInThreads;
            public Dictionary<dynamic, dynamic> ReturnDataCollection;
            public ManualResetEvent ThreadResetEvent;
        }
    }

    public interface IConcurrentProcessor
    {
        Dictionary<dynamic, dynamic> ExecuteInThreads<T>(Method<T> methodToCall, Dictionary<object, dynamic[]> threadClassifierCollection, bool enableConcurrentProcessing = true);
    }
}
