using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace Felkaru_Rabló
{
    public partial class MainWindow : Window
    {

        private const int SYMBOLS_PER_REEL = 6; //ezt kell atirni ha nehezebbre akarod(max 18)

        private static readonly string[] ALL_SYMBOLS = new string[]
        {
            "🍒", "🍋", "🍊", "🍇", "🍉", "🔔", "⭐", "💎", "🍀", "7",
            "🍑", "🥝", "🍓", "🍌", "🫐", "BAR", "🎯", "👑"
        };

        private readonly List<string> _reel1Symbols = new List<string>();
        private readonly List<string> _reel2Symbols = new List<string>();
        private readonly List<string> _reel3Symbols = new List<string>();

        private int _reel1Position = 0;
        private int _reel2Position = 0;
        private int _reel3Position = 0;

        private int _balance = 100;
        private int _bet = 10;

        private int _totalSpins = 0;
        private int _totalWins = 0;
        private int _totalLosses = 0;

        private readonly DispatcherTimer[] _reelTimers = new DispatcherTimer[3];
        private readonly int[] _spinCounters = new int[3];
        private readonly int[] _spinTargets = new int[3];
        private bool _isSpinning = false;

        private readonly TextBlock[] _reel1Blocks = new TextBlock[3];
        private readonly TextBlock[] _reel2Blocks = new TextBlock[3];
        private readonly TextBlock[] _reel3Blocks = new TextBlock[3];

        private readonly Random _random = new Random();

        public MainWindow()
        {
            InitializeComponent();
            BuildReelSymbols();
            InitReelDisplays();
            UpdateUI();
        }

        private void BuildReelSymbols()
        {
            List<string> all = new List<string>(ALL_SYMBOLS);
            
            for (int r = 0; r < 3; r++)
            {
                List<string> pool = new List<string>(all);
                List<string> chosen = new List<string>();

                for (int i = 0; i < SYMBOLS_PER_REEL; i++)
                {
                    int idx = _random.Next(pool.Count);
                    chosen.Add(pool[idx]);
                    pool.RemoveAt(idx);
                }

                if (r == 0) _reel1Symbols.AddRange(chosen);
                else if (r == 1) _reel2Symbols.AddRange(chosen);
                else _reel3Symbols.AddRange(chosen);
            }
        }

        private void InitReelDisplays()
        {
            InitSingleReel(Reel1Canvas, _reel1Blocks);
            InitSingleReel(Reel2Canvas, _reel2Blocks);
            InitSingleReel(Reel3Canvas, _reel3Blocks);

            UpdateReelDisplay(Reel1Canvas, _reel1Blocks, _reel1Symbols, _reel1Position);
            UpdateReelDisplay(Reel2Canvas, _reel2Blocks, _reel2Symbols, _reel2Position);
            UpdateReelDisplay(Reel3Canvas, _reel3Blocks, _reel3Symbols, _reel3Position);
        }

        private void InitSingleReel(Canvas canvas, TextBlock[] blocks)
        {
            canvas.Children.Clear();
            double slotHeight = canvas.Height / 3.0;

            for (int i = 0; i < 3; i++)
            {
                SolidColorBrush bg;
                if (i == 1)
                    bg = new SolidColorBrush(Color.FromArgb(40, 233, 69, 96));
                else
                    bg = Brushes.Transparent;

                TextBlock tb = new TextBlock();
                tb.FontSize = 48;
                tb.TextAlignment = TextAlignment.Center;
                tb.Width = canvas.Width;
                tb.Height = slotHeight;
                tb.Background = bg;
                tb.VerticalAlignment = VerticalAlignment.Center;

                Canvas.SetTop(tb, i * slotHeight + (slotHeight - 58) / 2.0);
                canvas.Children.Add(tb);
                blocks[i] = tb;
            }
        }

        private void UpdateReelDisplay(Canvas canvas, TextBlock[] blocks,
                                        List<string> symbols, int centerPos)
        {
            int count = symbols.Count;
            int above = (centerPos - 1 + count) % count;
            int below = (centerPos + 1) % count;

            blocks[0].Text = symbols[above];
            blocks[1].Text = symbols[centerPos];
            blocks[2].Text = symbols[below];
        }

        private void SpinButton_Click(object sender, RoutedEventArgs e)
        {
            if (_isSpinning) return;

            if (_balance < _bet)
            {
                ResultText.Text = "Nincs elég kredit!";
                ResultBackground.Color = Color.FromRgb(60, 20, 20);
                return;
            }

            _balance -= _bet;
            _isSpinning = true;
            SpinButton.IsEnabled = false;
            ResultText.Text = "Pörgés...";
            ResultBackground.Color = Color.FromRgb(22, 33, 62);
            UpdateUI();

            _spinTargets[0] = _random.Next(SYMBOLS_PER_REEL * 3, SYMBOLS_PER_REEL * 6);
            _spinTargets[1] = _random.Next(SYMBOLS_PER_REEL * 4, SYMBOLS_PER_REEL * 7);
            _spinTargets[2] = _random.Next(SYMBOLS_PER_REEL * 5, SYMBOLS_PER_REEL * 8);

            _spinCounters[0] = 0;
            _spinCounters[1] = 0;
            _spinCounters[2] = 0;

            StartReelTimer(0, Reel1Canvas, _reel1Blocks, _reel1Symbols);
            StartReelTimer(1, Reel2Canvas, _reel2Blocks, _reel2Symbols);
            StartReelTimer(2, Reel3Canvas, _reel3Blocks, _reel3Symbols);
        }

        private void StartReelTimer(int reelIndex, Canvas canvas,
                                     TextBlock[] blocks, List<string> symbols)
        {
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(80);
            _reelTimers[reelIndex] = timer;

            timer.Tick += delegate (object s, EventArgs e)
            {
                _spinCounters[reelIndex]++;

                int remaining = _spinTargets[reelIndex] - _spinCounters[reelIndex];
                if (remaining < 5)
                    timer.Interval = TimeSpan.FromMilliseconds(100 + (5 - remaining) * 60);

                if (reelIndex == 0)
                    _reel1Position = (_reel1Position + 1) % symbols.Count;
                else if (reelIndex == 1)
                    _reel2Position = (_reel2Position + 1) % symbols.Count;
                else
                    _reel3Position = (_reel3Position + 1) % symbols.Count;

                int currentPos;
                if (reelIndex == 0) currentPos = _reel1Position;
                else if (reelIndex == 1) currentPos = _reel2Position;
                else currentPos = _reel3Position;

                AnimateReelStep(canvas, blocks, symbols, currentPos);

                if (_spinCounters[reelIndex] >= _spinTargets[reelIndex])
                {
                    timer.Stop();
                    CheckAllStopped();
                }
            };

            timer.Start();
        }

        private void AnimateReelStep(Canvas canvas, TextBlock[] blocks,
                                      List<string> symbols, int centerPos)
        {
            foreach (TextBlock block in blocks)
            {
                TranslateTransform tt = new TranslateTransform();
                block.RenderTransform = tt;

                DoubleAnimation anim = new DoubleAnimation();
                anim.From = -20;
                anim.To = 0;
                anim.Duration = TimeSpan.FromMilliseconds(70);
                anim.EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut };

                tt.BeginAnimation(TranslateTransform.YProperty, anim);
            }

            UpdateReelDisplay(canvas, blocks, symbols, centerPos);
        }

        private void CheckAllStopped()
        {
            foreach (DispatcherTimer t in _reelTimers)
            {
                if (t != null && t.IsEnabled) return;
            }

            _isSpinning = false;
            SpinButton.IsEnabled = true;
            _totalSpins++;

            EvaluateResult();
            UpdateStatistics();
            UpdateUI();
        }

        private void EvaluateResult()
        {
            string s1 = _reel1Symbols[_reel1Position];
            string s2 = _reel2Symbols[_reel2Position];
            string s3 = _reel3Symbols[_reel3Position];

            int multiplier = _bet / 10;

            if (s1 == s2 && s2 == s3)
            {
                int prize = 50 * multiplier;
                _balance += prize;
                _totalWins++;
                ResultText.Text = "JACKPOT! +" + prize + " kredit! (" + s1 + " " + s2 + " " + s3 + ")";
                ResultBackground.Color = Color.FromRgb(20, 80, 20);
            }
            else if (s1 == s2 || s2 == s3 || s1 == s3)
            {
                int prize = 20 * multiplier;
                _balance += prize;
                _totalWins++;
                ResultText.Text = "Nyertél! +" + prize + " kredit! (" + s1 + " " + s2 + " " + s3 + ")";
                ResultBackground.Color = Color.FromRgb(30, 60, 30);
            }
            else
            {
                _totalLosses++;
                ResultText.Text = "Nem nyertél. (" + s1 + " " + s2 + " " + s3 + ")";
                ResultBackground.Color = Color.FromRgb(60, 20, 20);
            }
        }

        private void UpdateUI()
        {
            BalanceText.Text = _balance.ToString();

            if (_balance < _bet)
                SpinButton.IsEnabled = false;
        }

        private void UpdateStatistics()
        {
            StatSpins.Text = _totalSpins.ToString();
            StatWins.Text = _totalWins.ToString();
            StatLosses.Text = _totalLosses.ToString();

            double winRate = 0;
            if (_totalSpins > 0)
                winRate = Math.Round(_totalWins * 100.0 / _totalSpins, 1);

            StatWinRate.Text = winRate.ToString() + "%";
        }

        private void BetComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBoxItem item = BetComboBox.SelectedItem as ComboBoxItem;
            if (item != null)
            {
                int newBet = 0;
                if (int.TryParse(item.Content.ToString(), out newBet))
                {
                    _bet = newBet;
                    UpdateUI();
                }
            }
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            _balance = 100;
            _totalSpins = 0;
            _totalWins = 0;
            _totalLosses = 0;

            _reel1Symbols.Clear();
            _reel2Symbols.Clear();
            _reel3Symbols.Clear();
            BuildReelSymbols();

            _reel1Position = 0;
            _reel2Position = 0;
            _reel3Position = 0;

            UpdateReelDisplay(Reel1Canvas, _reel1Blocks, _reel1Symbols, _reel1Position);
            UpdateReelDisplay(Reel2Canvas, _reel2Blocks, _reel2Symbols, _reel2Position);
            UpdateReelDisplay(Reel3Canvas, _reel3Blocks, _reel3Symbols, _reel3Position);

            ResultText.Text = "Nyomj PÖRGETÉS-t a kezdéshez!";
            ResultBackground.Color = Color.FromRgb(22, 33, 62);

            SpinButton.IsEnabled = true;
            UpdateUI();
            UpdateStatistics();
        }
    }
}