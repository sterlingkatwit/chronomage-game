using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;

public static class ScoreboardManager
{
    private static string filePath;
    private static Dictionary<string, float> scores;

    static ScoreboardManager()
    {
        filePath = Path.Combine(Application.dataPath, "Scripts", "scores.txt");
        scores = new Dictionary<string, float>();
        loadScores();
    }

    // Load scores from scores.txt
    public static void loadScores()
    {
        if (File.Exists(filePath))
        {
            scores.Clear();
            using (StreamReader reader = new StreamReader(filePath))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    string[] data = line.Split(',');
                    string playerName = data[0];
                    float timeAlive = float.Parse(data[1]);
                    scores.Add(playerName, timeAlive);
                }
            }
        }
    }

    public static string ScoresToString(){
        string result = "Scores:\n";
        foreach (KeyValuePair<string, float> score in scores)
        {
            result += score.Key + ": " + score.Value.ToString("F2") + "\n";
        }
        return result;
    }

    //clears the scores.txt file
    public static void ClearScoresFile(){
        using (FileStream fs = File.Open(filePath, FileMode.Truncate))
        {
            // Truncate the file, effectively clearing its contents
        }
    }


    // Save scores to scores.txt
    public static void saveScores()
    {
        // sortScoresDescending();
        sortScoresAscending();
        using (StreamWriter writer = new StreamWriter(filePath))
        {
            foreach (KeyValuePair<string, float> score in scores)
            {
                writer.WriteLine(score.Key + "," + score.Value);
            }
        }
    }

    // Add score to the score dictionary
    public static void addScore(string playerName, float timeAlive)
    {
        if (!scores.ContainsKey(playerName))
        {
            scores.Add(playerName, timeAlive);
        }
        else
        {
            // Update the score if the player already exists
            scores[playerName] = timeAlive;
        }
    }

    // Remove score from score dictionary
    public static void removeScore(string playerName)
    {
        if (scores.ContainsKey(playerName))
        {
            scores.Remove(playerName);
            saveScores();
        }
    }

    // Sort scores in descending order
    public static void sortScoresDescending()
    {
        scores = scores.OrderByDescending(x => x.Value).ToDictionary(x => x.Key, x => x.Value);
    }

    public static void sortScoresAscending(){
        scores = scores.OrderBy(x => x.Value).ToDictionary(x => x.Key, x => x.Value);
    }

    //Returns new Dictionary of the top 5 scores in Ascending order
    public static Dictionary<string, float> GetTop5ScoresAscending(){
        sortScoresAscending();
        Dictionary<string, float> topScores = new Dictionary<string, float>();

        if (scores.Count == 0)
        {
            return topScores;
        }

        int count = Mathf.Min(5, scores.Count);

        // Iterate through the scores and add the top 5 to the new dictionary
        int index = 0;
        foreach (var score in scores)
        {
            topScores.Add(score.Key, score.Value);
            index++;
            if (index >= count)
            {
                break;
            }
        }

        return topScores;
    }

    //Returns new Dictionary of the top 5 scores in Ascending order
    public static Dictionary<string, float> GetTop5ScoresDescending(){
        sortScoresDescending();
        Dictionary<string, float> topScores = new Dictionary<string, float>();

        if (scores.Count == 0)
        {
            return topScores;
        }

        int count = Mathf.Min(5, scores.Count);

        // Iterate through the scores and add the top 5 to the new dictionary
        int index = 0;
        foreach (var score in scores)
        {
            topScores.Add(score.Key, score.Value);
            index++;
            if (index >= count)
            {
                break;
            }
        }

        return topScores;
    }

}