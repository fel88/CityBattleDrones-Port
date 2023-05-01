using System.Globalization;

namespace CityBattleDrones_Port
{
    public static class Helpers
    {
        public static float ParseFloat(string str)
        {
            return float.Parse(str.Replace(",", "."), CultureInfo.InvariantCulture);
        }
    }
}