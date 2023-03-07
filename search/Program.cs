
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

struct Result
{
    public string line;
    public int score;
}

struct Cell
{
    public int score;
    public bool isMatch;
}

class Program
{
    static void Main(string[] args)
    {
        while (true)
        {
            Console.Write("Enter a search term: ");
            string? searchTerm = Console.ReadLine();
            if (searchTerm == null || searchTerm == "")
            {
                break;
            }
            Console.Clear();

            string filename = args.Length > 1 ? args[1] : "hymns";
            using (StreamReader reader = new StreamReader(filename))
            {
                List<string> lines = new List<string>();
                string? line;
                while ((line = reader.ReadLine()) != null)
                {
                    lines.Add(line);
                }

                // Record the time it takes to find similar lines
                var watch = System.Diagnostics.Stopwatch.StartNew();
                var similarLines = FindSimilarLines(searchTerm.ToLower(), lines);
                watch.Stop();

                // Print out the similar lines
                if (similarLines.Any())
                {
                    var topScore = similarLines[0].score;
                    Console.WriteLine($"Top 5 matches for {searchTerm}:");
                    foreach (var similarLine in similarLines)
                    {
                        if (1.0 * similarLine.score/topScore > 0.7) {
                            Console.WriteLine($"{similarLine.line} --> Score: {similarLine.score}");
                        }
                    }
                    Console.WriteLine($"Found similar lines in {watch.ElapsedMilliseconds} ms.");
                }
                else
                {
                    Console.WriteLine("No similar lines found.");
                }
            }
        }
    }

    static List<Result> FindSimilarLines(string searchTerm, List<string> lines)
    {
        var similarLines = new List<Result>();
        for (int i = 0; i < lines.Count; i++)
        {
            int distance = CalculateNeedlemanWunschScore(searchTerm, lines[i].ToLower());
            similarLines.Add(new Result { line = lines[i], score = distance });
        }
        // Select Top 5 results
        return similarLines.OrderByDescending(x => x.score).Take(5).ToList();
    }

    static int CalculateNeedlemanWunschScore(string s, string t)
    {
        Cell[,] d = new Cell[s.Length + 1, t.Length + 1];
        d[0, 0].score = 0;
        for (int i = 1; i <= s.Length; i++)
        {
            d[i, 0].score = d[i - 1, 0].score - 1;
        }
        for (int j = 1; j <= t.Length; j++)
        {
            d[0, j].score = d[0, j - 1].score - 1;
        }
        for (int i = 1; i <= s.Length; i++)
        {
            for (int j = 1; j <= t.Length; j++)
            {
                int score = 0;
                int gap = -1;
                if (s[i - 1] == t[j - 1])
                {
                    d[i, j].isMatch = true;
                    score = 5;
                    // If the previous cell was a match, then we add a bonus 100 points to the score.
                    // This gives consecutive matches a higher score than non-consecutive matches.
                    if (d[i - 1, j - 1].isMatch)
                    {
                        score += 100;
                    }
                }
                else
                {
                    d[i, j].isMatch = false;
                    // Penalize new gaps more than single linear gaps.
                    // We determine "new" gaps by checking if the previous cell was a match.
                    if (d[i - 1, j - 1].isMatch)
                    {
                        gap *= 4;
                    }
                }
                d[i, j].score = Math.Max(Math.Max(d[i - 1, j].score + gap, d[i, j - 1].score + gap), d[i - 1, j - 1].score + score);
            }
        }
        return d[s.Length, t.Length].score;
    }
}

