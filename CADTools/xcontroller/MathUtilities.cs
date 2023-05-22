
namespace CADTools
{
    public class MathUtilities
    {
        public static char HexToChar(string hex)
        {
            return (char)ushort.Parse(hex, System.Globalization.NumberStyles.HexNumber);
        }
    }
}
