using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace WPFLab4.Engine
{
	[Serializable]
	public class GameState
	{
		public char[,] DungeonMap;
		public int PlayerX;
		public int PlayerY;
		public int TreasureCount;
		public int MoveCount;
		public int[,] Enemies;
		public int MapSize;
		public int TotalTreasures;
		public int EnemyCount;
	}

	public class GameEngine
	{
		// Основные поля, как и в консольной версии:
		public char[,] DungeonMap;
		public int PlayerX, PlayerY;
		public int TreasureCount = 0;
		public int MoveCount = 0;
		public int EnemyCount = 3;
		public int[,] Enemies;
		public int MapSize = 10;
		public int TotalTreasures = 5;

		// Некоторые настройки
		public string SaveFilePath { get; set; } = "savegame.txt";
		public bool IsGameInitialized { get; private set; } = false;

		private bool _exitGame = false;
		private static Random _random = new Random();

		// Конструктор
		public GameEngine()
		{
		}

		public void InitializeGame()
		{
			// Инициализация карты
			DungeonMap = new char[MapSize, MapSize];
			for (int i = 0; i < MapSize; i++)
			{
				for (int j = 0; j < MapSize; j++)
				{
					DungeonMap[i, j] = '.';
				}
			}

			// Размещение игрока
			PlayerX = _random.Next(0, MapSize);
			PlayerY = _random.Next(0, MapSize);
			DungeonMap[PlayerX, PlayerY] = 'P';

			// Размещение сокровищ
			for (int i = 0; i < TotalTreasures; i++)
			{
				int treasureX, treasureY;
				do
				{
					treasureX = _random.Next(0, MapSize);
					treasureY = _random.Next(0, MapSize);
				} while (DungeonMap[treasureX, treasureY] != '.');

				DungeonMap[treasureX, treasureY] = 'T';
			}

			// Размещение врагов
			Enemies = new int[EnemyCount, 2];
			for (int i = 0; i < EnemyCount; i++)
			{
				int enemyX, enemyY;
				do
				{
					enemyX = _random.Next(0, MapSize);
					enemyY = _random.Next(0, MapSize);
				} while (DungeonMap[enemyX, enemyY] != '.');

				Enemies[i, 0] = enemyX;
				Enemies[i, 1] = enemyY;
				DungeonMap[enemyX, enemyY] = 'E';
			}

			// Сброс счётчиков
			TreasureCount = 0;
			MoveCount = 0;
			IsGameInitialized = true;
			_exitGame = false;
		}

		public void ResetGameState()
		{
			DungeonMap = null;
			TreasureCount = 0;
			MoveCount = 0;
			Enemies = null;
			IsGameInitialized = false;
		}

		public void MovePlayer(int dx, int dy)
		{
			if (!IsGameInitialized) return;

			int newX = PlayerX + dx;
			int newY = PlayerY + dy;

			// Проверяем границы
			if (newX < 0 || newX >= MapSize || newY < 0 || newY >= MapSize)
				return;

			// Проверяем, нет ли там врага
			if (DungeonMap[newX, newY] == 'E')
			{
				// Столкновение с врагом
				// Можно кинуть event/событие или просто сохранить состояние, что проигрыш
				ResetGameState();
				throw new Exception("Вы столкнулись с врагом! Игра окончена.");
			}

			// Проверяем, нет ли там сокровища
			if (DungeonMap[newX, newY] == 'T')
			{
				TreasureCount++;
			}

			// Перемещаем игрока
			DungeonMap[PlayerX, PlayerY] = '.';
			PlayerX = newX;
			PlayerY = newY;
			DungeonMap[PlayerX, PlayerY] = 'P';

			MoveCount++;

			// Проверяем, не собраны ли все сокровища
			if (TreasureCount == TotalTreasures)
			{
				// Победа!
				// Можно кинуть событие или бросить исключение, или просто проставить флаг
				// Для простоты выбросим исключение с текстом "Победа"
				ResetGameState();
				throw new Exception("Поздравляем, вы нашли все сокровища! Победа!");
			}
		}

		/// <summary>
		/// Перемещаем врагов (аналог UpdateEnemies).
		/// Если враг встал на игрока – конец игры.
		/// </summary>
		public void UpdateEnemies()
		{
			if (!IsGameInitialized) return;

			// Перебираем врагов
			for (int i = 0; i < EnemyCount; i++)
			{
				int enemyX = Enemies[i, 0];
				int enemyY = Enemies[i, 1];

				// Стираем старое положение
				DungeonMap[enemyX, enemyY] = '.';

				// Движение в случайном направлении
				int direction = _random.Next(4);
				switch (direction)
				{
					case 0: enemyX = Math.Max(0, enemyX - 1); break; // вверх
					case 1: enemyX = Math.Min(MapSize - 1, enemyX + 1); break; // вниз
					case 2: enemyY = Math.Max(0, enemyY - 1); break; // влево
					case 3: enemyY = Math.Min(MapSize - 1, enemyY + 1); break; // вправо
				}

				// Проверяем, можно ли встать на клетку
				// Если клетка свободна ('.') или там игрок ('P'), то пересчитываем
				if (DungeonMap[enemyX, enemyY] == '.' || DungeonMap[enemyX, enemyY] == 'P')
				{
					Enemies[i, 0] = enemyX;
					Enemies[i, 1] = enemyY;
				}

				// Отрисовываем новую позицию
				DungeonMap[Enemies[i, 0], Enemies[i, 1]] = 'E';

				// Проверяем столкновение
				if (Enemies[i, 0] == PlayerX && Enemies[i, 1] == PlayerY)
				{
					ResetGameState();
					throw new Exception("Враг поймал вас! Игра окончена.");
				}
			}
		}

		#region Save/Load
		public void SaveGame(string filePath = null)
		{
			filePath ??= SaveFilePath;
			GameState state = new GameState
			{
				DungeonMap = DungeonMap,
				PlayerX = PlayerX,
				PlayerY = PlayerY,
				TreasureCount = TreasureCount,
				MoveCount = MoveCount,
				Enemies = Enemies,
				MapSize = MapSize,
				TotalTreasures = TotalTreasures,
				EnemyCount = EnemyCount
			};

			IFormatter formatter = new BinaryFormatter();
			using (Stream stream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
			{
				formatter.Serialize(stream, state);
			}
		}

		public void LoadGame(string filePath = null)
		{
			filePath ??= SaveFilePath;
			IFormatter formatter = new BinaryFormatter();
			using (Stream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
			{
				GameState state = (GameState)formatter.Deserialize(stream);
				DungeonMap = state.DungeonMap;
				PlayerX = state.PlayerX;
				PlayerY = state.PlayerY;
				TreasureCount = state.TreasureCount;
				MoveCount = state.MoveCount;
				Enemies = state.Enemies;
				MapSize = state.MapSize;
				TotalTreasures = state.TotalTreasures;
				EnemyCount = state.EnemyCount;
			}

			IsGameInitialized = true;
		}
		#endregion
	}
}
