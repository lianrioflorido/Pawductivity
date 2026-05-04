namespace Pawductivity.Models;

public enum PetMood { Happy, Neutral, Sad, Sick }
public enum PetEvolution { Egg, Baby, Junior, Adult, Legend }

/// Base class for all pets. Demonstrates Encapsulation + Inheritance.
public abstract class Pet
{
    // ── Private backing fields (Encapsulation) ───────────────────────
    private int _health;
    private int _mood;
    private int _xp;
    private int _level;
    private int _coins;

    // ── Properties ───────────────────────────────────────────────────
    public string Name { get; set; }
    public PetEvolution Stage { get; protected set; } = PetEvolution.Egg;

    public int Health
    {
        get => _health;
        set => _health = Math.Clamp(value, 0, 100);
    }

    public int Mood
    {
        get => _mood;
        set => _mood = Math.Clamp(value, 0, 100);
    }

    public int XP
    {
        get => _xp;
        set { _xp = value; CheckLevelUp(); }
    }

    public int Level
    {
        get => _level;
        private set => _level = value;
    }

    public int Coins
    {
        get => _coins;
        set => _coins = Math.Max(0, value);
    }

    // ── Derived Properties ───────────────────────────────────────────
    public PetMood CurrentMood => Mood switch
    {
        >= 70 => PetMood.Happy,
        >= 40 => PetMood.Neutral,
        >= 20 => PetMood.Sad,
        _ => PetMood.Sick
    };

    public string MoodEmoji => CurrentMood switch
    {
        PetMood.Happy => "🐾✨",
        PetMood.Neutral => "🐾",
        PetMood.Sad => "😿",
        PetMood.Sick => "🤒",
        _ => "🐾"
    };

    public virtual string StageEmoji => Stage switch
    {
        PetEvolution.Egg => "🥚",
        PetEvolution.Baby => "🐱",
        PetEvolution.Junior => "🐈‍⬛",
        PetEvolution.Adult => "🐈",
        PetEvolution.Legend => "✨🐈‍⬛✨",
        _ => "🐱"
    };

    // ── Constructor ──────────────────────────────────────────────────
    protected Pet(string name)
    {
        Name = name;
        Health = 80;
        Mood = 70;
        XP = 0;
        _level = 1;
        Coins = 0;
    }

    // ── Abstract methods (Polymorphism) ──────────────────────────────
    public abstract void ReactToTaskCompleted(TaskItem task);
    public abstract void ReactToTaskMissed();
    public abstract string GetGreeting();

    // ── Persistence ──────────────────────────────────────────────────
    /// Restores all stat values from a save file WITHOUT triggering
    /// CheckLevelUp. Called only by SaveManager.Restore().
    public void RestoreStats(int health, int mood, int xp, int level, int coins)
    {
        Health = health;
        Mood = mood;
        _xp = xp;    // bypass XP property to skip CheckLevelUp
        _level = level; // bypass Level property (private setter)
        Coins = coins;
        Evolve();       // re-derive Stage from the restored level
    }

    // ── Shared internal logic ────────────────────────────────────────
    private void CheckLevelUp()
    {
        int xpNeeded = _level * 50;
        if (_xp >= xpNeeded)
        {
            _xp -= xpNeeded;
            _level++;
            Evolve();
        }
    }

    private void Evolve()
    {
        Stage = _level switch
        {
            >= 10 => PetEvolution.Legend,
            >= 7 => PetEvolution.Adult,
            >= 4 => PetEvolution.Junior,
            >= 2 => PetEvolution.Baby,
            _ => PetEvolution.Egg
        };
    }

    public int XpForNextLevel => Level * 50;
}
