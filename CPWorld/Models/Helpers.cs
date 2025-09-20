namespace CpWorld.Helpers
{
    public static class Helpers
    {
        public static bool NotZeroOrNull(this int? number)
        {
            if (number != null && number != 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
