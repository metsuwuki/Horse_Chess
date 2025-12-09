using System;
using System.Collections.Generic;

public static class HorseGame
{
    static string whiteHorseSymb = "♞";
    static string blackHorseSymb = "♘";

    static string[,] board = new string[8, 8];
    static List<(int, int)> whiteHorse = new List<(int, int)>();
    static List<(int, int)> blackHorse = new List<(int, int)>();

    public static void Play()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        whiteHorse = new List<(int, int)> { (7, 1), (7, 6) };
        blackHorse = new List<(int, int)> { (0, 1), (0, 6) };

        InitBoard();

        bool whiteTurn = true;
        while (true)
        {
            PrintBoard();

            if (whiteTurn)
            {
                Console.WriteLine("\nWhite Horse turn (♞.). Type your way, 4 example: b1 c3");
                if (!PlayerMove(true)) break;
            }
            else
            {
                Console.WriteLine("\nBlack Horse turn (♘.). Type your way, 4 example: b8, c6");
                if (!PlayerMove(false)) break;
            }

            if (whiteHorse.Count == 0)
            {
                Console.WriteLine("All white Horses have died! Black Horses win!");
                break;
            }

            if (blackHorse.Count == 0)
            {
                Console.WriteLine("All black Horses have died! White Horses win!");
                break;
            }

            whiteTurn = !whiteTurn;
        }    
        Console.WriteLine("Game Over!");
    }

    static void InitBoard()
    {
        for (int r = 0; r < 8; r++)
            for (int c = 0; c < 8; c++)
                board[r, c] = ".";
        foreach (var w in whiteHorse)
            board[w.Item1, w.Item2] = whiteHorseSymb;
        foreach (var b in blackHorse)
            board[b.Item1, b.Item2] = blackHorseSymb;
    }

    static bool PlayerMove(bool isWhite)
    {
        var horses = isWhite ? whiteHorse : blackHorse;
        var oponentHorse = isWhite ? blackHorse : whiteHorse;

        Console.WriteLine("Your Horses are: " + string.Join(", ", horses.ConvertAll(h => ToSquare(h.Item1, h.Item2))));
        string input = Console.ReadLine();
        if (string.IsNullOrEmpty(input)) return false;

        string[] parts = input.Split(' ');
        
        if (parts.Length != 2)
        {
            Console.WriteLine("Incorrect format the way. Example: b1 c3");
            return true;
        }

        var from = ParseSquare(parts[0]);
        var to = ParseSquare(parts[1]);

        if (!horses.Contains(from))
        {
            Console.WriteLine("You shoud move your own Horse!");
            return true;
        }

        var moves = GetHorseMoves(from.Item1, from.Item2);
        
        if (!moves.Contains(to))
        {
            Console.WriteLine("Unavaliable turn 4 the horse!");
            return true;
        }

        if (horses.Contains(to))
        {
            Console.WriteLine("You can not go to this Square, where already is one of your horses!");
            return true;
        }

        if (oponentHorse.Contains(to))
        {
            Console.WriteLine(isWhite ? "White Horse Murdered the Black Horse!" : "Black Horse Murdered the White Horse!");
            oponentHorse.Remove(to);
        }

        board[from.Item1, from.Item2] = ".";
        board[to.Item1, to.Item2] = isWhite ? whiteHorseSymb : blackHorseSymb;

        horses.Remove(from);
        horses.Add(to);

        return true;
    }

    static List<(int, int)> GetHorseMoves(int row, int col)
    {
        var moves = new List<(int, int)>();
        int[,] offsets =
        {
            { 2, 1 }, { 2, -1 },
            { -2, 1 }, { -2 , -1 },
            { 1, 2 }, { 1, -2 },
            { -1, 2 }, { -1, -2 }
        };
        for (int i = 0; i < offsets.GetLength(0); i++)
        {
            int r = row + offsets[i, 0];
            int c = col + offsets[i, 1];
            if (InBoard(r, c)) moves.Add((r, c));
        }
        return moves;
    }

    static bool InBoard(int r, int c) => r >= 0 && r < 8 && c >= 0 && c < 8;
    static (int, int) ParseSquare(string sqre)
    {
        if (sqre.Length != 2) return (-1, -1);
        char file = sqre[0];
        char rank = sqre[1];
        int col = file - 'a';
        int row = 8 - (rank - '0');
        if (!InBoard(row, col)) return (-1, -1);
        return (row, col);
    }
    static void PrintBoard()
    {
        Console.WriteLine("  +-----------------+");
        for (int r = 0; r < 8; r++)
        {
            Console.Write(8 - r + " | ");
            for (int c = 0; c < 8; c++)
                Console.Write(board[r, c] + " ");
            Console.WriteLine("|");
        }
        Console.WriteLine("  +-----------------+");
        Console.WriteLine("    a b c d e f g h");
    }
    static string ToSquare(int row, int col)
    {
        char file = (char)('a' + col);
        int rank = 8 - row;
        return $"{file}{rank}";
    }
}

class Program
  {
    static void Main()
    {
        HorseGame.Play();
    }
}