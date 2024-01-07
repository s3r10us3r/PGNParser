using System;
using System.Collections.Generic;
using System.IO;

namespace PGNParser
{
    class Program
    {
        public static int MINIMAL_GAME_LENGTH = 30;
        public static int MOVES_TO_SAVE = 15;

        public static int ERRORS = 0;
        static void Main(string[] args)
        {
            if(args.Length < 2)
            {
                ShowHowToUse();
                return;
            }
            if(args.Length >= 3)
            {
                try
                {
                    MINIMAL_GAME_LENGTH = int.Parse(args[2]);
                }
                catch(FormatException)
                {
                    ShowHowToUse();
                    return;
                }
            }
            if(args.Length >= 4)
            {
                try
                {
                    MOVES_TO_SAVE = int.Parse(args[3]);
                }
                catch(FormatException)
                {
                    ShowHowToUse();
                    return;
                }
            }
            string input = args[0];
            string output = args[1];

            List<string> uciGameList = new List<string>();
           
            if(File.Exists(input))
            {
                ReadFile(input, uciGameList);
            }
            else if (Directory.Exists(input))
            {
                string[] files = Directory.GetFiles(input, "*.pgn", SearchOption.TopDirectoryOnly);
                foreach(string file in files)
                {
                    ReadFile(file, uciGameList);
                }
            }
            else
            {
                ShowHowToUse();
                return;
            }

            Console.WriteLine($"Converted {uciGameList.Count} games, Skipped {ERRORS} due to erorrs");
            Console.WriteLine("Writing...");
            using(StreamWriter sw = new StreamWriter(output))
            {
                foreach(string uciGame in uciGameList)
                {
                    sw.WriteLine(uciGame);
                }
            }
            Console.WriteLine($"Parsed games are at {output}");
        }

        static void ShowHowToUse()
        {
            Console.WriteLine("Usage:");
            Console.WriteLine("PGNParser <input file/firectory path> <output file path> [minimal length of a game to count default is 30] [number of moves to save default is 15]");
        }

        static void ReadFile(string input, List<string> uciGameList)
        {
            Queue<Game> games = GetGames(input);

            foreach (Game game in games)
            {
                UCIBoard board = new UCIBoard();
                string uciGame = board.ProcessGame(game);
                if(uciGame == null)
                {
                    ERRORS++;
                    continue;
                }
                uciGameList.Add(uciGame);
            }
        }

        static Queue<Game> GetGames(string input)
        {
            Queue<Game> games = new Queue<Game>();
            using(StreamReader sr = new StreamReader(input))
            {
                string line = "";
                while(line != null)
                {
                    string gameString = "";
                    while((line = sr.ReadLine()) != null && line != "" && line[0] != '[' )
                    {
                        gameString += line + " ";
                    }

                    if(gameString != "")
                    {
                        try
                        {
                            Game game = new Game(gameString);
                            games.Enqueue(game);
                        }
                        catch (ArgumentException) { }//this exception is thrown when the game is too short
                    }
                }
            }
            return games;
        }
    }


}
