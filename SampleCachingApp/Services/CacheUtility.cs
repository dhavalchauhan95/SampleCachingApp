using System.Text.RegularExpressions;
using System.Text;
using System.Security.Cryptography;
using Microsoft.Extensions.Caching.Distributed;

namespace SampleCachingApp.Services
{
    public static class CacheHelper
    { 
        //Get the hash value for setting it as a key along with query parameters for caching.
        public static string GetCombinedMethodCodeHash(Dictionary<string, List<string>> fileMethodMap)
        {
            StringBuilder combinedCode = new StringBuilder();

            foreach (var entry in fileMethodMap)
            {
                string filePath = entry.Key;
                List<string> methodNames = entry.Value;

                string fileContent = File.ReadAllText(filePath);
                foreach (string methodName in methodNames)
                {
                    string methodPattern = $@"(\[.*\]\s*)?(public|private|protected|internal|static|\s)*\b({methodName})\b\s*(<[^>]*>)?\s*\([^\)]*\)[\s\S]*?{{[\s\S]*?}}";

                    Match methodMatch = Regex.Match(fileContent, methodPattern);

                    if (methodMatch.Success)
                    {
                        string normalizedCode = NormalizeCode(methodMatch.Value);
                        combinedCode.Append(normalizedCode); // Append found method code
                    }
                    else
                    {
                        throw new FileNotFoundException($"Method '{methodName}' not found in the file '{filePath}'.");
                    }
                }
            }

            using (var sha256 = SHA256.Create())
            {
                byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(combinedCode.ToString()));
                return Convert.ToBase64String(hashBytes);
            }
        }
        private static string NormalizeCode(string code)
        {
            // Remove all redundant spaces and line breaks
            return Regex.Replace(code, @"\s+", " ").Trim();
        }
    }
}
