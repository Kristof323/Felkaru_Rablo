using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace Felkaru_Rabló
{
    public partial class MainWindow : Window
    {
        // TESZTELÉSI PARAMÉTER – csak ezt változtasd!
        // Teszteléskor: 4  |  Éles gépen: 18
        private const int SYMBOLS_PER_REEL = 4;

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
        }

        private void SpinButton_Click(object sender, RoutedEventArgs e) { }
        private void ResetButton_Click(object sender, RoutedEventArgs e) { }
        private void BetComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) { }
    }
}