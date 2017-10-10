using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Yc.Sql.Entity.Data.Core.Framework.Helper
{
    public delegate object Method(List<object> methodArguments);

    public class ConcurrentProcessor : IConcurrentProcessor
    {
        private ILogger<ConcurrentProcessor> logger;

        public ConcurrentProcessor(ILogger<ConcurrentProcessor> logger)
        {
            this.logger = logger;
        }

        public Dictionary<object, object> ExecuteInThreads(Method methodToCall, Dictionary<object, List<object>> threadClassifierCollection,
                                                            bool enableConcurrentProcessing = false)
        {
            var returnDataCollection = new Dictionary<object, object>();
            var threadDefinitions = new Dictionary<Thread, State>();
            var waitHandles = new List<WaitHandle>();

            foreach (var classifier in threadClassifierCollection)
            {
                var threadState = BuildThreadState(classifier.Key, methodToCall, classifier.Value, ref returnDataCollection);
                var thread = new Thread(ExecuteSingleThread);
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

        private void ExecuteSingleThread(object threadState)
        {
            var resetEvent = ((State)threadState).ThreadResetEvent;
            var methodToCall = ((State)threadState).MethodToCallInThreads;
            var methodArguments = ((State)threadState).MethodArguments;
            var threadKey = ((State)threadState).ThreadKey;

            try
            {
                ((State)threadState).ReturnDataCollection[threadKey] = methodToCall.Invoke(methodArguments);
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

        private static State BuildThreadState(object threadKey, Method methodToCall, List<object> methodArguments,
                                                ref Dictionary<object, object> returnDataCollection)
        {
            var threadState = new State
            {
                ThreadResetEvent = new ManualResetEvent(false),
                MethodArguments = methodArguments,
                ThreadKey = threadKey,
                MethodToCallInThreads = methodToCall,
                ReturnDataCollection = returnDataCollection
            };

            return threadState;
        }

        private class State
        {
            public List<object> MethodArguments;
            public object ThreadKey;
            public Method MethodToCallInThreads;
            public Dictionary<object, object> ReturnDataCollection;
            public ManualResetEvent ThreadResetEvent;
        }
    }

    public interface IConcurrentProcessor
    {
        Dictionary<object, object> ExecuteInThreads(Method methodToCall, Dictionary<object, List<object>> threadClassifierCollection, bool enableConcurrentProcessing = false);
    }
}
