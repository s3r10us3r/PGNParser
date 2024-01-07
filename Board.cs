using System;
using System.Collections.Generic;
using System.Text;

namespace PGNParser
{
    //this class takes the moves in algebraic chess notation and yields UCI moves 
    class UCIBoard
    {
        //R - rook, N - knight, B - bishop, K - king, Q - queen, P - pawn, lowercase letters denote black pieces, X is a forbidden square, no piece can move there. 
        private char[] board =
        {
            'X', 'X', 'X', 'X', 'X', 'X', 'X', 'X', 'X', 'X',
            'X', 'X', 'X', 'X', 'X', 'X', 'X', 'X', 'X', 'X',

            'X', 'R', 'N', 'B', 'K', 'Q', 'B', 'N', 'R', 'X',
            'X', 'P', 'P', 'P', 'P', 'P', 'P', 'P', 'P', 'X',
            'X', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', 'X',
            'X', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', 'X',
            'X', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', 'X',
            'X', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', 'X',
            'X', 'p', 'p', 'p', 'p', 'p', 'p', 'p', 'p', 'X',
            'X', 'r', 'n', 'b', 'k', 'q', 'b', 'n', 'r', 'X',

            'X', 'X', 'X', 'X', 'X', 'X', 'X', 'X', 'X', 'X',
            'X', 'X', 'X', 'X', 'X', 'X', 'X', 'X', 'X', 'X',
        };

        private Player nowMoving;
        private Player nowNotMoving;

        private readonly HashSet<char> PIECE_SET = new HashSet<char>
        {
            'B', 'R', 'N', 'K', 'Q'
        };

        public UCIBoard()
        {
            nowMoving = new Player("white", board);
            nowNotMoving = new Player("black", board);
        }

        //returns UCI string of the first moves
        public string ProcessGame(Game game)
        {
            StringBuilder stringBuilder = new StringBuilder(); 
            while(!game.IsEmpty())
            {
                try
                {
                    string UCIstring = MakeMove(game.NextMove());
                    stringBuilder.Append(UCIstring + " ");
                }
                catch(ArgumentException)
                {
                    return null;
                }
            }

            stringBuilder.Remove(stringBuilder.Length - 1, 1);
            return stringBuilder.ToString();
        }

        //takes move in algebraic notation, procceses it on the board and returns a move string in UCI notation
        private string MakeMove(string move)
        {
            move = move.Replace("+", string.Empty);

            bool isCapture = move.Contains("x");
            move = move.Replace("x", string.Empty);

            bool isPromotion = move.Contains("=");
            char promotedPiece = ' ';
            if(isPromotion)
            {
                int eqInd = move.IndexOf("=");
                promotedPiece = move[eqInd + 1];
                move = move.Remove(eqInd, 2);
            }

            if(move[0] == 'O')
            {
                string res = MakeCastle(move);
                return res;
            }

            char piece = move[0];
            if(PIECE_SET.Contains(piece))
            {
                move = move.Remove(0, 1);
            }
            else
            {
                piece = 'P';
            }
            Predicate<int> disambg = (int field) => false;

            if (move.Length == 3)
            {
                if(move[0] >= 'a' && move[0] <= 'z')
                {
                    int file = 'h' - move[0] + 1;
                    disambg = (int field) => field % 10 != file;
                }
                else
                {
                    int rank = move[0] - '1' + 2;
                    disambg = (int field) => field / 10 != rank;
                }
                move = move.Remove(0, 1);
            }

            if(move.Length == 4)
            {
                string fieldString = move.Substring(0, 2);
                int fieldNum = ToFieldNumber(fieldString);
                disambg = (int field) => field != fieldNum;
                move = move.Remove(0, 2);
            }

            int target = ToFieldNumber(move);

            int start = FindStart(target, piece, isCapture, disambg);

            if(isCapture)
            {
                char capturedPiece = board[target];
                int removedSquare = target;
                if(capturedPiece == ' ')
                {
                    removedSquare = target - nowMoving.pawnDirection;
                    capturedPiece = board[removedSquare];
                }

                board[removedSquare] = ' ';
                try
                {
                    nowNotMoving.RemovePiece(capturedPiece, removedSquare);
                }
                catch(ArgumentException e)
                {
                    throw e;
                }
                
            }
            
            (board[target], board[start]) = (board[start], board[target]);

            if(isPromotion)
            {
                Promote(promotedPiece, target);
            }

            ChangeTurns();
            
            return ToUCIString(start) + ToUCIString(target);
        }

