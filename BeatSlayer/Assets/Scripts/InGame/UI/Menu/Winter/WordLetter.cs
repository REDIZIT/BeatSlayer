namespace InGame.UI.Menu.Winter
{
    public class WordLetter
    {
		public char letter;
		public bool isCollected;

        public WordLetter() { }
        public WordLetter(char letter)
        {
            this.letter = letter.ToString().ToUpper()[0];
        }
    }
}