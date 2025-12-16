using System;
using System.Collections.Generic;

namespace KnightChessGUI
{
    public enum GameResult { None, WhiteWin, BlackWin, Draw }

    public class GameState
    {
        public Board board;
        public bool whiteTurn = true;
        public Piece? selectedPiece = null;
        public bool winnerScreenShown = false;

        public GameState()
        {
            board = new Board();
            PlaceInitialPieces();
        }

        private List<Piece> GetAttackers(string kingColor)
        {
            var attackers = new List<Piece>();
            var king = board.GetKing(kingColor);
            if (king == null) return attackers;

            foreach (var p in board.Grid)
            {
                if (p == null || p.Color == kingColor) continue;
                foreach (var (r, c) in p.Moves(board))
                {
                    if (r == king.R && c == king.C) attackers.Add(p);
                }
            }
            return attackers;
        }

        private HashSet<Piece> GetDefenders(string color)
        {
            var attackers = GetAttackers(color);
            var defenders = new HashSet<Piece>();
            if (attackers.Count == 0) return defenders;
            var king = board.GetKing(color);
            defenders.Add(king!);
            if (attackers.Count > 1) return defenders;
            var attacker = attackers[0];

            foreach (var p in board.Grid)
            {
                if (p == null || p.Color != color) continue;
                var (_, attacks) = board.GetAvailableMoves(p);
                foreach (var (ar, ac) in attacks)
                    if (ar == attacker.R && ac == attacker.C) defenders.Add(p);
            }

            if (attacker is not Knight)
            {
                var line = GetLineSquaresBetween(attacker, king!);
                foreach (var p in board.Grid)
                {
                    if (p == null || p.Color != color || p is King) continue;
                    var (moves, _) = board.GetAvailableMoves(p);
                    foreach (var (mr, mc) in moves)
                        if (line.Contains((mr, mc))) defenders.Add(p);
                }
            }

            return defenders;
        }

        private List<(int r, int c)> GetLineSquaresBetween(Piece attacker, Piece king)
        {
            var line = new List<(int, int)>();
            int dr = Math.Sign(king.R - attacker.R);
            int dc = Math.Sign(king.C - attacker.C);
            if (!((dr == 0 || dc == 0) || Math.Abs(dr) == Math.Abs(dc))) return line;
            int r = attacker.R + dr;
            int c = attacker.C + dc;
            while (r != king.R || c != king.C)
            {
                line.Add((r, c));
                r += dr; c += dc;
            }
            return line;
        }

        private void PlaceInitialPieces()
        {
            AddPiece(new Rook(7, 0, "White"));
            AddPiece(new Knight(7, 1, "White"));
            AddPiece(new Bishop(7, 2, "White"));
            AddPiece(new Queen(7, 3, "White"));
            AddPiece(new King(7, 4, "White"));
            AddPiece(new Bishop(7, 5, "White"));
            AddPiece(new Knight(7, 6, "White"));
            AddPiece(new Rook(7, 7, "White"));
            for (int c = 0; c < 8; c++) AddPiece(new Pawn(6, c, "White"));

            AddPiece(new Rook(0, 0, "Black"));
            AddPiece(new Knight(0, 1, "Black"));
            AddPiece(new Bishop(0, 2, "Black"));
            AddPiece(new Queen(0, 3, "Black"));
            AddPiece(new King(0, 4, "Black"));
            AddPiece(new Bishop(0, 5, "Black"));
            AddPiece(new Knight(0, 6, "Black"));
            AddPiece(new Rook(0, 7, "Black"));
            for (int c = 0; c < 8; c++) AddPiece(new Pawn(1, c, "Black"));
        }

        private void AddPiece(Piece p) => board.Grid[p.R, p.C] = p;

        public void SelectPiece(int r, int c)
        {
            var p = board.GetPiece(r, c);
            string currentColor = whiteTurn ? "White" : "Black";
            if (p == null || p.Color != currentColor) { selectedPiece = null; return; }
            var defenders = GetDefenders(currentColor);
            if (defenders.Count > 0 && !defenders.Contains(p)) { selectedPiece = null; return; }
            selectedPiece = p;
        }

        public bool MoveSelectedPiece(int r, int c)
        {
            if (selectedPiece == null) return false;

            var piece = selectedPiece;
            selectedPiece = null;
            var (moves, attacks) = board.GetAvailableMovesSafe(piece);
            var all = new HashSet<(int, int)>(moves);
            all.UnionWith(attacks);

            if (!all.Contains((r, c))) return false;

            board.MovePiece(piece, r, c);
            string opponentColor = whiteTurn ? "Black" : "White";
            board.IsKingInCheck(opponentColor, true);

            whiteTurn = !whiteTurn;

            return true;
        }

        public GameResult CheckGameOver()
        {
            bool whiteAlive = board.IsKingAlive("White");
            bool blackAlive = board.IsKingAlive("Black");
            if (!whiteAlive) return GameResult.BlackWin;
            if (!blackAlive) return GameResult.WhiteWin;
            string currentColor = whiteTurn ? "White" : "Black";
            bool inCheck = board.IsKingInCheck(currentColor);
            bool hasMoves = HasLegalMoves(currentColor);
            if (inCheck && !hasMoves) return whiteTurn ? GameResult.BlackWin : GameResult.WhiteWin;
            if (!inCheck && !hasMoves) return GameResult.Draw;
            return GameResult.None;
        }

        private bool HasLegalMoves(string color)
        {
            foreach (var p in board.Grid)
            {
                if (p == null || p.Color != color) continue;
                var (moves, attacks) = board.GetAvailableMoves(p);
                foreach (var m in moves) if (!WouldBeInCheck(p, m.Item1, m.Item2)) return true;
                foreach (var a in attacks) if (!WouldBeInCheck(p, a.Item1, a.Item2)) return true;
            }
            return false;
        }

        private bool WouldBeInCheck(Piece piece, int targetR, int targetC)
        {
            int originalR = piece.R; int originalC = piece.C;
            var targetPiece = board.GetPiece(targetR, targetC);
            board.Grid[originalR, originalC] = null;
            board.Grid[targetR, targetC] = piece;
            piece.R = targetR; piece.C = targetC;
            bool inCheck = board.IsKingInCheck(piece.Color, false);
            piece.R = originalR; piece.C = originalC;
            board.Grid[originalR, originalC] = piece;
            board.Grid[targetR, targetC] = targetPiece;
            return inCheck;
        }

        public void EndTurn()
        {
            whiteTurn = !whiteTurn;
            string opponentColor = whiteTurn ? "Black" : "White";
            board.IsKingInCheck(opponentColor, true);
        }
    }
}
