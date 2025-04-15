namespace Core.Random
{
    using System.Collections.Generic;
    using Random = System.Random;

    public class BattleRandom
    {
        private int seed;
        private Random random;
        public int randomCount;
    
        public BattleRandom(int seed)
        {
            this.seed = seed;
            random = new Random(seed);
        }
    
        public bool Bool()
        {
            var value = random.Next(0, 2);
            randomCount++;
            return value == 1;
        }

        public int Range(int min, int max)
        {
            var value = random.Next(min, max);
            randomCount++;
            return value;
        }
        public double Range(double min, double max)
        {
            var value = random.NextDouble();
            value = min + (max - min) * value;
            randomCount++;
            return value;
        }

        public float Range(float min, float max)
        {
            var value = random.NextDouble();
            value = min + (max - min) * value;
            randomCount++;
            return (float)value;
        }
    
        public T GetRandom<T>(List<T> list) => list[Range(0, list.Count)];
    }
}
