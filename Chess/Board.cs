using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace KnightChessGUI
{
    public class Board
    {
        public const int Size = 8;

        public (int r, int c)? CheckedKing = null;
        public Piece?[,] Grid = new Piece?[Size, Size];

        public int OffsetX = 40;
        public int OffsetY = 40;

        public readonly List<MoveRecord> MoveHistory = new();
        private readonly HashSet<Piece> movedPieces = new();
        public (int r, int c)? EnPassantTarget = null;

        public record MoveRecord(
            Piece Piece,
            int FromR, int FromC,
            int ToR, int ToC,
            Piece? Captured,
            bool IsCastle,
            bool IsEnPassant,
            Piece? PromotionResult
        );

        public bool InBounds(int r, int c) =>
            r >= 0 && r < Size && c >= 0 && c < Size;

        public Piece? GetPiece(int r, int c) =>
            InBounds(r, c) ? Grid[r, c] : null;

        public bool IsEmpty(int r, int c) => GetPiece(r, c) == null;

        public bool IsSquareAttacked(int r, int c, string attackerColor)
        {
            foreach (var p in Grid)
            {
                if (p == null || p.Color != attackerColor) continue;
                foreach (var (ar, ac) in p.Moves(this))
                {
                    if (ar == r && ac == c)
                        return true;
                }
            }
            return false;
        }

        public Piece? GetKing(string color)
        {
            foreach (var p in Grid)
                if (p is King k && k.Color == color)
                    return k;
            return null;
        }

        public bool IsKingAlive(string color) => GetKing(color) != null;

        public (List<(int r, int c)> moves, List<(int r, int c)> attacks) GetAvailableMoves(Piece p)
        {
            var moves = new List<(int, int)>();
            var attacks = new List<(int, int)>();

            foreach (var (r, c) in p.Moves(this))
            {
                var target = GetPiece(r, c);
                if (target == null) moves.Add((r, c));
                else if (target.Color != p.Color) attacks.Add((r, c));
            }

            if (p is Pawn pawn)
            {
                if (EnPassantTarget != null)
                {
                    var (er, ec) = EnPassantTarget.Value;
                    int dir = pawn.Color == "White" ? -1 : 1;
                    if (er == pawn.R + dir && Math.Abs(ec - pawn.C) == 1)
                    {
                        var capturedPawn = GetPiece(pawn.R, ec);
                        if (capturedPawn is Pawn cp && cp.Color != pawn.Color)
                            moves.Add((er, ec));
                    }
                }
            }

            if (p is King king)
            {
                if (!HasMoved(king) && !IsKingInCheck(king.Color))
                {
                    int r = king.R;
                    if (CanCastleShort(king)) moves.Add((r, king.C + 2));
                    if (CanCastleLong(king)) moves.Add((r, king.C - 2));
                }
            }

            return (moves, attacks);
        }

        public (List<(int r, int c)> moves, List<(int r, int c)> attacks) GetAvailableMovesSafe(Piece p)
        {
            var (moves, attacks) = GetAvailableMoves(p);
            var safeMoves = new List<(int, int)>();
            var safeAttacks = new List<(int, int)>();

            foreach (var (r, c) in moves)
                if (!MovePutsKingInCheck(p, r, c))
                    safeMoves.Add((r, c));

            foreach (var (r, c) in attacks)
                if (!MovePutsKingInCheck(p, r, c))
                    safeAttacks.Add((r, c));

            return (safeMoves, safeAttacks);
        }

        private bool HasMoved(Piece p) => movedPieces.Contains(p);

        private bool CanCastleShort(King king)
        {
            int r = king.R;
            int c = king.C;
            int rookC = Size - 1;
            var rook = GetPiece(r, rookC) as Rook;
            if (rook == null || rook.Color != king.Color) return false;
            if (HasMoved(rook) || HasMoved(king)) return false;
            for (int cc = c + 1; cc < rookC; cc++) if (!IsEmpty(r, cc)) return false;
            if (IsSquareAttacked(r, c, OpponentColor(king.Color))) return false;
            if (IsSquareAttacked(r, c + 1, OpponentColor(king.Color))) return false;
            if (IsSquareAttacked(r, c + 2, OpponentColor(king.Color))) return false;
            return true;
        }

        private bool CanCastleLong(King king)
        {
            int r = king.R;
            int c = king.C;
            int rookC = 0;
            var rook = GetPiece(r, rookC) as Rook;
            if (rook == null || rook.Color != king.Color) return false;
            if (HasMoved(rook) || HasMoved(king)) return false;
            for (int cc = rookC + 1; cc < c; cc++) if (!IsEmpty(r, cc)) return false;
            if (IsSquareAttacked(r, c, OpponentColor(king.Color))) return false;
            if (IsSquareAttacked(r, c - 1, OpponentColor(king.Color))) return false;
            if (IsSquareAttacked(r, c - 2, OpponentColor(king.Color))) return false;
            return true;
        }

        private static string OpponentColor(string color) => color == "White" ? "Black" : "White";

        public void MovePiece(Piece piece, int newR, int newC)
        {
            int oldR = piece.R;
            int oldC = piece.C;

            Piece? captured = null;
            bool isCastle = false;
            bool isEnPassant = false;
            Piece? promotionResult = null;

            if (piece is Pawn pawn && EnPassantTarget != null)
            {
                var (er, ec) = EnPassantTarget.Value;
                if (newR == er && newC == ec && oldC != ec && IsEmpty(newR, newC))
                {
                    int capturedR = oldR;
                    int capturedC = newC;
                    captured = Grid[capturedR, capturedC];
                    if (captured is Pawn cp && cp.Color != pawn.Color)
                    {
                        Grid[capturedR, capturedC] = null;
                        isEnPassant = true;
                    }
                }
            }

            if (piece is King && Math.Abs(newC - oldC) == 2)
            {
                isCastle = true;
                if (newC > oldC)
                {
                    var rook = Grid[oldR, Size - 1];
                    if (rook is Rook r)
                    {
                        Grid[oldR, Size - 1] = null;
                        Grid[oldR, oldC + 1] = r;
                        r.R = oldR; r.C = oldC + 1;
                        movedPieces.Add(r);
                    }
                }
                else
                {
                    var rook = Grid[oldR, 0];
                    if (rook is Rook r)
                    {
                        Grid[oldR, 0] = null;
                        Grid[oldR, oldC - 1] = r;
                        r.R = oldR; r.C = oldC - 1;
                        movedPieces.Add(r);
                    }
                }
            }

            if (!isEnPassant) captured = Grid[newR, newC];

            Grid[oldR, oldC] = null;
            Grid[newR, newC] = piece;
            piece.R = newR; piece.C = newC;
            movedPieces.Add(piece);

            if (piece is Pawn movedPawn)
            {
                if ((movedPawn.Color == "White" && newR == 0) ||
                    (movedPawn.Color == "Black" && newR == 7))
                {
                    var queen = new Queen(newR, newC, movedPawn.Color);
                    Grid[newR, newC] = queen;
                    promotionResult = queen;
                    movedPieces.Add(queen);
                }
            }

            if (piece is Pawn p2 && Math.Abs(newR - oldR) == 2)
            {
                int betweenR = (oldR + newR) / 2;
                EnPassantTarget = (betweenR, newC);
            }
            else
            {
                EnPassantTarget = null;
            }

            MoveHistory.Add(new MoveRecord(piece, oldR, oldC, newR, newC, captured, isCastle, isEnPassant, promotionResult));
        }

        private bool MovePutsKingInCheck(Piece piece, int newR, int newC)
        {
            int oldR = piece.R;
            int oldC = piece.C;

            Piece? targetBefore = Grid[newR, newC];
            Piece? capturedEnPassant = null;
            (int r, int c)? oldEnPassant = EnPassantTarget;

            bool wasCastle = piece is King && Math.Abs(newC - oldC) == 2;
            Piece? rookMovedDuringCastle = null;
            (int rookOldR, int rookOldC, int rookNewR, int rookNewC) rookMoveInfo = (-1, -1, -1, -1);

            try
            {
                if (piece is Pawn && EnPassantTarget != null)
                {
                    var (er, ec) = EnPassantTarget.Value;
                    if (newR == er && newC == ec && IsEmpty(newR, newC) && oldC != newC)
                    {
                        capturedEnPassant = Grid[oldR, newC];
                        Grid[oldR, newC] = null;
                    }
                }

                if (wasCastle)
                {
                    int r = oldR;
                    if (newC > oldC)
                    {
                        var rook = Grid[r, Size - 1];
                        if (rook is Rook rr)
                        {
                            rookMovedDuringCastle = rr;
                            rookMoveInfo = (r, Size - 1, r, oldC + 1);
                            Grid[r, Size - 1] = null;
                            Grid[r, oldC + 1] = rr;
                            rr.R = r; rr.C = oldC + 1;
                        }
                    }
                    else
                    {
                        var rook = Grid[r, 0];
                        if (rook is Rook rr)
                        {
                            rookMovedDuringCastle = rr;
                            rookMoveInfo = (r, 0, r, oldC - 1);
                            Grid[r, 0] = null;
                            Grid[r, oldC - 1] = rr;
                            rr.R = r; rr.C = oldC - 1;
                        }
                    }
                }

                Grid[oldR, oldC] = null;
                Grid[newR, newC] = piece;
                piece.R = newR; piece.C = newC;

                bool inCheck = IsKingInCheck(piece.Color, false);
                return inCheck;
            }
            finally
            {
                Grid[oldR, oldC] = piece;
                piece.R = oldR; piece.C = oldC;
                Grid[newR, newC] = targetBefore;
                if (capturedEnPassant != null) Grid[oldR, newC] = capturedEnPassant;

                if (rookMovedDuringCastle != null)
                {
                    var (rOldR, rOldC, rNewR, rNewC) = rookMoveInfo;
                    Grid[rOldR, rOldC] = rookMovedDuringCastle;
                    Grid[rNewR, rNewC] = null;
                    rookMovedDuringCastle.R = rOldR;
                    rookMovedDuringCastle.C = rOldC;
                }

                EnPassantTarget = oldEnPassant;
            }
        }

        public bool IsKingInCheck(string color, bool setHighlight = true)
        {
            var king = GetKing(color);
            if (king == null)
            {
                if (setHighlight) CheckedKing = null;
                return true;
            }

            bool attacked = IsSquareAttacked(king.R, king.C, OpponentColor(color));
            if (setHighlight) CheckedKing = attacked ? (king.R, king.C) : null;
            return attacked;
        }

        public Dictionary<Piece, List<(int r, int c)>> GetAllLegalMoves(string color)
        {
            var res = new Dictionary<Piece, List<(int r, int c)>>();
            foreach (var p in Grid)
            {
                if (p == null || p.Color != color) continue;
                var (moves, attacks) = GetAvailableMoves(p);
                var all = new List<(int, int)>();
                all.AddRange(moves); all.AddRange(attacks);

                var legal = new List<(int, int)>();
                foreach (var (r, c) in all)
                    if (!MovePutsKingInCheck(p, r, c))
                        legal.Add((r, c));
                if (legal.Count > 0) res[p] = legal;
            }
            return res;
        }

        public void Draw(Graphics g, int cellSize, Piece? selectedPiece)
        {
            var light = Color.FromArgb(255, 245, 222, 179);
            var dark = Color.FromArgb(255, 90, 77, 63);

            HashSet<(int, int)>? moves = null;
            HashSet<(int, int)>? attacks = null;

            if (selectedPiece != null)
            {
                var (m, a) = GetAvailableMovesSafe(selectedPiece);
                moves = new HashSet<(int, int)>(m);
                attacks = new HashSet<(int, int)>(a);
            }

            for (int r = 0; r < Size; r++)
            {
                for (int c = 0; c < Size; c++)
                {
                    var rect = new Rectangle(OffsetX + c * cellSize, OffsetY + r * cellSize, cellSize, cellSize);
                    bool isDark = (r + c) % 2 == 1;
                    using (var brush = new SolidBrush(isDark ? dark : light)) g.FillRectangle(brush, rect);
                    if (moves?.Contains((r, c)) == true) g.FillRectangle(new SolidBrush(Color.FromArgb(120, 96, 219, 107)), rect);
                    if (attacks?.Contains((r, c)) == true) g.FillRectangle(new SolidBrush(Color.FromArgb(150, 224, 82, 72)), rect);
                    var p = Grid[r, c];
                    if (p != null) g.DrawImage(p.Img, rect);
                    g.DrawRectangle(Pens.Black, rect);
                }
            }

            if (CheckedKing != null)
            {
                var (kr, kc) = CheckedKing.Value;
                g.FillRectangle(new SolidBrush(Color.FromArgb(150, 255, 0, 0)),
                    OffsetX + kc * cellSize, OffsetY + kr * cellSize, cellSize, cellSize);
            }

            using Font f = new("Calibri", 16, FontStyle.Bold);
            using SolidBrush textBrush = new(Color.FromArgb(255, 90, 77, 63));
            using var sf = new StringFormat() { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
            string[] letters = { "A", "B", "C", "D", "E", "F", "G", "H" };

            for (int c = 0; c < Size; c++)
            {
                RectangleF top = new(OffsetX + c * cellSize, OffsetY - cellSize * 0.75f, cellSize, cellSize);
                RectangleF bottom = new(OffsetX + c * cellSize, OffsetY + Size * cellSize - cellSize * 0.25f, cellSize, cellSize);
                g.DrawString(letters[c], f, textBrush, top, sf);
                g.DrawString(letters[c], f, textBrush, bottom, sf);
            }

            for (int r = 0; r < Size; r++)
            {
                string number = (Size - r).ToString();
                RectangleF left = new(OffsetX - cellSize * 0.75f, OffsetY + r * cellSize, cellSize, cellSize);
                RectangleF right = new(OffsetX + Size * cellSize - cellSize * 0.25f, OffsetY + r * cellSize, cellSize, cellSize);
                g.DrawString(number, f, textBrush, left, sf);
                g.DrawString(number, f, textBrush, right, sf);
            }
        }

        private void RecomputeMovedPieces()
        {
            movedPieces.Clear();
            foreach (var rec in MoveHistory)
                movedPieces.Add(rec.Piece);
        }
    }
}
