namespace Game
{
    public enum GameState 
    {
        MenuMain,         // (par défaut, avant Start)
        Loading,      // (chargement ressources / scène)
        Cinematic,    // (une cinématique est en train de jouer)
        Gameplay,     // (le joueur contrôle son perso, IA actives, etc.)
        Paused,       // (menu pause, jeu sur pause)
        GameOver,     // (écran de game over)
        Victory       // (écran de victoire)
    }
}