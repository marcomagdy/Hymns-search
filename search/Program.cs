
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

struct Result {
    public string line;
    public int score;
}

struct Cell {
    public int score;
    public bool isMatch;
}

class Program {
    static void Main(string[] args) {
        // Prompt user for input
        Console.Write("Enter a search term: ");
        string searchTerm = Console.ReadLine();

        // Open file and read each line
        using (StreamReader reader = new StreamReader("hymns")) {
            List<string> lines = new List<string>();
            string line;
            while ((line = reader.ReadLine()) != null) {
                lines.Add(line);
            }

            // Find lines that contain a word similar to the search term
            var similarLines = FindSimilarLines(searchTerm.ToLower(), lines);

            // Print out the similar lines
            if (similarLines.Any()) {
                Console.WriteLine($"Found {similarLines.Count} similar lines:");
                foreach (var similarLine in similarLines) {
                    Console.WriteLine(similarLine);
                }
            } else {
                Console.WriteLine("No similar lines found.");
            }
        }
    }

    static List<string> FindSimilarLines(string searchTerm, List<string> lines) {
        int maxDistance = 3;
        var similarLines = new List<string>();
        for (int i = 0; i < lines.Count; i++) {
            var words = lines[i].Split(',');
            foreach (var word in words) {
                int distance = CalculateLevenshteinDistance(searchTerm, word.Trim().ToLower());
                //int distance = CalculateNeedlemanWunschDistance(searchTerm, word.Trim().ToLower());
                
                if (distance <= maxDistance) {
                    similarLines.Add(lines[i]);
                    break;
                }
            }
        }
        return similarLines;
    }

    static int CalculateLevenshteinDistance(string s, string t) {
        int[,] d = new int[s.Length + 1, t.Length + 1];
        for (int i = 0; i <= s.Length; i++) {
            d[i, 0] = i;
        }
        for (int j = 0; j <= t.Length; j++) {
            d[0, j] = j;
        }
        for (int j = 1; j <= t.Length; j++) {
            for (int i = 1; i <= s.Length; i++) {
                int cost = (s[i - 1] == t[j - 1]) ? 0 : 1;
                d[i, j] = Math.Min(Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1), d[i - 1, j - 1] + cost);
            }
        }
        return d[s.Length, t.Length];
    }

    static int CalculateNeedlemanWunschDistance(string s, string t) {
        Cell[,] d = new Cell[s.Length + 1, t.Length + 1];
        d[0, 0].score = 0;
        for (int i = 1; i <= s.Length; i++) {
            d[i, 0].score = d[i - 1, 0].score - 1;
        }
        for (int j = 1; j <= t.Length; j++) {
            d[0, j].score = d[0, j - 1].score - 1;
        }
        for (int j = 1; j <= t.Length; j++) {
            for (int i = 1; i <= s.Length; i++) {
                int score = 0;
                int gap = -1;
                if (s[i - 1] == t[j - 1]) {
                    d[i, j].isMatch = true;
                    score = 5;
                }
                else {
                    d[i, j].isMatch = false;
                }
                // If the previous cell is a match, then we add a bonus 4x score.
                // This gives consecutive matches a higher score than non-consecutive matches.
                // If the previous cell is a match, but the current one is not, then we penalize that by multiplying the gap by 4.
                if (d[i - 1, j - 1].isMatch) {
                    score *= 3;
                    gap *= 4;
                }
                d[i, j].score = Math.Max(Math.Max(d[i - 1, j].score + gap, d[i, j - 1].score + gap), d[i - 1, j - 1].score + score);
            }
        }
        return d[s.Length, t.Length].score;
    }
}

