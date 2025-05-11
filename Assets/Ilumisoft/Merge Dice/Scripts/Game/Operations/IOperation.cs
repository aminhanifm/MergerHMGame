using System.Collections;

namespace Ilumisoft.MergeDice.Operations
{
    public interface IOperation
    {
        IEnumerator Execute();
    }
}