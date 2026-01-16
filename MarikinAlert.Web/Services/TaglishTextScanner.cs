namespace MarikinAlert.Web.Services
{
    using MarikinAlert.Web.Models;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Context-Aware Taglish Disaster Triage Scanner
    /// Adds false positive filtering and context analysis
    /// </summary>
    public class TaglishTextScanner : ITextScanner
    {
        // Category keywords (same as before)
        private readonly Dictionary<ReportCategory, List<string>> _categoryKeywords = new()
        {
            { 
                ReportCategory.Fire, 
                new List<string> { 
                    "sunog", "nasusunog", "nasunog", "apoy", "usok", "smoke", "fire", 
                    "nag-aapoy", "umaapoy", "sinusunog", "nagliliyab"
                } 
            },
            { 
                ReportCategory.BuildingCollapse, 
                new List<string> { 
                    "naipit", "naiipit", "trapped", "gumuhong", "gumuho", "runtuhan", 
                    "guho", "gumiba", "bumagsak", "collapse", "bagsak", "building", 
                    "pader", "wall", "bubong", "roof", "lupa", "debris", "wreckage"
                } 
            },
            { 
                ReportCategory.Medical, 
                new List<string> { 
                    "dugo", "duguan", "blood", "sugat", "injured", "nasugatan",
                    "nahimatay", "hinimatay", "nawalan ng malay", "walang malay", "unconscious",
                    "medikal", "ospital", "hospital", "doctor", "nurse", "ambulansya",
                    "patay", "dead", "namatay", "buntis", "pregnant", "lola", "elderly",
                    "nahihirapan huminga", "hirap huminga", "sakit dibdib", "chest pain",
                    "convulse", "seizure", "nagsusuka", "vomit", "lagnat", "fever"
                } 
            },
            { 
                ReportCategory.Logistics, 
                new List<string> { 
                    "tubig", "water", "pagkain", "food", "kumot", "blanket", "relief",
                    "supplies", "tulong", "kailangan", "wala", "kulang", "evacuation",
                    "gamot", "medicine", "damit", "clothes"
                } 
            },
            { 
                ReportCategory.Infrastructure, 
                new List<string> { 
                    "tulay", "bridge", "daan", "road", "kalsada", "street", "sira", "damaged",
                    "lubog", "flooded", "baha", "flood", "linya", "power line", "poste"
                } 
            },
            { 
                ReportCategory.Noise, 
                new List<string> { 
                    "brownout", "kuryente", "power", "signal", "lowbat", "battery",
                    "takot", "scared", "worried", "natatakot"
                } 
            }
        };

        // NEW: False positive patterns - these indicate NON-emergencies
        private readonly Dictionary<ReportCategory, List<string>> _falsePositivePatterns = new()
        {
            {
                ReportCategory.Fire,
                new List<string>
                {
                    // Small/trivial fires
                    "posporo", "matchstick", "kandila", "candle", "lighter", "pagkain", "food",
                    "kusina", "kitchen", "nagluluto", "cooking", "niluto", "cooked",
                    // Past tense (already handled)
                    "nasunog na", "napatay na", "nawala na", "tapos na"
                }
            },
            {
                ReportCategory.BuildingCollapse,
                new List<string>
                {
                    // Small objects, not structures
                    "upuan", "chair", "mesa", "table", "ilaw", "light", "kahon", "box",
                    "laruan", "toy", "bato", "stone", "basura", "trash"
                }
            },
            {
                ReportCategory.Medical,
                new List<string>
                {
                    // Minor injuries or routine care
                    "gasgas", "scratch", "hiwa", "cut", "konti lang", "only small",
                    "maayos na", "already okay", "check-up", "checkup", "vaccine"
                }
            }
        };

        // NEW: Emergency context indicators (boosts score when present)
        private readonly List<string> _emergencyContextWords = new()
        {
            // Locations
            "bahay", "house", "building", "school", "paaralan", "mall", "hospital",
            "simbahan", "church", "market", "palengke",
            // People trapped/affected
            "tao", "people", "mga tao", "bata", "children", "matanda", "elderly",
            "pamilya", "family", "kami", "we", "ako", "me",
            // Scale indicators
            "malaki", "big", "marami", "many", "lahat", "all", "buong", "entire"
        };

        private readonly List<string> _criticalKeywords = new() 
        { 
            "naipit", "naiipit", "trapped", "sunog", "nasusunog", "fire", "apoy",
            "gumuhong", "gumuho", "collapse", "patay", "dead", "namatay",
            "dugo", "duguan", "blood", "nahimatay", "hinimatay", "walang malay"
        };

        private readonly List<string> _urgencyWords = new()
        {
            "agad", "mabilis", "emergency", "saklolo", "tulong", "help", "asap",
            "bilisan", "rush", "critical", "grabe", "serious"
        };

        private readonly Dictionary<string, string> _normalizations = new()
        {
            { "nasusunog", "sunog" },
            { "nasunog", "sunog" },
            { "nag-aapoy", "apoy" },
            { "umaapoy", "apoy" },
            { "gumuho", "guho" },
            { "gumuhong", "guho" },
            { "bumagsak", "bagsak" },
            { "hinimatay", "nahimatay" },
            { "nasugatan", "sugat" },
            { "namatay", "patay" }
        };

        public ReportCategory ScanForCategory(string rawMessage)
        {
            if (string.IsNullOrWhiteSpace(rawMessage))
                return ReportCategory.Noise;

            string normalized = NormalizeText(rawMessage);

            // Score each category with context awareness
            var categoryScores = new Dictionary<ReportCategory, double>();

            foreach (var category in _categoryKeywords)
            {
                double score = 0;
                
                // Calculate base keyword score
                foreach (var keyword in category.Value)
                {
                    if (ContainsWord(normalized, keyword))
                    {
                        score += 10;
                    }
                    else
                    {
                        double fuzzyScore = FuzzyMatchScore(normalized, keyword);
                        score += fuzzyScore;
                    }
                }

                // NEW: Apply false positive penalty
                if (_falsePositivePatterns.ContainsKey(category.Key))
                {
                    foreach (var falsePositive in _falsePositivePatterns[category.Key])
                    {
                        if (ContainsWord(normalized, falsePositive))
                        {
                            // Heavy penalty for false positives
                            score -= 15;
                        }
                    }
                }

                // NEW: Apply emergency context boost
                int contextCount = _emergencyContextWords.Count(word => 
                    ContainsWord(normalized, word));
                
                if (contextCount > 0 && score > 0)
                {
                    // Boost score if keywords + context both present
                    score += contextCount * 3;
                }

                categoryScores[category.Key] = Math.Max(0, score); // Don't go negative
            }

            var bestMatch = categoryScores.OrderByDescending(x => x.Value).FirstOrDefault();

            // Require higher threshold (8 instead of 5) to reduce false positives
            return bestMatch.Value >= 8 ? bestMatch.Key : ReportCategory.Noise;
        }

        public ReportPriority DeterminePriority(string rawMessage, ReportCategory category)
        {
            if (string.IsNullOrWhiteSpace(rawMessage))
                return ReportPriority.Low;

            // If already classified as Noise, always Low priority
            if (category == ReportCategory.Noise)
                return ReportPriority.Low;

            string normalized = NormalizeText(rawMessage);

            int urgencyScore = 0;

            // Critical keywords present
            bool hasCriticalKeyword = _criticalKeywords.Any(keyword => 
                ContainsWord(normalized, keyword));

            if (hasCriticalKeyword)
                urgencyScore += 50;

            // Urgency multiplier words
            int urgencyWordCount = _urgencyWords.Count(word => 
                ContainsWord(normalized, word));
            urgencyScore += urgencyWordCount * 10;

            // NEW: Context awareness for priority
            int emergencyContextCount = _emergencyContextWords.Count(word =>
                ContainsWord(normalized, word));
            urgencyScore += emergencyContextCount * 5;

            // NEW: Check for false positive indicators - downgrade priority
            bool hasFalsePositive = false;
            if (_falsePositivePatterns.ContainsKey(category))
            {
                hasFalsePositive = _falsePositivePatterns[category].Any(fp =>
                    ContainsWord(normalized, fp));
            }

            if (hasFalsePositive)
            {
                // Drastically reduce urgency if false positive detected
                urgencyScore = (int)(urgencyScore * 0.3);
            }

            // Category baseline (only if no false positive)
            if (!hasFalsePositive)
            {
                if (category == ReportCategory.Fire || 
                    category == ReportCategory.BuildingCollapse)
                    urgencyScore += 30;
                else if (category == ReportCategory.Medical)
                    urgencyScore += 25;
            }

            // Determine priority
            if (urgencyScore >= 50)
                return ReportPriority.Critical;
            else if (urgencyScore >= 30)
                return ReportPriority.High;
            else if (urgencyScore >= 15)
                return ReportPriority.Medium;
            else
                return ReportPriority.Low;
        }

        // ===== PRIVATE HELPER METHODS (unchanged) =====

        private string NormalizeText(string text)
        {
            string normalized = text.ToLowerInvariant().Trim();

            foreach (var norm in _normalizations)
            {
                normalized = Regex.Replace(normalized, $@"\b{norm.Key}\b", norm.Value, RegexOptions.IgnoreCase);
            }

            return normalized;
        }

        private bool ContainsWord(string text, string word)
        {
            string pattern = $@"\b{Regex.Escape(word)}\b";
            return Regex.IsMatch(text, pattern, RegexOptions.IgnoreCase);
        }

        private double FuzzyMatchScore(string text, string keyword)
        {
            var words = text.Split(new[] { ' ', ',', '.', '!', '?' }, StringSplitOptions.RemoveEmptyEntries);
            
            double bestScore = 0;
            foreach (var word in words)
            {
                int distance = LevenshteinDistance(word, keyword);
                int maxLength = Math.Max(word.Length, keyword.Length);
                
                if (maxLength > 0)
                {
                    double similarity = 1.0 - ((double)distance / maxLength);
                    
                    if (similarity > 0.7)
                    {
                        double score = similarity * 5;
                        bestScore = Math.Max(bestScore, score);
                    }
                }
            }
            
            return bestScore;
        }

        private int LevenshteinDistance(string source, string target)
        {
            if (string.IsNullOrEmpty(source))
                return string.IsNullOrEmpty(target) ? 0 : target.Length;
            
            if (string.IsNullOrEmpty(target))
                return source.Length;

            int[,] distance = new int[source.Length + 1, target.Length + 1];

            for (int i = 0; i <= source.Length; i++)
                distance[i, 0] = i;
            
            for (int j = 0; j <= target.Length; j++)
                distance[0, j] = j;

            for (int i = 1; i <= source.Length; i++)
            {
                for (int j = 1; j <= target.Length; j++)
                {
                    int cost = (target[j - 1] == source[i - 1]) ? 0 : 1;
                    
                    distance[i, j] = Math.Min(
                        Math.Min(distance[i - 1, j] + 1, distance[i, j - 1] + 1),
                        distance[i - 1, j - 1] + cost);
                }
            }

            return distance[source.Length, target.Length];
        }
    }
}