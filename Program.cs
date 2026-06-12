namespace Project_Ensemble {
	internal class Program
	{
		private static void Main(string[] args) {
			using (var game = new GameCore()) {
				game.Run();
			}
		}
	}
}
