namespace Ilumisoft.MergeDice
{
    public class SelectionValidator : IValidator
    {
        ISelection selection;

        public SelectionValidator(ISelection selection)
        {
            if (selection == null)
            {
                throw new System.ArgumentException("Parameter cannot be null", "selection");
            }

            this.selection = selection;
        }

        /// <summary>
        /// Returns true if the selection has more than two elements
        /// </summary>
        public bool IsValid
        {
            get
            {
                if (selection == null)
                {
                    throw new System.Exception("Selection cannot be null");
                }

                return this.selection.Count >= GameRules.MinSelectionSize;
            }
        }
    }
}