using EnsoulSharp.SDK.Utility;

namespace MightyAio.utilities
{
    public class StarterItem
    {
        public StarterItem()
        {
            CreateMenu();
            DelayAction.Add(7000, () => BuyItem());
        }

        private static void BuyItem()
        {
            
        }

        private static void CreateMenu()
        {
            
        }
    }
}