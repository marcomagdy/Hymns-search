
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
    public int isMatch;
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
            // int distance = CalculateNeedlemanWunschScore(searchTerm, lines[i].ToLower());
            int distance = CalculateSmithWatermanScore(searchTerm, lines[i].ToLower());
            similarLines.Add(new Result { line = lines[i], score = distance });
        }
        // Select Top 5 results
        return similarLines.OrderByDescending(x => x.score).Take(10).ToList();
    }

    static int CalculateSmithWatermanScore(string needle, string haystack)
    {
        Cell[] lastRow = new Cell[haystack.Length + 1];
        Cell[] currentRow = new Cell[haystack.Length + 1];

        int maxScore = 0;

        for (int i = 1; i <= needle.Length; i++)
        {
            for (int j = 1; j <= haystack.Length; j++)
            {
                int score = 0;
                int gap = -1;
                if (needle[i - 1] == haystack[j - 1])
                {
                    currentRow[j].isMatch = 2;
                    // If the previous cell was a match, then we add a bonus 100 points to the score.
                    // This gives consecutive matches a higher score than non-consecutive matches.
                    score += 1 + lastRow[j - 1].isMatch;
                }
                else
                {
                    currentRow[j].isMatch = 0;
                }
                score = Math.Max(Math.Max(currentRow[j - 1].score + gap, lastRow[j].score + gap), lastRow[j - 1].score + score);
                if (score < 0)
                {
                    score = 0;
                }
                else if (score > maxScore)
                {
                    maxScore = score;
                }
                currentRow[j].score = score;
            }
            Cell[] temp = lastRow;
            lastRow = currentRow;
            currentRow = temp;
        }
        return maxScore;
    }

    static int CalculateNeedlemanWunschScore(string needle, string haystack)
    {
        Cell[] lastRow = new Cell[haystack.Length + 1];
        Cell[] currentRow = new Cell[haystack.Length + 1];
        lastRow[0].score = 0;
        lastRow[0].isMatch = 0;

        for (int i = 1; i <= haystack.Length; i++)
        {
            lastRow[i].score = lastRow[i - 1].score - 1;
            lastRow[i].isMatch = 0;
        }

        for (int i = 1; i <= needle.Length; i++)
        {
            currentRow[0].score = lastRow[0].score - 1;
            currentRow[0].isMatch = 0;
            for (int j = 1; j <= haystack.Length; j++)
            {
                int score = 0;
                int gap = -1;
                if (needle[i - 1] == haystack[j - 1])
                {
                    currentRow[j].isMatch = 100;
                    score = 10;
                    // If the previous cell was a match, then we add a bonus 100 points to the score.
                    // This gives consecutive matches a higher score than non-consecutive matches.
                    score += lastRow[j - 1].isMatch;
                }
                else
                {
                    currentRow[j].isMatch = 0;
                    // Penalize new gaps more than single linear gaps.
                    // We determine "new" gaps by checking if the previous cell was a match.
                    if (lastRow[j - 1].isMatch > 0)
                    {
                        gap = -5;
                    }
                }
                currentRow[j].score = Math.Max(Math.Max(currentRow[j - 1].score + gap, lastRow[j].score + gap), lastRow[j - 1].score + score);
            }
            Cell[] temp = lastRow;
            lastRow = currentRow;
            currentRow = temp;
        }
        return lastRow[haystack.Length].score;
    }
}

