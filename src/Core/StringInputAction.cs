namespace GameFramework
{
    public class StringInputAction : BaseAction
    {
        private readonly string _actionName;

        public StringInputAction(string actionName)
        {
            if (string.IsNullOrWhiteSpace(actionName))
            {
                throw new System.ArgumentException("Action name cannot be null or whitespace.", nameof(actionName));
            }
            _actionName = actionName;
        }

        public override string Name => _actionName;
    }
}
