using System;
using System.Security.Cryptography;

namespace Assets.Scripts
{
    public static class MazeBuilder
    {
        public static int[,] maze { get; private set; }
        private static int mazeHeight, mazeWidth;

        public static int[,] GenerateMaze(int height, int width)
        {

            maze = new int[height, width];

            // Initialize
            for (int i = 0; i < height; i++)
                for (int j = 0; j < width; j++)
                    maze[i, j] = 1;

            Random rand = new Random();
            // r for row、c for column
            // Generate random r
            int r = rand.Next(height);
            while (r % 2 == 0)
            {
                r = rand.Next(height);
            }
            // Generate random c
            int c = rand.Next(width);
            while (c % 2 == 0)
            {
                c = rand.Next(width);
            }
            // Starting cell
            maze[r, c] = 0;

            mazeHeight = height;
            mazeWidth = width;

            //　Allocate the maze with recursive method
            recursion(r, c);

            return maze;
        }

        public static void recursion(int r, int c)
        {
            // 4 random directions
            int[] directions = new int[] { 1, 2, 3, 4 };

            //directions = generateRandomDirections();
            Shuffle(directions);

            // Examine each direction
            for (int i = 0; i < directions.Length; i++)
            {

                switch (directions[i])
                {
                    case 1: // Up
                            //　Whether 2 cells up is out or not
                        if (r - 2 <= 0)
                            continue;
                        if (maze[r - 2, c] != 0)
                        {
                            maze[r - 2, c] = 0;
                            maze[r - 1, c] = 0;
                            recursion(r - 2, c);
                        }
                        break;
                    case 2: // Right
                            // Whether 2 cells to the right is out or not
                        if (c + 2 >= mazeWidth - 1)
                            continue;
                        if (maze[r, c + 2] != 0)
                        {
                            maze[r, c + 2] = 0;
                            maze[r, c + 1] = 0;
                            recursion(r, c + 2);
                        }
                        break;
                    case 3: // Down
                            // Whether 2 cells down is out or not
                        if (r + 2 >= mazeHeight - 1)
                            continue;
                        if (maze[r + 2, c] != 0)
                        {
                            maze[r + 2, c] = 0;
                            maze[r + 1, c] = 0;
                            recursion(r + 2, c);
                        }
                        break;
                    case 4: // Left
                            // Whether 2 cells to the left is out or not
                        if (c - 2 <= 0)
                            continue;
                        if (maze[r, c - 2] != 0)
                        {
                            maze[r, c - 2] = 0;
                            maze[r, c - 1] = 0;
                            recursion(r, c - 2);
                        }
                        break;
                }
            }

        }

        //public static void Shuffle<T>(T[] array)
        //{
        //    Random _random = new Random();
        //    for (int i = array.Length; i > 1; i--)
        //    {
        //        // Pick random element to swap.
        //        int j = _random.Next(i); // 0 <= j <= i-1
        //                                 // Swap.
        //        T tmp = array[j];
        //        array[j] = array[i - 1];
        //        array[i - 1] = tmp;
        //    }
        //}
        //public static void Shuffle<T>(T[] list)
        //{
        //    Random rng = new Random();
        //    int n = list.Length;
        //    while (n > 1)
        //    {
        //        n--;
        //        int k = rng.Next(n + 1);
        //        T value = list[k];
        //        list[k] = list[n];
        //        list[n] = value;
        //    }
        //}
        public static void Shuffle<T>(T[] list)
        {
            RNGCryptoServiceProvider provider = new RNGCryptoServiceProvider();
            int n = list.Length;
            while (n > 1)
            {
                byte[] box = new byte[1];
                do provider.GetBytes(box);
                while (!(box[0] < n * (Byte.MaxValue / n)));
                int k = (box[0] % n);
                n--;
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
    }

}
