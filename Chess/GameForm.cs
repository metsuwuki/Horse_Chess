using System;
using System.Drawing;
using System.Windows.Forms;

namespace KnightChessGUI
{
    public class GameForm : Form
    {
        private const int CellSize = 80;
        public GameState gameState;
        private Panel? winPanel = null;
        private string? color;

        public GameForm()
        {
            InitializeForm();
            gameState = new GameState();
            SetupSize();
            this.MouseClick += OnMouseClick;
        }

        private void InitializeForm()
        {
            Text = "Knight Chess — GUI";
            DoubleBuffered = true;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            MinimizeBox = true;
            BackColor = Color.FromArgb(255, 156, 134, 110);
        }

        private void SetupSize()
        {
            int w = gameState.board.OffsetX * 2 + CellSize * Board.Size;
            int h = gameState.board.OffsetY * 2 + CellSize * Board.Size;
            ClientSize = new Size(w, h);
        }

        private void OnMouseClick(object? sender, MouseEventArgs e)
        {
            if (gameState.winnerScreenShown) return;

            int col = (e.X - gameState.board.OffsetX) / CellSize;
            int row = (e.Y - gameState.board.OffsetY) / CellSize;

            if (!gameState.board.InBounds(row, col)) return;

            if (gameState.selectedPiece != null)
            {
                bool moved = gameState.MoveSelectedPiece(row, col);
                if (moved)
                {
                    var result = gameState.CheckGameOver();
                    if (result != GameResult.None)
                    {
                        string winner = result == GameResult.WhiteWin ? "White"
                                        : result == GameResult.BlackWin ? "Black"
                                        : "Draw";
                        ShowWinScreen(winner);
                        return;
                    }
                }
            }
            else
            {
                gameState.SelectPiece(row, col);
            }
            Invalidate();
        }


        private void ResetGame()
        {
            gameState = new GameState();
            if (winPanel != null)
            {
                Controls.Remove(winPanel);
                winPanel.Dispose();
                winPanel = null;
            }
            Invalidate();
        }

        private void ShowWinScreen(string winner)
        {
            gameState.winnerScreenShown = true;

            winPanel = new Panel()
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(50, 0, 0, 0) 
            };

            var label = new Label()
            {
                Text = winner == "Draw" ? "DRAW!" : $"{winner} WINS!",
                ForeColor = winner == "White" ? Color.White : Color.Black,
                Font = new Font("Calibri", 48, FontStyle.Bold),
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Top,
                Height = 120
            };
            winPanel.Controls.Add(label);

            if (winner != "Draw")
            {
                string kingImagePath = winner == "White"
                    ? "Images/white_king.png"
                    : "Images/black_king.png";

                if (System.IO.File.Exists(kingImagePath))
                {
                    Image kingImage = Image.FromFile(kingImagePath);
                    var img = new PictureBox()
                    {
                        Image = kingImage,
                        Size = new Size(220, 220),
                        SizeMode = PictureBoxSizeMode.Zoom,
                        BackColor = Color.Transparent
                    };
                    img.Location = new Point(
                        (this.ClientSize.Width - img.Width) / 2,
                        200
                    );
                    winPanel.Controls.Add(img);
                }
            }

            var btnRestart = new Button()
            {
                Text = "Restart",
                ForeColor = Color.FromArgb(255, 90, 77, 63),
                Font = new Font("Calibri", 18, FontStyle.Bold),
                BackColor = Color.FromArgb(220, 96, 223, 107),
                Size = new Size(250, 60),
                Location = new Point((ClientSize.Width - 250) / 2, 450)
            };
            btnRestart.Click += (s, e) => ResetGame();
            winPanel.Controls.Add(btnRestart);

            var btnExit = new Button()
            {
                Text = "Exit",
                Font = new Font("Calibri", 18, FontStyle.Bold),
                ForeColor = Color.FromArgb(255, 90, 77, 63),
                BackColor = Color.FromArgb(220, 223, 82, 72),
                Size = new Size(250, 60),
                Location = new Point((ClientSize.Width - 250) / 2, 530)
            };
            btnExit.Click += (s, e) => Close();
            winPanel.Controls.Add(btnExit);

            Controls.Add(winPanel);
            winPanel.BringToFront();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            gameState.board.Draw(e.Graphics, CellSize, gameState.selectedPiece);
        }
    }
}
