using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace PGNParser
{
    //this class parses the game noted in standard algebraic notation and later yields the moves in algebraic notation
    class Game
    {
        private Queue<string> moves;
        public Game(string gameString)
        {
            moves = new Queue<string>();
            //replace all \n signs with a whitespace
            //string newLinePattern = @"\r?\n";
            //string space = " ";
            
            //gameString = Regex.Replace(gameString, newLinePattern, space);
            //find a number followed by a dot which is followed by any number of arbitrary characters until you find another number followed by a dot or end of the string
            string pattern = @"(\d+\..*?)((?=\d+\.)|$)";
            MatchCollection matches = Regex.Matches(gameString, pattern);
            
            if(matches.Count < Program.MINIMAL_GAME_LENGTH)
            {
                throw new ArgumentException("Game is too short!");
            }

            for(int i = 0; i < Math.Min(Program.MOVES_TO_SAVE, matches.Count); i++)
            {
                string round = matches[i].Value;
                string[] strings = round.Split(' ');
                if(strings.Length != 4)
                {
                    throw new InvalidDataException("Invalid format!");
                }
                moves.Enqueue(strings[1]);
                moves.Enqueue(strings[2]);
            }
        }

        public string NextMove()
        {
            return moves.Dequeue();
        }

        public bool IsEmpty()
        {
            return moves.Count == 0;
        }
    }
}
