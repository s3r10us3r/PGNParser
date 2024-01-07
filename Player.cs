using System;
using System.Collections.Generic;

namespace PGNParser
{
    class Player
    {
        public HashSet<int> pawns = new HashSet<int>();
        public HashSet<int> knights = new HashSet<int>();
        public HashSet<int> bishops = new HashSet<int>();
        public HashSet<int> rooks = new HashSet<int>();
        public HashSet<int> queens = new HashSet<int>();
        public int king;
        public int pawnDirection;
        public readonly string color;

        Predicate<char> colorPredicate;

        public Player(string color, char[] board)
        {
            if (color == "white")
            {
                colorPredicate = (char c) => c >= 'A' && c <= 'Z';
                pawnDirection = 10;
            }
            else if (color == "black")
            {
                colorPredicate = (char c) => c >= 'a' && c <= 'z';
                pawnDirection = -10;
            }
            this.color = color;
            SetUpSets(board);
        }

        public void RemovePiece(char piece, int square)
        {
            piece = piece.ToString().ToUpper()[0];
            switch(piece)
            {
                case 'P':
                    pawns.Remove(square);
                    break;
                case 'R':
                    rooks.Remove(square);
                    break;
                case 'N':
                    knights.Remove(square);
                    break;
                case 'B':
                    bishops.Remove(square);
                    break;
                case 'Q':
                    queens.Remove(square);
                    break;
                default:
                    throw new ArgumentException("Invelid piece char!");
            }
        }

        private void SetUpSets(char[] board)
        {
            for (int i = 0; i < 120; i++)
            {
                char c = board[i];
                if (!colorPredicate(c))
                    continue;
                c = c.ToString().ToLower()[0];
                switch (c)
                {
                    case 'p':
                        pawns.Add(i);
                        break;
                    case 'r':
                        rooks.Add(i);
                        break;
                    case 'n':
                        knights.Add(i);
                        break;
                    case 'b':
                        bishops.Add(i);
                        break;
                    case 'q':
                        queens.Add(i);
                        break;
                    case 'k':
                        king = i;
                        break;
                }
            }
        }
    }
}
