using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace emojistats
{
    partial class Program
    {
        static void Main2(string[] args)
        {
            var path = @"C:\Users\kiere\AppData\Local\Temp\Red River Slack export Mar 1 2019 - Mar 19 2021 x\";

            JsonDocument usersDoc;

            using (var fs = new FileStream(path + "users.json", FileMode.Open))
            {
                usersDoc = JsonDocument.Parse(fs);
            }

            var userDicts = new Dictionary<string, Dictionary<string, int>>();
            var userNames = new Dictionary<string, string>();
            foreach (var user in usersDoc.RootElement.EnumerateArray())
            {
                var id = user.GetProperty("id").GetString();
                var name = user.GetProperty("name").GetString();
                userNames[id] = name;
                userDicts[id] = new Dictionary<string, int>();
            }
            userDicts["kierenj"] = new Dictionary<string, int>();

            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            DateTime TsToDateTime(double ts)
            {
                var timespan = TimeSpan.FromSeconds(ts);
                return epoch.Add(timespan).ToUniversalTime();
            }

var common = new string[] {
    "the",
    "of",
    "to",
    "and",
    "a",
    "in",
    "is",
    "it",
    "you",
    "that",
    "he",
    "was",
    "for",
    "on",
    "are",
    "with",
    "as",
    "i",
    "his",
    "they",
    "be",
    "at",
    "one",
    "have",
    "this",
    "from",
    "or",
    "had",
    "by",
    "not",
    "word",
    "but",
    "what",
    "some",
    "we",
    "can",
    "out",
    "other",
    "were",
    "all",
    "there",
    "when",
    "up",
    "use",
    "your",
    "how",
    "said",
    "an",
    "each",
    "she",
    "which",
    "do",
    "their",
    "time",
    "if",
    "will",
    "way",
    "about",
    "many",
    "then",
    "them",
    "write",
    "would",
    "like",
    "so",
    "need",
    "think"
};
            var wordsRegex = new Regex("(\\w|')+", RegexOptions.Compiled);

            foreach (var channelFolder in Directory.GetDirectories(path))
            {
                var channelName = Path.GetFileName(channelFolder);
                Console.WriteLine(channelName);
                if (channelName == "koalatea_teas" || channelName == "tea-bot" || channelName == "tea-bot-testing") continue;
                foreach (var dayPath in Directory.GetFiles(channelFolder))
                {
                    var dateStr = Path.GetFileNameWithoutExtension(dayPath);

                    JsonDocument dayDoc;
                    using (var fs = new FileStream(dayPath, FileMode.Open))
                    {
                        try{
                        dayDoc = JsonDocument.Parse(fs);
                        }catch (Exception ex)
                        {
                            Console.WriteLine("failed parsing " + dayPath);
                            continue;
                        }
                    }
                    foreach (var message in dayDoc.RootElement.EnumerateArray())
                    {
                        var when = TsToDateTime(double.Parse(message.GetProperty("ts").GetString()));
                        if (message.TryGetProperty("user", out var userProp))
                        {
                            var whoMsg = "kierenj";//userProp.GetString();
                            if (!userDicts.ContainsKey(whoMsg)) continue;
                            var userDict = userDicts[whoMsg];

                            // here!
                            var text = message.GetProperty("text").ToString().ToLower();
                            if (text.Contains("has joined the channel")) continue;
                            if (text.Contains("skin-tone")) continue;
                            if (text.Contains("at System.")) continue;
                            if (text.Contains("https://")) continue;
                            var words = wordsRegex.Matches(text).Select(m => m.Value).ToArray();
                            for (int i = 3; i <= 7; i++)
                            {
                                for (int j = 0; j <= words.Length - i; j++)
                                {
                                    var arr = words.Skip(j).Take(i).ToArray();
                                    var entry = string.Join(" ", arr);
                                    var val = i * 10;
                                    foreach (var word in arr)
                                    {
                                        if (common.Contains(word))
                                            val -= 9;
                                    }
                                    if (!userDict.ContainsKey(entry)) { userDict[entry] = val; } else { userDict[entry] = userDict[entry] + val; }
                                }
                            }
                        }
                    }
                }
            }

    foreach (var top in userDicts["kierenj"].OrderByDescending(d => d.Value).Select(d => d.Key).Take(20))
    {
        Console.WriteLine(top);
    }
            foreach (var user in userDicts)
            {
                if (userDicts[user.Key].Count() < 100) continue;
                Console.WriteLine(userNames[user.Key]);
                Console.WriteLine("  " + userDicts[user.Key].OrderByDescending(d => d.Value).Select(d => d.Key).FirstOrDefault());
            }

        }
    }
}
