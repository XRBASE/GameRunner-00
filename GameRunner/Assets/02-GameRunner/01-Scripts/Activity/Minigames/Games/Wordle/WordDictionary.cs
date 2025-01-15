using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

using MathBuddy.Strings;

namespace Cohort.GameRunner.Minigames.Wordle {
    public static class WordDictionary {
        private static string DICTIONARY_FILEPATH = "WordGame/WordListDutch";
        private static List<string> _cachedDictionary = new List<string>();
        private static int _cachedCharAmount;

        public static List<string> GetWordDictionary(int charAmount) {
            if (charAmount == _cachedCharAmount)
                return _cachedDictionary;
            _cachedCharAmount = charAmount;
            _cachedDictionary = new List<string>();
            TextAsset textAsset = Resources.Load<TextAsset>(DICTIONARY_FILEPATH);
            var data = StringExtentions.FromCsv(textAsset.text);

            foreach (var row in data) {
                string output = Regex.Replace(row, @"\s+", "");
                if (output.Length == charAmount && output.All(char.IsLetter))
                    _cachedDictionary.Add(output);
            }

            return _cachedDictionary;
        }
    }
}