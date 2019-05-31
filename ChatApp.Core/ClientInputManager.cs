using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace ChatApp.Core
{
    public static class ClientInputManager
    {
        public enum InputCommand
        {
            Exit
        }

        private static readonly Dictionary<string, InputCommand> _possibleClientActions = new Dictionary<string, InputCommand>
        {
            { "/exit", InputCommand.Exit }
        };

        private static readonly Regex _clientCommandRegex = new Regex(@"(\/\S*)", RegexOptions.Compiled);

        public static IEnumerable<InputCommand> Process(string input)
        {
            MatchCollection matches;
            if ((matches = _clientCommandRegex.Matches(input)).Count <= 0) yield break;

            foreach (Match match in matches)
            {
                if (!_possibleClientActions.TryGetValue(match.Value, out var command)) continue;

                yield return command;
            }
        }

        public static string GetInput(string prompt = null, bool hidden = false)
        {
            Console.WriteLine();
            var inputBuilder = new StringBuilder();
            if (!string.IsNullOrEmpty(prompt))
            {
                Console.Write(prompt);
            }

            while (true)
            {
                var keyInfo = Console.ReadKey(true);
                if (keyInfo.Key == ConsoleKey.Backspace)
                {
                    if (Console.CursorLeft <= prompt?.Length)
                    {
                        continue;
                    }

                    inputBuilder.Remove(inputBuilder.Length - 1, 1);
                    Console.Write("\b \b");
                }
                else if (keyInfo.Key == ConsoleKey.Enter)
                {
                    if (inputBuilder.Length <= 0)
                    {
                        continue;
                    }

                    break;
                }
                else
                {
                    Console.Write(hidden ? '*' : keyInfo.KeyChar);
                    inputBuilder.Append(keyInfo.KeyChar);
                }
            }

            return inputBuilder.ToString();
        }
    }
}