namespace Pawductivity.Models;

/// Cat pet — earns more XP but loses mood faster. (Inheritance + Polymorphism)</summary>
public class CatPet : Pet
{
    public CatPet(string name) : base(name) { }

    public override void ReactToTaskCompleted(TaskItem task)
    {
        int xp     = task.Priority switch { TaskPriority.High => 30, TaskPriority.Medium => 20, _ => 10 };
        int moodUp = 15;

        XP     += xp;
        Mood   += moodUp;
        Health += 5;
        Coins  += xp / 2;
    }

    public override void ReactToTaskMissed()
    {
        Mood   -= 20;   // cats get grumpy fast
        Health -= 8;
    }

    public override string GetGreeting() => CurrentMood switch
    {
        PetMood.Happy   => $"{Name} purrs and bumps your head! 🐱💕",
        PetMood.Neutral => $"{Name} gives you a slow blink. 🐱",
        PetMood.Sad     => $"{Name} hides under the bed... 😿",
        PetMood.Sick    => $"{Name} needs help! Finish your tasks! 🤒",
        _               => $"{Name} is watching you..."
    };
}

/// Dog pet — more forgiving, spreads rewards more evenly.</summary>
public class DogPet : Pet
{
    public DogPet(string name) : base(name) { }

    public override void ReactToTaskCompleted(TaskItem task)
    {
        int xp     = task.Priority switch { TaskPriority.High => 25, TaskPriority.Medium => 15, _ => 8 };
        int moodUp = 20;   // dogs get happy easier

        XP     += xp;
        Mood   += moodUp;
        Health += 8;
        Coins  += xp / 2;
    }

    public override void ReactToTaskMissed()
    {
        Mood   -= 12;   // dogs are more forgiving
        Health -= 10;
    }

    public override string GetGreeting() => CurrentMood switch
    {
        PetMood.Happy   => $"{Name} wags their tail super fast! 🐶💖",
        PetMood.Neutral => $"{Name} is waiting patiently. 🐶",
        PetMood.Sad     => $"{Name} gives you puppy eyes... 🥺",
        PetMood.Sick    => $"{Name} really needs you to be productive! 🤒",
        _               => $"{Name} is here for you!"
    };
    public override string StageEmoji => Stage switch
    {
        PetEvolution.Egg => "🥚",
        PetEvolution.Baby => "🐶",
        PetEvolution.Junior => "🐕",
        PetEvolution.Adult => "🦮",
        PetEvolution.Legend => "✨🐕‍🦺✨",
        _ => "🐶"
    };
}
