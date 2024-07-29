using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;

namespace MatchThreeEngine
{
    public sealed class GameBoard : MonoBehaviour
    {
        [SerializeField] private TileTypeAsset[] tileTypes;
        [SerializeField] private Row[] rows;
        [SerializeField] private AudioClip matchSound;
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private float tweenDuration;
        [SerializeField] private Transform swappingOverlay;
        [SerializeField] private bool ensureNoStartingMatches;
        [SerializeField] private Slider timerSlider;
        [SerializeField] private float gameDuration = 120f;
        [SerializeField] private TMP_Text winTimeText;
        
        private readonly List<Tile> _selection = new List<Tile>();
        private bool _isSwapping;
        public GameObject EventGame;
        private bool _isMatching;
        private bool _isShuffling;
        private bool _isGameRunning;
        public TMP_Text scoreGame;
        public TMP_Text scoreLose;
        public int score;
        public GameObject winPanel;
        public GameObject lostPanel;
        private float _timeRemaining;
        private Coroutine _timerCoroutine;
        public int currentLevel = 1;
        public TMP_Text currentLevelText;
        public Button[] levelButtons;
        public int levels = 1;
        public GameObject levelMenu;
        public TMP_Text winTime;
        [SerializeField] private TMP_Text timerText;

        public event Action<TileTypeAsset, int> OnMatch;

        
          public void SelectLevelButton(int levelSelected)
        {
            currentLevel = levelSelected;
            levelMenu.SetActive(false);
            StartLevel();
        }

        public void ExitGame()
        {
            Application.Quit();
        }

        void SaveLevelsCompleted()
        {
            PlayerPrefs.SetInt("LevelsCompleted", levels);
            PlayerPrefs.Save();
        }

        void Start()
        {
            LoadLevelsCompleted();
            UpdateButtonAvailability();
        }

        public void LoadNextLevel()
        {
            if (currentLevel < levels) // 
            {
                currentLevel++; // 
            }
            currentLevelText.text = $"Level {currentLevel}";

    
            ResetGridForNextLevel(); // 

            StartLevel(); // 
        }

        void ResetGridForNextLevel()
        {
          
            score = 0;
            scoreGame.text = $"Points: {score}/400";
            
        }

        void LoadLevelsCompleted()
        {
            levels = PlayerPrefs.GetInt("LevelsCompleted", 1);
        }

        void UpdateButtonAvailability()
        {
            for (int i = 0; i < levelButtons.Length; i++)
            {
                levelButtons[i].interactable = i < levels;
            }
        }

        public void StartLevel()
        {
            for (var y = 0; y < rows.Length; y++)
            {
                for (var x = 0; x < rows.Max(row => row.tiles.Length); x++)
                {
                    var tile = GetTile(x, y);

                    tile.x = x;
                    tile.y = y;

                    tile.Type = tileTypes[Random.Range(0, tileTypes.Length)];

                    tile.button.onClick.AddListener(() => ClickTile(tile));
                }
            }
            score = 0;
            scoreGame.text = $"Points: {score}/400";

 
            if (score >= 400)
            {
         
                GameVictory();
                return; 
            }

            // Continue setting up the game
            if (ensureNoStartingMatches) StartCoroutine(EnsureNoStartingMatches());
            StopTimer();
            _timeRemaining = gameDuration;
            timerSlider.maxValue = gameDuration;
            _isGameRunning = true;
            _timerCoroutine = StartCoroutine(CountdownCoroutine());
        }
        
        private TileInfo[,] GameSet
        {
            get
            {
                var width = rows.Max(row => row.tiles.Length);
                var height = rows.Length;

                var data = new TileInfo[width, height];

                for (var y = 0; y < height; y++)
                    for (var x = 0; x < width; x++)
                        data[x, y] = GetTile(x, y).Data;

                return data;
            }
        }
        

        private IEnumerator CountdownCoroutine()
        {
            while (_timeRemaining > 0 && _isGameRunning)
            {
                _timeRemaining -= Time.deltaTime;
                timerSlider.value = _timeRemaining;

                int seconds = (int)_timeRemaining;
                timerText.text = $"Time left: {seconds}s";

                if (_timeRemaining <= 0)
                {
                    GameLoss();
                }

                yield return null;
            }
        }

        public void StopTimer()
        {
            _isGameRunning = false;
            if (_timerCoroutine != null)
            {
                StopCoroutine(_timerCoroutine);
            }
        }

        public void StartTimer()
        {
            _isGameRunning = true;
            _timerCoroutine = StartCoroutine(CountdownCoroutine());
        }
        
        public void UpdatePlayerScore()
        {
            if (score >= 350)
                GameVictory();
        }

        public void PostVictoryActions()
        {
            if (levels < 30)
            {
                levels++;
            }
            SaveLevelsCompleted();
            UpdateButtonAvailability();
        }
        
        void GameVictory()
        {
            Debug.Log("Game won!");

            // Update winPanel text
            if (winPanel != null)
            {
                var winTextComponent = winPanel.GetComponentInChildren<TMP_Text>();
                if (winTextComponent != null)
                {
                    winTextComponent.text = $"Points: {score}/400";
                }
                else
                {
                    Debug.LogError("TMP_Text component not found in winPanel!");
                }

                // Update time
                if (winTimeText != null)
                {
                    int secondsRemaining = Mathf.FloorToInt(_timeRemaining);
                    winTimeText.text = $"Time: {secondsRemaining}s";
                }
                else
                {
                    Debug.LogError("winTimeText component not set in the inspector!");
                }

                winPanel.SetActive(true);
            }
            else
            {
                Debug.LogError("winPanel is not set in the inspector!");
            }

            StopTimer();
        }

