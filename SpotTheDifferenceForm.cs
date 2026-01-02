using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.Linq;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;

namespace SpotTheDifferencesGame
{
    public partial class SpotTheDifferenceForm : Form
    {
        private const int FormW = 1000;
        private const int FormH = 750;
        private const int ImgBoxSize = 400;
        private const int SpotRadius= 5;
        private PictureBox picLeft, picRight;
        private System.Windows.Forms.Timer gameTimer;
        private System.Windows.Forms.Timer wrongClickTimer;
        private Point? wrongClickLocation = null;
        private PictureBox clickedPictureBox = null;
        private readonly GameState gameState = new GameState();
        private readonly SoundManager soundManager = new SoundManager();
        private Label statusLabel, timerLabel, attemptsLabel, foundLabel;
        private ComboBox difficultyComboBox;
        private RadioButton timeModeRadio, attemptsModeRadio;
        private RadioButton generateImageRadio, loadBothImagesRadio;
        private Button startButton;
        private Button clearButton;
        private Panel gamePanel;
        private Button btnLoadImage1, btnLoadImage2;

        public SpotTheDifferenceForm()
        {
            InitializeUI();
            InitializeTimers();
        }
        private void InitializeUI()
        {
            this.Text = "Spot the Difference Game";
            this.Size = new Size(FormW, FormH);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Font = new Font("Segoe UI", 9);
            this.BackColor = SystemColors.ControlLight;
            var modeGroupBox = new GroupBox
            {
                Text = "Game Mode",
                Location = new Point(20, 20),
                Size = new Size(200, 60)
            };
            timeModeRadio = new RadioButton
            {
                Text = "Time Based",
                Location = new Point(10, 20),
                Checked = true
            };
            modeGroupBox.Controls.Add(timeModeRadio);
            attemptsModeRadio = new RadioButton
            {
                Text = "Attempts Based",
                Location = new Point(115, 20)
            };
            modeGroupBox.Controls.Add(attemptsModeRadio);
            this.Controls.Add(modeGroupBox);
            var imageSourceGroupBox = new GroupBox
            {
                Text = "Image Source",
                Location = new Point(240, 20),
                Size = new Size(200, 60)
            };
            generateImageRadio = new RadioButton
            {
                Text = "Generate Modified",
                Location = new Point(10, 20),
                Checked = true
            };
            generateImageRadio.CheckedChanged += ImageSourceRadio_CheckedChanged;
            imageSourceGroupBox.Controls.Add(generateImageRadio);
            loadBothImagesRadio = new RadioButton
            {
                Text = "Load",
                Location = new Point(115, 20)
            };
            loadBothImagesRadio.CheckedChanged += ImageSourceRadio_CheckedChanged;
            imageSourceGroupBox.Controls.Add(loadBothImagesRadio);
            this.Controls.Add(imageSourceGroupBox);
            difficultyComboBox = new ComboBox
            {
                Location = new Point(460, 40),
                Size = new Size(120, 30),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            difficultyComboBox.Items.AddRange(Enum.GetNames(typeof(DifficultyLevel)));
            difficultyComboBox.SelectedIndex = (int)gameState.CurrentDifficulty;
            this.Controls.Add(difficultyComboBox);
            startButton = new Button
            {
                Text = "Start Game",
                Location = new Point(600, 40),
                Size = new Size(120, 30),
                BackColor = Color.SteelBlue,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            startButton.FlatAppearance.BorderSize = 0;
            startButton.Click += StartButton_Click;
            this.Controls.Add(startButton);
            btnLoadImage1 = new Button
            {
                Text = "Load Image 1",
                Location = new Point(20, 90),
                Size = new Size(120, 30),
                Visible = false
            };
            btnLoadImage1.Click += BtnLoadImage1_Click;
            this.Controls.Add(btnLoadImage1);
            btnLoadImage2 = new Button
            {
                Text = "Load Image 2",
                Location = new Point(150, 90),
                Size = new Size(120, 30),
                Visible = false
            };
            btnLoadImage2.Click += BtnLoadImage2_Click;
            this.Controls.Add(btnLoadImage2);
            gamePanel = new Panel
            {
                Location = new Point(20, 130),
                Size = new Size(FormW - 40, ImgBoxSize + 20),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
            this.Controls.Add(gamePanel);
            picLeft = new PictureBox
            {
                Location = new Point(10, 10),
                Size = new Size(ImgBoxSize, ImgBoxSize),
                BorderStyle = BorderStyle.FixedSingle,
                SizeMode = PictureBoxSizeMode.Zoom,
                Cursor = Cursors.Hand
            };
            picLeft.MouseClick += PictureBox_MouseClick;
            picLeft.Paint += PictureBox_Paint;
            gamePanel.Controls.Add(picLeft);
            picRight = new PictureBox
            {
                Location = new Point(ImgBoxSize + 30, 10),
                Size = new Size(ImgBoxSize, ImgBoxSize),
                BorderStyle = BorderStyle.FixedSingle,
                SizeMode = PictureBoxSizeMode.Zoom,
                Cursor = Cursors.Hand
            };
            picRight.MouseClick += PictureBox_MouseClick;
            picRight.Paint += PictureBox_Paint;
            gamePanel.Controls.Add(picRight);
            statusLabel = new Label
            {
                Location = new Point(20, ImgBoxSize + 160),
                AutoSize = true,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Text = "Differences remaining: 0"
            };
            this.Controls.Add(statusLabel);
            foundLabel = new Label
            {
                Location = new Point(20, ImgBoxSize + 190),
                AutoSize = true,
                Font = new Font("Segoe UI", 10),
                Text = "Found: 0"
            };
            this.Controls.Add(foundLabel);
            timerLabel = new Label
            {
                Location = new Point(250, ImgBoxSize + 160),
                AutoSize = true,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Text = "Time: 0 s"
            };
            this.Controls.Add(timerLabel);
            attemptsLabel = new Label
            {
                Location = new Point(450, ImgBoxSize + 160),
                AutoSize = true,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Text = "Attempts left: 0"
            };
            this.Controls.Add(attemptsLabel);
            clearButton = new Button
            {
                Text = "Clear",
                Location = new Point(730, 40),
                Size = new Size(120, 30),
                BackColor = Color.LightGray,
                ForeColor = Color.Black,
                FlatStyle = FlatStyle.Flat
            };
            clearButton.FlatAppearance.BorderSize = 0;
            clearButton.Click += ClearButton_Click;
            this.Controls.Add(clearButton);
        }


        ///////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////

        private void InitializeGame()
        {
            gameState.CurrentDifficulty = (DifficultyLevel)difficultyComboBox.SelectedIndex;
            gameState.CurrentGameMode = timeModeRadio.Checked ? GameMode.TimeBased : GameMode.AttemptsBased;
            gameState.ImageSource = generateImageRadio.Checked ? ImageSource.GenerateModified : ImageSource.LoadBoth;
            if (gameState.ImageSource == ImageSource.LoadBoth)
            {
                if (gameState.LeftImage == null || gameState.RightImage == null)
                {
                    MessageBox.Show("Please load both images first","Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                var differences = SpotDiffs(gameState.LeftImage, gameState.RightImage);
                gameState.FoundDifferences.Clear();
                gameState.RemainingDifferences = differences.Count;
            }
            else
            {
                if (gameState.LeftImage == null)
                {
                    MessageBox.Show("Please load an image first","Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                gameState.RightImage = CraftDiffImage(gameState.LeftImage);
                picRight.Image = gameState.RightImage;
                switch (gameState.CurrentDifficulty)
                {
                    case DifficultyLevel.Easy:
                        gameState.RemainingDifferences = 5;
                        break;
                    case DifficultyLevel.Medium:
                        gameState.RemainingDifferences = 8;
                        break;
                    case DifficultyLevel.Hard:
                        gameState.RemainingDifferences = 10;
                        break;
                }
            }
            gameState.FoundDifferences.Clear();
            wrongClickLocation = null;
            clickedPictureBox = null;
            if (gameState.CurrentGameMode == GameMode.TimeBased)
            {
                switch (gameState.CurrentDifficulty)
                {
                    case DifficultyLevel.Easy:
                        gameState.TimeRemaining = 60;
                        break;
                    case DifficultyLevel.Medium:
                        gameState.TimeRemaining = 45;
                        break;
                    case DifficultyLevel.Hard:
                        gameState.TimeRemaining = 30;
                        break;
                }
                gameState.AttemptsLeft = int.MaxValue;
            }
            else
            {
                switch (gameState.CurrentDifficulty)
                {
                    case DifficultyLevel.Easy:
                        gameState.AttemptsLeft = 10;
                        break;
                    case DifficultyLevel.Medium:
                        gameState.AttemptsLeft = 7;
                        break;
                    case DifficultyLevel.Hard:
                        gameState.AttemptsLeft = 5;
                        break;
                }
                gameState.TimeRemaining = int.MaxValue;
            }
            RefreshStats();
        }

        private void InitializeTimers()
        {
            gameTimer = new System.Windows.Forms.Timer { Interval = 1000 };
            gameTimer.Tick += GameTimer_Tick;
            wrongClickTimer = new System.Windows.Forms.Timer { Interval = 1000 };
            wrongClickTimer.Tick += WrongClickTimer_Tick;
        }

        private void StartButton_Click(object sender, EventArgs e)
        {
            if (gameState.ImageSource == ImageSource.GenerateModified)
            {
                using (var imageDialog = new OpenFileDialog())
                {
                    imageDialog.Title = "Select Base Image";
                    imageDialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp";
                    if (imageDialog.ShowDialog() == DialogResult.OK)
                    {
                        try
                        {
                            gameState.LeftImage?.Dispose();
                            gameState.LeftImage = new Bitmap(imageDialog.FileName);
                            picLeft.Image = gameState.LeftImage;
                            InitializeGame();
                            if (gameState.CurrentGameMode == GameMode.TimeBased)
                            {
                                gameTimer.Start();
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Error loading image: {ex.Message}", "Error",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
            else
            {
                InitializeGame();
                if (gameState.CurrentGameMode == GameMode.TimeBased)
                {
                    gameTimer.Start();
                }
            }
        }

        private void BtnLoadImage1_Click(object sender, EventArgs e)
        {
            using (var openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp";
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        gameState.LeftImage?.Dispose();
                        gameState.LeftImage = new Bitmap(openFileDialog.FileName);
                        picLeft.Image = gameState.LeftImage;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error loading image: {ex.Message}","Error" ,MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void BtnLoadImage2_Click(object sender, EventArgs e)
        {
            using (var openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp";
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        gameState.RightImage?.Dispose();
                        gameState.RightImage = new Bitmap(openFileDialog.FileName);
                        picRight.Image = gameState.RightImage;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error loading image: {ex.Message}","Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void ImageSourceRadio_CheckedChanged(object sender, EventArgs e)
        {
            if (generateImageRadio.Checked)
            {
                gameState.ImageSource = ImageSource.GenerateModified;
                btnLoadImage1.Visible = false;
                btnLoadImage2.Visible = false;
            }
            else
            {
                gameState.ImageSource = ImageSource.LoadBoth;
                btnLoadImage1.Visible = true;
                btnLoadImage2.Visible = true;
            }
        }

        private void WrongClickTimer_Tick(object sender, EventArgs e)
        {
            wrongClickTimer.Stop();
            wrongClickLocation = null;
            if (clickedPictureBox != null)
            {
                clickedPictureBox.Invalidate();
                clickedPictureBox = null;
            }
        }
        private void ClearButton_Click(object sender, EventArgs e)
        {
            gameTimer.Stop();
            wrongClickTimer.Stop();
            gameState.Reset();
            picLeft.Image?.Dispose();
            picLeft.Image = null;
            picRight.Image?.Dispose();
            picRight.Image = null;
            statusLabel.Text = "Differences remaining: 0";
            foundLabel.Text = "Found: 0";
            timerLabel.Text = "Time: 0 s";
            attemptsLabel.Text = "Attempts left: 0";
            attemptsLabel.ForeColor = SystemColors.ControlText;
            wrongClickLocation = null;
            clickedPictureBox = null;
            picLeft.Invalidate();
            picRight.Invalidate();
        }



        private List<Rectangle> SpotDiffs(Bitmap image1, Bitmap image2)
        {
            var differences = new List<Rectangle>();
            using (Mat mat1 = image1.ToMat())
            using (Mat mat2 = image2.ToMat())
            using (Mat gray1 = new Mat())
            using (Mat gray2 = new Mat())
            using (Mat diff = new Mat())
            {
                CvInvoke.CvtColor(mat1, gray1, ColorConversion.Bgr2Gray);
                CvInvoke.CvtColor(mat2, gray2, ColorConversion.Bgr2Gray);
                CvInvoke.AbsDiff(gray1, gray2, diff);
                CvInvoke.Threshold(diff, diff, 12, 255, ThresholdType.Binary); //12
                using (VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint())
                using (Mat hierarchy = new Mat())
                {
                    CvInvoke.FindContours(diff, contours, hierarchy, RetrType.External, ChainApproxMethod.ChainApproxSimple);
                    var minArea = 13 ; //13 
                    for (int i = 0; i < contours.Size; i++)
                    {
                        using (VectorOfPoint contour = contours[i])
                        {
                            var area = CvInvoke.ContourArea(contour);
                            if (area > minArea)
                            {
                                var boundingRect = CvInvoke.BoundingRectangle(contour);
                                boundingRect.Inflate(2, 2); 
                                differences.Add(boundingRect);
                            }
                        }
                    }
                }
            }
            return differences;
        }

        private Bitmap CraftDiffImage(Bitmap original)
        {
            var modified = new Bitmap(original);
            var rnd = new Random();
            int differenceCount;

            switch (gameState.CurrentDifficulty)
            {
                case DifficultyLevel.Easy:
                    differenceCount = 5;
                    break;
                case DifficultyLevel.Medium:
                    differenceCount = 8;
                    break;
                case DifficultyLevel.Hard:
                    differenceCount = 10;
                    break;
                default:
                    differenceCount = 5;
                    break;
            }
            List<Point> placedCenters = new List<Point>();
            int minDistance = 50; 
            using (Graphics g = Graphics.FromImage(modified))
            {
                int attempts = 0;
                for (int i = 0; i < differenceCount && attempts < 500; attempts++)
                {
                    int size = rnd.Next(15, 25);
                    int x = rnd.Next(20, modified.Width - size - 20);
                    int y = rnd.Next(20, modified.Height - size - 20);
                    Point center = new Point(x + size / 2, y + size / 2);
                    bool tooClose = placedCenters.Any(p => Distance(p, center) < minDistance);
                    if (tooClose) continue;

                    int sampleX = Math.Min(Math.Max(x + rnd.Next(-10, 10), 0), modified.Width - 1);
                    int sampleY = Math.Min(Math.Max(y + rnd.Next(-10, 10), 0), modified.Height - 1);
                    Color nearbyColor = modified.GetPixel(sampleX, sampleY);
                    Color differenceColor = Color.FromArgb(
                        Math.Min(Math.Max(nearbyColor.R + rnd.Next(-50, 50), 0), 255),
                        Math.Min(Math.Max(nearbyColor.G + rnd.Next(-50, 50), 0), 255),
                        Math.Min(Math.Max(nearbyColor.B + rnd.Next(-50, 50), 0), 255));

                    using (SolidBrush brush = new SolidBrush(differenceColor))
                    {
                        g.FillRectangle(brush, x, y, size, size);
                    }

                    placedCenters.Add(center);
                    i++; 
                }
            }

            string fileName = $"diff_{Guid.NewGuid().ToString().Substring(0, 8)}.png";
            string savePath = Path.Combine(@"C:\Users\LENOVO\Desktop\pics", fileName);
            modified.Save(savePath, System.Drawing.Imaging.ImageFormat.Png);
            return modified;
        }

        private double Distance(Point p1, Point p2)
        {
            int dx = p1.X - p2.X;
            int dy = p1.Y - p2.Y;
            return Math.Sqrt(dx * dx + dy * dy);
        }



        private void PictureBox_MouseClick(object sender, MouseEventArgs e)
        {
            if (gameState.LeftImage == null || gameState.RightImage == null || (!gameTimer.Enabled && gameState.CurrentGameMode == GameMode.TimeBased))
                return;
            if (gameState.RemainingDifferences <= 0 || (gameState.CurrentGameMode == GameMode.AttemptsBased && gameState.AttemptsLeft <= 0))
                return;

            var clickedBox = (PictureBox)sender;
            var clickPoint = MapToImage(clickedBox, e.Location);
            if (clickPoint == Point.Empty || IsHit(clickPoint))
                return;
            Rectangle? differenceRect = HitDiff(clickPoint);

            if (differenceRect.HasValue)
            {
                gameState.FoundDifferences.Add(differenceRect.Value);
                gameState.RemainingDifferences--;
                soundManager.PlaySound("correct");
                if (gameState.RemainingDifferences == 0)
                {
                    gameTimer.Stop();
                    soundManager.PlaySound("win");
                    ShowGameResult("Congratulations! You found all differences!", true);
                }
            }
            else
            {
                gameState.AttemptsLeft--;
                soundManager.PlaySound("wrong");
                wrongClickLocation = e.Location;
                clickedPictureBox = clickedBox;
                wrongClickTimer.Start();
                if (gameState.CurrentGameMode == GameMode.AttemptsBased && gameState.AttemptsLeft == 0)
                {
                    gameTimer.Stop();
                    soundManager.PlaySound("lose");
                    ShowGameResult("Game Over! No more attempts left.", false);
                }
            }
            picLeft.Invalidate();
            picRight.Invalidate();
            RefreshStats();
        }

        private void PictureBox_Paint(object sender, PaintEventArgs e)
        {
            var pictureBox = (PictureBox)sender;
            foreach (var rect in gameState.FoundDifferences)
            {
                Point[] points = {
                    MapPointToUI(pictureBox, rect.Location),
                    MapPointToUI(pictureBox, new Point(rect.Right, rect.Bottom))
                };
                if (points[0] != Point.Empty && points[1] != Point.Empty)
                {
                    int width = points[1].X - points[0].X;
                    int height = points[1].Y - points[0].Y;
                    e.Graphics.DrawRectangle(Pens.Green, points[0].X, points[0].Y, width, height);
                }
            }
            if (wrongClickLocation != null && pictureBox == clickedPictureBox)
            {
                int x = wrongClickLocation.Value.X;
                int y = wrongClickLocation.Value.Y;
                int size = 20;
                e.Graphics.DrawLine(Pens.Red, x - size / 2, y - size / 2, x + size / 2, y + size / 2);
                e.Graphics.DrawLine(Pens.Red, x + size / 2, y - size / 2, x - size / 2, y + size / 2);
            }
        }

        private Point MapToImage(PictureBox box, Point clickLocation)
        {
            if (box.Image == null) return Point.Empty;
            float imageRatio = (float)box.Image.Width / box.Image.Height;
            float boxRatio = (float)box.Width / box.Height;
            int displayWidth, displayHeight;
            int offsetX = 0, offsetY = 0;
            if (boxRatio > imageRatio)
            {
                displayHeight = box.Height;
                displayWidth = (int)(box.Height * imageRatio);
                offsetX = (box.Width - displayWidth) / 2;
            }
            else
            {
                displayWidth = box.Width;
                displayHeight = (int)(box.Width / imageRatio);
                offsetY = (box.Height - displayHeight) / 2;
            }
            if (clickLocation.X < offsetX || clickLocation.X >= offsetX + displayWidth ||
                clickLocation.Y < offsetY || clickLocation.Y >= offsetY + displayHeight)
            {
                return Point.Empty;
            }
            float scaleX = (float)box.Image.Width / displayWidth;
            float scaleY = (float)box.Image.Height / displayHeight;
            int x = (int)((clickLocation.X - offsetX) * scaleX);
            int y = (int)((clickLocation.Y - offsetY) * scaleY);
            return new Point(x, y);
        }

        private Point MapPointToUI(PictureBox box, Point imagePoint)
        {
            if (box.Image == null) return Point.Empty;
            float imageRatio = (float)box.Image.Width / box.Image.Height;
            float boxRatio = (float)box.Width / box.Height;
            int displayWidth, displayHeight;
            int offsetX = 0, offsetY = 0;
            if (boxRatio > imageRatio)
            {
                displayHeight = box.Height;
                displayWidth = (int)(box.Height * imageRatio);
                offsetX = (box.Width - displayWidth) / 2;
            }
            else
            {
                displayWidth = box.Width;
                displayHeight = (int)(box.Width / imageRatio);
                offsetY = (box.Height - displayHeight) / 2;
            }
            float scaleX = displayWidth / (float)box.Image.Width;
            float scaleY = displayHeight / (float)box.Image.Height;
            int x = (int)(imagePoint.X * scaleX) + offsetX;
            int y = (int)(imagePoint.Y * scaleY) + offsetY;
            return new Point(x, y);
        }

        private bool IsHit(Point clickPoint)
        {
            foreach (var rect in gameState.FoundDifferences)
            {
                Rectangle expandedRect = Rectangle.Inflate(rect, SpotRadius , SpotRadius  );
                if (expandedRect.Contains(clickPoint))
                {
                    return true;
                }
            }
            return false;
        }

  

        private Rectangle? HitDiff(Point centerPoint)
        {
            if (IsHit(centerPoint))
                return null;

            for (int dx = -SpotRadius ; dx <= SpotRadius ; dx++)
            {
                for (int dy = -SpotRadius ; dy <= SpotRadius ;dy++)
                {
                    int x = centerPoint.X + dx;
                    int y = centerPoint.Y + dy;
                    if (x >= 0 && y >= 0 && x < gameState.LeftImage.Width && y < gameState.LeftImage.Height)
                    {
                        Color leftColor = gameState.LeftImage.GetPixel(x, y);
                        Color rightColor = gameState.RightImage.GetPixel(x, y);
                        if (!ColorsAreSimilar(leftColor, rightColor))
                        {
                            return FindDifferenceBounds(x, y);
                        }
                    }
                }
            }
            return null;
        }

        private Rectangle FindDifferenceBounds(int startX, int startY)
        {
            var visited = new HashSet<Point>();
            var queue = new Queue<Point>();
            int minX = startX, maxX = startX;
            int minY = startY, maxY = startY;

            queue.Enqueue(new Point(startX, startY));
            visited.Add(new Point(startX, startY));

            while (queue.Count > 0)
            {
                Point p = queue.Dequeue();
                foreach (var offset in new[] {
            new Point(0, 1), new Point(1, 0),
            new Point(0, -1), new Point(-1, 0) })
                {
                    int nx = p.X + offset.X;
                    int ny = p.Y + offset.Y;
                    if (nx < 0 || ny < 0 || nx >= gameState.LeftImage.Width || ny >= gameState.LeftImage.Height)
                        continue;

                    Point np = new Point(nx, ny);
                    if (!visited.Contains(np) && !ColorsAreSimilar(gameState.LeftImage.GetPixel(nx, ny), gameState.RightImage.GetPixel(nx, ny)))
                    {
                        visited.Add(np);
                        queue.Enqueue(np);
                        minX = Math.Min(minX, nx);
                        maxX = Math.Max(maxX, nx);
                        minY = Math.Min(minY, ny);
                        maxY = Math.Max(maxY, ny);
                    }
                }
            }
            return new Rectangle(minX, minY, maxX - minX + 1, maxY - minY + 1);
        }


        private bool ColorsAreSimilar(Color c1, Color c2)
        {
            int threshold = 10;
            return Math.Abs(c1.R - c2.R) < threshold &&
                   Math.Abs(c1.G - c2.G) < threshold &&
                   Math.Abs(c1.B - c2.B) < threshold;
        }

        private void GameTimer_Tick(object sender, EventArgs e)
        {
            if (gameState.CurrentGameMode == GameMode.TimeBased)
            {
                gameState.TimeRemaining--;
                timerLabel.Text = $"Time: {gameState.TimeRemaining} s";
                if (gameState.TimeRemaining <= 0)
                {
                    gameTimer.Stop();
                    soundManager.PlaySound("lose");
                    ShowGameResult("Time's up! Game over.", false);
                }
            }
        }

        private void RefreshStats()
        {
            statusLabel.Text = $"Differences remaining: {gameState.RemainingDifferences}";
            foundLabel.Text = $"Found: {gameState.FoundDifferences.Count}";
            if (gameState.CurrentGameMode == GameMode.TimeBased)
            {
                attemptsLabel.Text = "Attempts: Unlimited";
                attemptsLabel.ForeColor = SystemColors.ControlText;
            }
            else
            {
                attemptsLabel.Text = $"Attempts left: {gameState.AttemptsLeft}";
                if (gameState.AttemptsLeft <= 2)
                    attemptsLabel.ForeColor = Color.Red;
                else if (gameState.AttemptsLeft <= 4)
                    attemptsLabel.ForeColor = Color.Orange;
                else
                    attemptsLabel.ForeColor = SystemColors.ControlText;
            }
        }

        private void ShowGameResult(string message, bool isWin)
        {
            string caption = isWin ? "You Win" : "Game Over";
            MessageBoxIcon icon = isWin ? MessageBoxIcon.Information : MessageBoxIcon.Exclamation;
            MessageBox.Show(message, caption, MessageBoxButtons.OK, icon);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                gameState.LeftImage?.Dispose();
                gameState.RightImage?.Dispose();
                gameTimer?.Dispose();
                wrongClickTimer?.Dispose();
                soundManager?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}