using System.Collections;
using System.Collections.Generic;

namespace Ilumisoft.MergeDice.Operations
{
    public class OperationQueue : IOperation
    {
        Queue<IOperation> queue;

        public OperationQueue()
        {
            queue = new Queue<IOperation>();
        }

        public void Clear()
        {
            queue.Clear();
        }

        public void Add(IOperation operation)
        {
            queue.Enqueue(operation);
        }

        public IEnumerator Execute()
        {
            foreach (var operation in queue)
            {
                yield return operation.Execute();
            }
        }
    }
}