        void GameLoss()
        {
            lostPanel.SetActive(true);
            // Set final score text
            scoreLose.text = $"Points: {score}/400";
            StopTimer();
            
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                var bestMove = TileInfoMatrixUtility.FindBestMove(GameSet);

                if (bestMove != null)
                {
                    ClickTile(GetTile(bestMove.X1, bestMove.Y1));
                    ClickTile(GetTile(bestMove.X2, bestMove.Y2));
                }
            }
            UpdatePlayerScore();
        }

        private IEnumerator EnsureNoStartingMatches()
        {
            var wait = new WaitForEndOfFrame();

            while (TileInfoMatrixUtility.FindBestMatch(GameSet) != null)
            {
                ShuffleGrid();
                yield return wait;
            }
        }

        private Tile GetTile(int x, int y) => rows[y].tiles[x];

        private Tile[] GetTiles(IList<TileInfo> TileInfo)
        {
            var length = TileInfo.Count;

            var tiles = new Tile[length];

            for (var i = 0; i < length; i++) tiles[i] = GetTile(TileInfo[i].X, TileInfo[i].Y);

            return tiles;
        }

        private async void ClickTile(Tile tile)
        {
            if (_isSwapping || _isMatching || _isShuffling)
            {
                Debug.Log("Action in progress, selection ignored.");
                return;
            }

            if (!_selection.Contains(tile))
            {
                if (_selection.Count > 0)
                {
                    if (Math.Abs(tile.x - _selection[0].x) == 1 && Math.Abs(tile.y - _selection[0].y) == 0
                        || Math.Abs(tile.y - _selection[0].y) == 1 && Math.Abs(tile.x - _selection[0].x) == 0)
                    {
                        _selection.Add(tile);
                    }
                }
                else
                {
                    _selection.Add(tile);
                }
            }

            if (_selection.Count < 2) return;

            _isSwapping = true;
            bool success = await SwapAndMatchAsync(_selection[0], _selection[1]);
            if (!success)
            {
                await SwapAsync(_selection[0], _selection[1]);
            }
            _isSwapping = false;

            _selection.Clear();
            EnsurePlayableGrid();
        }

        private async Task<bool> SwapAndMatchAsync(Tile tile1, Tile tile2)
        {
            await SwapAsync(tile1, tile2);
            if (await TryMatchAsync())
            {
                return true;
            }
            return false;
        }

        private async Task SwapAsync(Tile tile1, Tile tile2)
        {
            var icon1 = tile1.icon;
            var icon2 = tile2.icon;

            var icon1Transform = icon1.transform;
            var icon2Transform = icon2.transform;

            icon1Transform.SetParent(swappingOverlay);
            icon2Transform.SetParent(swappingOverlay);

            icon1Transform.SetAsLastSibling();
            icon2Transform.SetAsLastSibling();

            icon1Transform.SetParent(tile2.transform);
            icon2Transform.SetParent(tile1.transform);

            tile1.icon = icon2;
            tile2.icon = icon1;

            var tile1Item = tile1.Type;
            tile1.Type = tile2.Type;
            tile2.Type = tile1Item;
        }

        private void EnsurePlayableGrid()
        {
            var matrix = GameSet;

            while (TileInfoMatrixUtility.FindBestMove(matrix) == null || TileInfoMatrixUtility.FindBestMatch(matrix) != null)
            {
                ShuffleGrid();
                matrix = GameSet;
            }
        }

        private async Task<bool> TryMatchAsync()
        {
            var didMatch = false;

            _isMatching = true;

            var match = TileInfoMatrixUtility.FindBestMatch(GameSet);

            while (match != null)
            {
                didMatch = true;

                var tiles = GetTiles(match.Tiles);

                var deflateSequence = DOTween.Sequence();

                foreach (var tile in tiles)
                    deflateSequence.Join(tile.icon.transform.DOScale(Vector3.zero, tweenDuration).SetEase(Ease.InBack));

                audioSource.PlayOneShot(matchSound);

                await deflateSequence.Play().AsyncWaitForCompletion();

                foreach (var tile in tiles)
                    tile.Type = tileTypes[Random.Range(0, tileTypes.Length)];

                var inflateSequence = DOTween.Sequence();

                foreach (var tile in tiles)
                    inflateSequence.Join(tile.icon.transform.DOScale(Vector3.one, tweenDuration).SetEase(Ease.OutBack));

                await inflateSequence.Play().AsyncWaitForCompletion();

                OnMatch?.Invoke(tiles[0].Type, tiles.Length);

                // 
                int matchPoints = 15;
                if (tiles.Length >= 5)
                {
                    matchPoints += 7; // 
                }

                score += matchPoints;
                scoreGame.text = "Points: " + score + "/400"; // Update score text

                match = TileInfoMatrixUtility.FindBestMatch(GameSet);
            }

            _isMatching = false;

            return didMatch;
        }

        private void ShuffleGrid()
        {
            _isShuffling = true;

            foreach (var row in rows)
                foreach (var tile in row.tiles)
                    tile.Type = tileTypes[Random.Range(0, tileTypes.Length)];

            _isShuffling = false;
        }

        public void ResetGrid()
        {
            // Save current timer state
            float remainingTime = _timeRemaining;

            // Stop the current timer coroutine if it's running
            StopTimer();

            // Logic to reset game grid
            ResetGridForNextLevel(); // Example: reset grid for the next level
            StartLevel(); // Restart the game after resetting

            // Restore timer state
            _timeRemaining = remainingTime;
            timerSlider.value = _timeRemaining;
    
            // Start the countdown coroutine again
            StartTimer();
        }

        
        public void ActivateButton()
        {
            EventGame.SetActive(false);
            StartCoroutine("ResetEvent", 0.1f);
        }

        public void ResetEvent()
        {
            EventGame.SetActive(true);
        }
    }
}
