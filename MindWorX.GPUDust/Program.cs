using System;

namespace MindWorX.GPUDust
{
    public static class Program
    {
        public static void Main(String[] args)
        {
            using (var game = new GPUDustGame())
                game.Run();
        }
    }
}

