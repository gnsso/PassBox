using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PassBox
{
    public class Random
    {
        private System.Random random;
        private Dictionary<string, List<int>> randoms = new Dictionary<string, List<int>>();

        public Random()
        {
            random = new System.Random();
        }

        public int Generate(int min = 0, int max = int.MaxValue - 1, string keyForUnique = null, params int[] exex)
        {
            var r = 0;
            if (keyForUnique != null)
            {
                if (randoms.ContainsKey(keyForUnique))
                {
                    exex = exex.Concat(randoms[keyForUnique]).Distinct().ToArray();
                    if (!IsPossible(min, max, exex)) throw new ArgumentException($"Not possible to generate value");
                    else
                    {
                        r = random.Next(min, max + 1);
                        while (exex.Contains(r)) r = random.Next(min, max + 1);
                        randoms[keyForUnique].Add(r);
                        return r;
                    }
                }
                else
                {
                    r = random.Next(min, max + 1);
                    while (exex.Contains(r)) r = random.Next(min, max + 1);
                    randoms.Add(keyForUnique, new List<int> { r });
                    return r;
                }
            }
            else
            {
                r = random.Next(min, max + 1);
                while (exex.Contains(r)) r = random.Next(min, max + 1);
                return r;
            }
        }

        /// <summary>
        /// Generate random formatted password
        /// </summary>
        /// <param name="format">Password format (for digits: d, lowercase letter: l, uppercase letter: L, punctuations: p)</param>
        /// <param name="nonRepetitive">Is unique same type consequtive characters</param>
        /// <param name="validPunctuations">Valid punctuations (default: ['.'])</param>
        /// <returns></returns>
        public static string GeneratePassword(string format = "dddlllpLd", bool nonRepetitive = false, params char[] validPunctuations)
        {
            var rbld = new StringBuilder();
            var rand = new Random();
            var ltrs = new Lazy<char[]>(() => Enumerable.Range('a', 'z' - 'a' + 1).Select(s => (char)s).ToArray());
            var dgts = new Lazy<int[]>(() => Enumerable.Range(0, 10).ToArray());
            var pncs = new Lazy<char[]>(() => (validPunctuations != null && validPunctuations.Length != 0) ? validPunctuations : new[] { '.' });
            var func = new Func<char, string, char>((item, key) =>
            {
                switch (item)
                {
                    case 'd': return dgts.Value[rand.Generate(0, 9, key)].ToString()[0];
                    case 'L': return char.ToUpperInvariant(ltrs.Value[rand.Generate(0, ltrs.Value.Length - 1, key)]);
                    case 'p': return pncs.Value[rand.Generate(0, pncs.Value.Length - 1)];
                    /* case 'l': */ // default
                    default: return ltrs.Value[rand.Generate(0, ltrs.Value.Length - 1, key)];
                }
            });
            format = format.Aggregate("0x", (a, b) =>
            {
                if (b != a[a.Length - 1])
                    return $"{a}1{b}";
                return $"{a.Substring(0, a.Length - 2)}" +
                       $"{int.Parse(a[a.Length - 2].ToString()) + 1}" +
                       $"{a[a.Length - 1]}";
            }).Substring(2);
            for (int i = 0; i < format.Length; i += 2)
            {
                int c = int.Parse(format[i].ToString());
                char k = format[i + 1];
                var key = nonRepetitive && c > 1 ? Guid.NewGuid().ToString() : null;
                for (int j = 0; j < c; j++) rbld.Append(func(k, key));
            }
            return rbld.ToString();
        }

        private static bool IsPossible(int min, int max, IEnumerable<int> exex)
        {
            return !Enumerable.Range(min, max - min + 1).All(a => exex.Contains(a));
        }
    }
}
