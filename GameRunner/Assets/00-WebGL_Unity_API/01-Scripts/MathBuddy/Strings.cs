using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace MathBuddy.Strings
{
    public static class StringExtentions
    {
        public static bool IsSubpathOf(this string subDir, string rootDir) {
            subDir = subDir.Replace('/', '\\');
            rootDir = rootDir.Replace('/', '\\');

            return subDir.StartsWith(rootDir);
        }
        
        public static string ReverseString(string input) {
            Char[] chars = input.ToCharArray();
            Array.Reverse(chars);
            
            return new string(chars);
        }

        public static string ToCsv<T>(IEnumerable<T> data) {
            string output = "";
            if (data == null || !data.Any())
                return output;
            
            foreach (var item in data) {
                output += $"{item.ToString()},";
            }

            //remove last comma value
            output = output.Substring(0, output.Length - 1);

            return output;
        }
        
        public static string ToCsv(IEnumerable<string> data)
        {
            string output = "";
            if (data == null || !data.Any())
                return output;
            
            foreach (var str in data) {
                output += $"{str},";
            }

            //remove last comma value
            output = output.Substring(0, output.Length - 1);

            return output;
        }
        
        /// <summary>
        /// Convert csv data into a list of strings, split by comma's.
        /// </summary>
        /// <param name="data">csv data string.</param>
        public static List<string> FromCsv(string data)
        {
            if (string.IsNullOrEmpty(data))
                return new List<string>();
            
            return data.Split(",").ToList();
        }

        public static string ToString<T>(this T input, NamingCastType castFrom, NamingCastType castTo)
        {
            return ToString(input.ToString(), castFrom, castTo);
        }
        
        public static string ToString(this string input, NamingCastType castFrom, NamingCastType castTo)
        {
            string[] words;
            switch (castFrom) {
                case NamingCastType.lowerCamelCase:
                case NamingCastType.UpperCamelCase:
                    List<int> wordIndexes = new List<int>();
                    wordIndexes.Add(0);
                    
                    for (int i = 1; i < input.Length; i++) {
                        if (char.IsUpper(input[i]))
                            wordIndexes.Add(i);
                    }

                    words = new string[wordIndexes.Count];
                    for (int i = 0; i < words.Length; i++) {
                        if (i < words.Length - 1) {
                            words[i] = (input.Substring(wordIndexes[i], wordIndexes[i + 1] - wordIndexes[i])).ToLower();    
                        } else
                        {
                            words[i] = (input.Substring(wordIndexes[i]));
                        }
                    }
                    
                    break;
                case NamingCastType.UPPER_CASE:
                case NamingCastType.lower_case:
                    input = input.ToLower();
                    words = input.Split('_');
                    break;
                default:
                    throw new Exception($"Type of naming for {castFrom} not found");
            }

            string output = "";
            switch (castTo) {
                case NamingCastType.lowerCamelCase:
                    output += words[0];
                    for (int i = 1; i < words.Length; i++) {
                        words[i] = char.ToUpper(words[i][0]) + words[i].Remove(0, 1);
                        output += words[i];
                    }
                    break;
                case NamingCastType.UpperCamelCase:
                    for (int i = 0; i < words.Length; i++) {
                        words[i] = char.ToUpper(words[i][0]) + words[i].Remove(0, 1);
                        output += words[i];
                    }
                    break;
                case NamingCastType.lower_case:
                    for (int i = 0; i < words.Length; i++) {
                        output += words[i] + "_";
                    }

                    output = output.Remove(output.Length - 1, 1);
                    break;
                case NamingCastType.UPPER_CASE:
                    for (int i = 0; i < words.Length; i++) {
                        output += words[i].ToUpper() + "_";
                    }

                    output = output.Remove(output.Length - 1, 1);
                    break;
                case NamingCastType.UserFormatting:
                    words[0] = char.ToUpper(words[0][0]) + words[0].Substring(1);
                    
                    for (int i = 0; i < words.Length; i++) {
                        output += words[i] + " ";
                    }
                    
                    output = output.Remove(output.Length - 1, 1);
                    break;
                default:
                    throw new Exception($"Type of naming for {castTo} not found");
            }

            return output;
        }

        public static T PickFromJson<T>(string fieldName, string json) {
            json = json.Replace(fieldName, "field");
            
            JSONProxyObject<T> obj = JsonUtility.FromJson<JSONProxyObject<T>>(json);
            
            return obj.field;
        }

        [Serializable]
        private class JSONProxyObject<T>
        {
            public T field;
        }
        
        private static readonly Regex sWhitespace = new Regex(@"\s+");
        public static string RemoveWhitespace(string input) 
        {
            return sWhitespace.Replace(input, "");
        }

        public enum NamingCastType
        {
            lowerCamelCase,
            UpperCamelCase,
            UPPER_CASE,
            lower_case,
            UserFormatting, //This is how normies write text, starting with a capital and spaces between words.
        }
    }
}