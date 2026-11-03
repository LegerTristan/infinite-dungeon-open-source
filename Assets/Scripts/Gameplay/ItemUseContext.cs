namespace LingeringHopes.Gameplay
{
    public class ItemUseContext
    {
        public SatietyManager SatietyManager { get; }

        public ItemUseContext(SatietyManager newSatietyManager)
        {
            SatietyManager = newSatietyManager;
        }
    }
}

