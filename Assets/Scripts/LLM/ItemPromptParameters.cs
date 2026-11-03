namespace LingeringHopes.LargeLanguageModel
{
    public class ItemPromptParameters : PromptParameters
    {
        public readonly string ItemName = null;

        public ItemPromptParameters(string itemName)
        {
            ItemName = itemName;
        }
    }
}

