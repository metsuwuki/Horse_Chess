using System;
using System.Collections.Generic;
using System.Drawing;

namespace KnightChessGUI
{
    public abstract class Piece
    {
        public int R, C;
        public string Color;  
        public Image Img;

        protected Piece(int r, int c, string color, Image img)
        {
            R = r;
            C = c;
            Color = color;
            Img = img;
        }

        public abstract List<(int, int)> Moves(Board b);

        protected bool IsCellSafe(Board b, int r, int c)
        {
            if (!b.InBounds(r, c))
                return false;
            var target = b.GetPiece(r, c);
            return target == null || target.Color != this.Color;
        }

        protected bool IsNotNextToEnemyKing(Board b, int r, int c)
        {
            foreach (var p in b.Grid)
            {
                if (p is King k && k.Color != this.Color)
                    if (Math.Abs(k.R - r) <= 1 && Math.Abs(k.C - c) <= 1)
                        return false;
            }
            return true;
        }
    }

    class Knight : Piece
    {
        private static readonly Image WhiteImg = Image.FromFile("Images/white_horse.png");
        private static readonly Image BlackImg = Image.FromFile("Images/black_horse.png");

        public Knight(int r, int c, string color)
            : base(r, c, color, color == "White" ? WhiteImg : BlackImg) { }

        public override List<(int, int)> Moves(Board b)
        {
            int[,] d =
            {
                {2,1},{2,-1},{-2,1},{-2,-1},
                {1,2},{1,-2},{-1,2},{-1,-2}
            };
            var moves = new List<(int, int)>();
            for (int i = 0; i < 8; i++)
            {
                int nr = R + d[i, 0];
                int nc = C + d[i, 1];
                if (IsCellSafe(b, nr, nc)) moves.Add((nr, nc));
            }
            return moves;
        }
    }

    class King : Piece
    {
        private static readonly Image WhiteImg = Image.FromFile("Images/white_king.png");
        private static readonly Image BlackImg = Image.FromFile("Images/black_king.png");

        public King(int r, int c, string color)
            : base(r, c, color, color == "White" ? WhiteImg : BlackImg) { }

        public override List<(int, int)> Moves(Board b)
        {
            var moves = new List<(int, int)>();
            for (int dr = -1; dr <= 1; dr++)
                for (int dc = -1; dc <= 1; dc++)
                {
                    if (dr == 0 && dc == 0) continue;
                    int nr = R + dr;
                    int nc = C + dc;
                    if (IsCellSafe(b, nr, nc) && IsNotNextToEnemyKing(b, nr, nc))
                        moves.Add((nr, nc));
                }
            return moves;
        }
    }

    class Queen : Piece
    {
        private static readonly Image WhiteImg = Image.FromFile("Images/white_queen.png");
        private static readonly Image BlackImg = Image.FromFile("Images/black_queen.png");

        public Queen(int r, int c, string color)
            : base(r, c, color, color == "White" ? WhiteImg : BlackImg) { }

        public override List<(int, int)> Moves(Board b)
        {
            var moves = new List<(int, int)>();
            moves.AddRange(Scan(b, 1, 0));
            moves.AddRange(Scan(b, -1, 0));
            moves.AddRange(Scan(b, 0, 1));
            moves.AddRange(Scan(b, 0, -1));
            moves.AddRange(Scan(b, 1, 1));
            moves.AddRange(Scan(b, 1, -1));
            moves.AddRange(Scan(b, -1, 1));
            moves.AddRange(Scan(b, -1, -1));
            return moves;
        }

        private List<(int, int)> Scan(Board b, int dr, int dc)
        {
            var list = new List<(int, int)>();
            for (int i = 1; i < 8; i++)
            {
                int nr = R + dr * i;
                int nc = C + dc * i;
                if (!IsCellSafe(b, nr, nc)) break;
                list.Add((nr, nc));
                if (b.GetPiece(nr, nc) != null) break;
            }
            return list;
        }
    }

    class Rook : Piece
    {
        private static readonly Image WhiteImg = Image.FromFile("Images/white_rook.png");
        private static readonly Image BlackImg = Image.FromFile("Images/black_rook.png");

        public Rook(int r, int c, string color)
            : base(r, c, color, color == "White" ? WhiteImg : BlackImg) { }

        public override List<(int, int)> Moves(Board b)
        {
            var moves = new List<(int, int)>();
            moves.AddRange(Scan(b, 1, 0));
            moves.AddRange(Scan(b, -1, 0));
            moves.AddRange(Scan(b, 0, 1));
            moves.AddRange(Scan(b, 0, -1));
            return moves;
        }

        private List<(int, int)> Scan(Board b, int dr, int dc)
        {
            var list = new List<(int, int)>();
            for (int i = 1; i < 8; i++)
            {
                int nr = R + dr * i;
                int nc = C + dc * i;
                if (!IsCellSafe(b, nr, nc)) break;
                list.Add((nr, nc));
                if (b.GetPiece(nr, nc) != null) break;
            }
            return list;
        }
    }

    class Bishop : Piece
    {
        private static readonly Image WhiteImg = Image.FromFile("Images/white_bishop.png");
        private static readonly Image BlackImg = Image.FromFile("Images/black_bishop.png");

        public Bishop(int r, int c, string color)
            : base(r, c, color, color == "White" ? WhiteImg : BlackImg) { }

        public override List<(int, int)> Moves(Board b)
        {
            var moves = new List<(int, int)>();
            moves.AddRange(Scan(b, 1, 1));
            moves.AddRange(Scan(b, 1, -1));
            moves.AddRange(Scan(b, -1, 1));
            moves.AddRange(Scan(b, -1, -1));
            return moves;
        }

        private List<(int, int)> Scan(Board b, int dr, int dc)
        {
            var list = new List<(int, int)>();
            for (int i = 1; i < 8; i++)
            {
                int nr = R + dr * i;
                int nc = C + dc * i;
                if (!IsCellSafe(b, nr, nc)) break;
                list.Add((nr, nc));
                if (b.GetPiece(nr, nc) != null) break;
            }
            return list;
        }
    }

    class Pawn : Piece
    {
        private static readonly Image WhiteImg = Image.FromFile("Images/white_pawn.png");
        private static readonly Image BlackImg = Image.FromFile("Images/black_pawn.png");

        public Pawn(int r, int c, string color)
            : base(r, c, color, color == "White" ? WhiteImg : BlackImg) { }

        public override List<(int, int)> Moves(Board b)
        {
            var list = new List<(int, int)>();
            int dir = Color == "White" ? -1 : 1;
            int startRow = Color == "White" ? 6 : 1;

            if (b.InBounds(R + dir, C) && b.GetPiece(R + dir, C) == null)
            {
                list.Add((R + dir, C));
                if (R == startRow && b.GetPiece(R + 2 * dir, C) == null)
                    list.Add((R + 2 * dir, C));
            }
            foreach (int dc in new[] { -1, 1 })
            {
                int nr = R + dir;
                int nc = C + dc;
                if (b.InBounds(nr, nc))
                {
                    var p = b.GetPiece(nr, nc);
                    if (p != null && p.Color != Color) list.Add((nr, nc));
                }
            }

            return list;
        }
    }
}
