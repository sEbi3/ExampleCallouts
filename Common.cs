using System;

namespace ExampleCalloutsSRC
{
    internal static class Common
    {
        public static Random rand;
        static Common()
        {
            Common.rand = new Random();
        }
    }
}