using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using WPFLab4.Engine;

namespace WPFLab4
{
	/// <summary>
	/// Логика взаимодействия для SettingsWindow.xaml
	/// </summary>
	public partial class SettingsWindow : Window
	{
		private GameEngine _engine;

		public SettingsWindow(GameEngine engine)
		{
			InitializeComponent();
			_engine = engine ?? throw new ArgumentNullException(nameof(engine));

			// Заполняем TextBox текущими значениями
			EnemyCountTextBox.Text = _engine.EnemyCount.ToString();
			MapSizeTextBox.Text = _engine.MapSize.ToString();
		}

		private void OkButton_Click(object sender, RoutedEventArgs e)
		{
			// Считываем значения
			if (int.TryParse(EnemyCountTextBox.Text, out int newEnemyCount) && newEnemyCount > 0)
			{
				_engine.EnemyCount = newEnemyCount;
			}
			else
			{
				MessageBox.Show("Некорректное значение для количества врагов.");
				return;
			}

			if (int.TryParse(MapSizeTextBox.Text, out int newMapSize) && newMapSize >= 5)
			{
				_engine.MapSize = newMapSize;
				_engine.TotalTreasures = newMapSize / 2;
			}
			else
			{
				MessageBox.Show("Некорректное значение для размера карты (минимум 5).");
				return;
			}

			// Закрываем окно с результатом «OK»
			this.DialogResult = true;
			this.Close();
		}

		private void CancelButton_Click(object sender, RoutedEventArgs e)
		{
			// Закрываем окно без сохранения
			this.DialogResult = false;
			this.Close();
		}
	}
}
