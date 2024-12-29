using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WPFLab4.Engine;
namespace WPFLab4
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
    {
        // Экземпляр игрового «движка»
        private GameEngine _gameEngine = new GameEngine();

        public MainWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Обновляем содержимое TextBlock-ов: карту и статус
        /// </summary>
        private void RefreshUI()
        {
            // Если игра не инициализирована, показываем сообщение
            if (!_gameEngine.IsGameInitialized || _gameEngine.DungeonMap == null)
            {
                MapTextBlock.Text = "Запустите игру.";
                StatusTextBlock.Text = "";
                return;
            }

            // Формируем «карту» в виде строк
            var mapSize = _gameEngine.MapSize;
            char[,] map = _gameEngine.DungeonMap;

            // Собираем карту в одну большую строку
            var mapStr = "";
            for (int i = 0; i < mapSize; i++)
            {
                for (int j = 0; j < mapSize; j++)
                {
                    mapStr += map[i, j] + " ";
                }
                mapStr += "\n";
            }
            MapTextBlock.Text = mapStr;

            // Статус
            StatusTextBlock.Text = $"Ходы: {_gameEngine.MoveCount} | Найдено сокровищ: {_gameEngine.TreasureCount}/{_gameEngine.TotalTreasures}";
        }

        #region Кнопки меню
        private void NewGameButton_Click(object sender, RoutedEventArgs e)
        {
            _gameEngine.InitializeGame();
            RefreshUI();
        }

        private void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _gameEngine.LoadGame();
                RefreshUI();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке: {ex.Message}");
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!_gameEngine.IsGameInitialized)
                {
                    MessageBox.Show("Игра не была начата. Нечего сохранять.");
                    return;
                }

                _gameEngine.SaveGame();
                MessageBox.Show("Игра сохранена!");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении: {ex.Message}");
            }
        }

		private void SettingsButton_Click(object sender, RoutedEventArgs e)
		{
			// Окно настроек
			var settingsWindow = new SettingsWindow(_gameEngine);
			// Модально открываем:
			settingsWindow.Owner = this; // чтобы окно было поверх MainWindow
			bool? dialogResult = settingsWindow.ShowDialog();

			// Если нажали «OK», то окно вернёт true, 
			// значит настройки применены:
			if (dialogResult == true)
			{
				if (_gameEngine.IsGameInitialized)
				{
					MessageBox.Show("Настройки применены.\n" +
									"Перезапускаем игру для новых параметров...");
					_gameEngine.InitializeGame();
					RefreshUI();
				}
			}
			else
			{
				// Отменили
			}
		}
		

		/// <summary>
		/// Отслеживаем нажатия клавиш. 
		/// Важно! Нужно поставить фокус на окно, чтобы KeyDown срабатывал.
		/// </summary>
		private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (!_gameEngine.IsGameInitialized) return;

            int dx = 0, dy = 0;
            switch (e.Key)
            {
                case Key.W:
                    dx = -1;  // вверх
                    break;
                case Key.S:
                    dx = 1;   // вниз
                    break;
                case Key.A:
                    dy = -1;  // влево
                    break;
                case Key.D:
                    dy = 1;   // вправо
                    break;
                case Key.Escape:
					// Предложение закрыть программу
					var result = MessageBox.Show(
						"Вы действительно хотите выйти из игры?",  // Текст вопроса
						"Выход",                                   // Заголовок окна
						MessageBoxButton.YesNo,                    // Кнопки "Yes" и "No"
						MessageBoxImage.Question);                 // Иконка вопроса

					if (result == MessageBoxResult.Yes)
					{
						// Закрыть приложение
						Application.Current.Shutdown();
					}
					// Если нажали "No", просто выходим из switch без закрытия
					return;
			}

            try
            {
                // Двигаем игрока
                _gameEngine.MovePlayer(dx, dy);
                // Перемещаем врагов
                _gameEngine.UpdateEnemies();
            }
            catch (Exception ex)
            {
                // Если вылетело исключение "Проигрыш" или "Победа"
                MessageBox.Show(ex.Message);
            }

            // После любой попытки хода — перерисовываем интерфейс
            RefreshUI();
        }
    }
	#endregion
}