        private string MakeCastle(string castle)
        {
            if(castle == "O-O")
            {
                int kingStart = nowMoving.king;
                int kingTarget = kingStart - 2;
                int rookTarget = kingStart - 1;
                int rookStart = kingStart - 3;

                board[rookTarget] = board[rookStart];
                board[rookStart] = ' ';
                board[kingTarget] = board[nowMoving.king];
                board[nowMoving.king] = ' ';

                nowMoving.king = kingTarget;
                nowMoving.rooks.Remove(rookStart);
                nowMoving.rooks.Add(rookTarget);

                ChangeTurns();
                return ToUCIString(kingStart) + ToUCIString(kingTarget);
            }
            else if(castle == "O-O-O")
            {
                int kingStart = nowMoving.king;
                int kingTarget = kingStart + 2;
                int rookTarget = kingStart + 1;
                int rookStart = kingStart + 4;

                board[rookTarget] = board[rookStart];
                board[rookStart] = ' ';
                board[kingTarget] = board[kingStart];
                board[kingStart] = ' ';

                nowMoving.king = kingTarget;
                nowMoving.rooks.Remove(rookStart);
                nowMoving.rooks.Add(rookTarget);

                ChangeTurns();
                return ToUCIString(kingStart) + ToUCIString(kingTarget);
            }

            throw new ArgumentException("Move provided is not a castle");
        }

        //returns the starting position of a piece that could move to a target position, makes changes to the Player sets of the player moving now
        private int FindStart(int target, char piece, bool wasCapture, Predicate<int> disambg)
        {
            int start;
            switch (piece)
            {
                case 'P':
                    if (wasCapture)
                        start = PawnCaptureMoves(target, disambg);
                    else
                        start = PawnMove(target);
                    nowMoving.pawns.Remove(start);
                    nowMoving.pawns.Add(target);
                    return start;
                case 'R':
                    start = RookMoves(target, disambg);
                    nowMoving.rooks.Remove(start);
                    nowMoving.rooks.Add(target);
                    return start;
                case 'B':
                    start = BishopMoves(target, disambg);
                    nowMoving.bishops.Remove(start);
                    nowMoving.bishops.Add(target);
                    return start;
                case 'N':
                    start = KnightMove(target, disambg);
                    nowMoving.knights.Remove(start);
                    nowMoving.knights.Add(target);
                    return start;
                case 'Q':
                    start = QueenMoves(target, disambg);
                    nowMoving.queens.Remove(start);
                    nowMoving.queens.Add(target);
                    return start;
                case 'K':
                    start = KingMove(target);
                    nowMoving.king = target;
                    return start;
            }

            throw new ArgumentException("Invalid piece string!");
        }

        private string ToUCIString(int field)
        {
            int rank = field / 10 - 1;
            char file = (char)('h' - (field % 10 - 1));

            return file.ToString() + rank.ToString();
        }

        //first character must be a file letter and second charater must be a rank number e.g. e4
        private int ToFieldNumber(string field)
        {
            int file = 'h' - field[0] + 1;
            int rank = field[1] - '1' + 2;

            return rank * 10 + file;
        }

        private void ChangeTurns()
        {
            (nowMoving, nowNotMoving) = (nowNotMoving, nowMoving);
        }

