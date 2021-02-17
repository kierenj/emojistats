using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace emojistats
{
    class Program
    {
        static void Main(string[] args)
        {
            var path = @"c:\users\kiere\onedrive\documents\slack exports\18-01-2021\";

            JsonDocument usersDoc;
            using (var fs = new FileStream(path + "users.json", FileMode.Open))
            {
                usersDoc = JsonDocument.Parse(fs);
            }

            var userNames = new Dictionary<string, string>();
            foreach (var user in usersDoc.RootElement.EnumerateArray())
            {
                var id = user.GetProperty("id").GetString();
                var name = user.GetProperty("name").GetString();
                userNames[id] = name;
            }

            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            DateTime TsToDateTime(double ts)
            {
                var timespan = TimeSpan.FromSeconds(ts);
                return epoch.Add(timespan).ToUniversalTime();
            }

            var allMojis = new List<(DateTime, string, string)>();
            void RegisterEmoji(DateTime when, string userId, string name)
            {
                allMojis.Add((when, userId, name));
            }

            foreach (var channelFolder in Directory.GetDirectories(path))
            {
                var channelName = Path.GetFileName(channelFolder);
                foreach (var dayPath in Directory.GetFiles(channelFolder))
                {
                    var dateStr = Path.GetFileNameWithoutExtension(dayPath);

                    JsonDocument dayDoc;
                    using (var fs = new FileStream(dayPath, FileMode.Open))
                    {
                        dayDoc = JsonDocument.Parse(fs);
                    }
                    foreach (var message in dayDoc.RootElement.EnumerateArray())
                    {
                        var when = TsToDateTime(double.Parse(message.GetProperty("ts").GetString()));
                        if (message.TryGetProperty("reactions", out var reactions))
                        {
                            foreach (var reaction in reactions.EnumerateArray())
                            {
                                var name = reaction.GetProperty("name").GetString();
                                foreach (var user in reaction.GetProperty("users").EnumerateArray())
                                {
                                    RegisterEmoji(when, user.GetString(), name);
                                }
                            }
                        }
                        if (message.TryGetProperty("user", out var userProp))
                        {
                            var whoMsg = userProp.GetString();
                            if (message.TryGetProperty("blocks", out var blocks))
                            {
                                foreach (var block in blocks.EnumerateArray())
                                {
                                    void DigForEmojis(JsonElement block)
                                    {
                                        if (block.TryGetProperty("elements", out var elements))
                                        {
                                            foreach (var element in elements.EnumerateArray())
                                            {
                                                DigForEmojis(element);
                                                var et = element.GetProperty("type").GetString();
                                                if (et == "emoji")
                                                {
                                                    var emoji = element.GetProperty("name").GetString();
                                                    RegisterEmoji(when, whoMsg, emoji);
                                                }
                                            }
                                        }
                                    }

                                    DigForEmojis(block);
                                }
                            }
                        }
                    }
                }
            }

            var dances = new[]
            {
                "bender",
                "yay-fruit",
                "banana-man",
                "charmander_dance",
                "catjam",
                "christmas_parrot",
                "confused_parrot",
                "everythings_fine_parrot",
                "goth_parrot",
                "happygoat",
                "meow_party",
                "mj",
                "party_badger",
                "party_blob",
                "party_dino",
                "party_wizard",
                "pig_scoot",
                "pw",
                "yay-anime",
                "yay-cat",
                "yay-naruto",
                "zoidberg-dance"
            };

            Console.WriteLine(allMojis.Count);
            var top = allMojis
                .Where(m => m.Item1 > DateTime.UtcNow.AddDays(-7.0))
                .Where(m => dances.Contains(m.Item3))
                .GroupBy(m => m.Item2)
                .OrderByDescending(m => m.Count());
            foreach (var user in top)
            {
                Console.WriteLine(userNames[user.Key] + " - " + user.Count());
            }
        }
    }
}