        private int RookMoves(int target, Predicate<int> disambg)
        {
            int[] directions = { 1, -1, 10, -10 };
            List<int> result = new List<int>();

            foreach(int direction in directions)
            {
                int i = target + direction;
                while (board[i] != 'X')
                {
                    result.Add(i);
                    if (board[i] != ' ')
                        break;
                    i += direction;
                }
            }

            result.RemoveAll(disambg);
            foreach (int square in result)
            {
                if (nowMoving.rooks.Contains(square))
                    return square;
            }
            throw new ArgumentException("There is no piece that can move to this square!");

        }

        private int BishopMoves(int target, Predicate<int> disambg)
        {
            int[] directions = { 9, 11, -9, -11 };
            List<int> result = new List<int>();

            foreach (int direction in directions)
            {
                int i = target + direction;
                while (board[i] != 'X')
                {
                    result.Add(i);
                    if (board[i] != ' ')
                        break;
                    i += direction;
                }
            }

            result.RemoveAll(disambg);

            foreach(int square in result)
            {
                if (nowMoving.bishops.Contains(square))
                    return square;
            }
            throw new ArgumentException("There is no piece that can move to this square!");
        }

        private int QueenMoves(int target, Predicate<int> disambg)
        {
            int[] directions = { 9, 11, -9, -11, 1, -1, 10, -10};
            List<int> result = new List<int>();

            foreach (int direction in directions)
            {
                int i = target + direction;
                while (board[i] != 'X')
                {
                    result.Add(i);
                    if (board[i] != ' ')
                        break;
                    i += direction;
                }
            }

            result.RemoveAll(disambg);

            foreach (int square in result)
            {
                if (nowMoving.queens.Contains(square))
                    return square;
            }
            throw new ArgumentException("There is no piece that can move to this square!");
        }

        private int PawnCaptureMoves(int target, Predicate<int> disambg)
        {

            int square = target - nowMoving.pawnDirection + 1;
            if(nowMoving.pawns.Contains(square) && !disambg(square))
            {
                return square;
            }
            square = target -nowMoving.pawnDirection - 1;
            return square;
        }

        private int PawnMove(int target)
        {
            if(nowMoving.pawns.Contains(target - nowMoving.pawnDirection))
            {
                return target - nowMoving.pawnDirection;
            }
            else
            {
                return target - 2 * nowMoving.pawnDirection;
            }
        }

        private int KingMove(int target)
        {
            int[] directions = {10, -10, 1, -1, 9, -9, 11, -11 };
            foreach(int direction in directions)
            {
                if (target + direction == nowMoving.king)
                {
                    return target + direction;
                }
            }

            throw new ArgumentException("Can't find piece that moves to target");
        }

        private int KnightMove(int target, Predicate<int> disambg)
        {
            //NNW, NNE, SSW, SSE, WWN, WWS, EEN, EES 
            int[] moves = { 19, 21, -21, -19, 8, -12, 12, -8 };
            foreach(int move in moves)
            {
                if(nowMoving.knights.Contains(target + move) && !disambg(target + move))
                {
                    return target + move;
                }
            }

            throw new ArgumentException("Can't find piece that moves to target");
        }

        private void Promote(char piece, int square)
        {
            switch(piece)
            {
                case 'R':
                    nowMoving.rooks.Add(square);
                    break;
                case 'B':
                    nowMoving.bishops.Add(square);
                    break;
                case 'N':
                    nowMoving.knights.Add(square);
                    break;
                case 'Q':
                    nowMoving.queens.Add(square);
                    break;
            }

            nowMoving.pawns.Remove(square);
            if(nowMoving.color == "black")
            {
                piece = piece.ToString().ToLower()[0];
            }

            board[square] = piece;
        }

        public void WriteBoard()
        {
            Console.WriteLine();
            for(int i = 2; i < 10; i++)
            {
                for(int j = 1; j <= 8; j++)
                {
                    Console.Write(board[i * 10 + j]);
                }
                Console.Write("\n");
            }
            Console.WriteLine();
        }
    }
}
