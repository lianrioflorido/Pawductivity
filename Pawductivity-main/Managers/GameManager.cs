using Pawductivity.Models;

namespace Pawductivity.Managers;

/// Holds all runtime state. Acts as the glue between forms.
/// Demonstrates Abstraction — forms don't know HOW things are stored.
public class GameManager
{
    public Pet Pet { get; private set; }
    public List<TaskItem> Tasks { get; private set; } = [];
    public List<ShopItem> ShopItems { get; private set; } = ShopItem.DefaultShop();
    public string UserName { get; set; } = string.Empty;

    // ── Stats ────────────────────────────────────────────────────────
    public int TotalCompleted { get; private set; }
    public int CurrentStreak { get; private set; }
    public int LongestStreak { get; private set; }

    /// Exposed so SaveManager can snapshot it.
    /// Not intended for use in UI code.
    public DateTime? LastCompletionDate { get; private set; }

    // tracks the last date we applied overdue penalties.
    // Persisted via SaveManager so we know if a new day has started.
    public DateTime? LastPenaltyDate { get; private set; }

    public GameManager(Pet pet) => Pet = pet;

    // ── Task Operations ──────────────────────────────────────────────
    public void AddTask(TaskItem task) => Tasks.Add(task);

    public void EditTask(Guid id, string title, string desc, TaskPriority priority, DateTime due)
    {
        var t = Tasks.FirstOrDefault(t => t.Id == id);
        if (t is null) return;
        t.Title = title;
        t.Description = desc;
        t.Priority = priority;
        t.DueDate = due;
        // if the new due date is in the future, reset the penalty flag
        // so the task can be penalized again if it goes overdue again.
        if (!t.IsOverdue) t.OverduePenaltyApplied = false;
    }

    public void DeleteTask(Guid id) => Tasks.RemoveAll(t => t.Id == id);

    public PetChangeResult CompleteTask(Guid id)
    {
        var task = Tasks.FirstOrDefault(t => t.Id == id && !t.IsCompleted);
        if (task is null) return PetChangeResult.None;

        int healthBefore = Pet.Health;
        int moodBefore = Pet.Mood;
        int coinsBefore = Pet.Coins;
        int xpBefore = Pet.XP;

        task.Complete();
        Pet.ReactToTaskCompleted(task);
        TotalCompleted++;
        UpdateStreak();

        return new PetChangeResult(
            true,
            XpDelta: Pet.XP - xpBefore,
            MoodDelta: Pet.Mood - moodBefore,
            HealthDelta: Pet.Health - healthBefore,
            CoinDelta: Pet.Coins - coinsBefore,
            AffectedTasks: 1);
    }

    public PetChangeResult ApplyOverduePenalties()
    {
        // reset OverduePenaltyApplied for all overdue tasks if a new
        // calendar day has started since we last applied penalties.
        // This means a task overdue across multiple days keeps hurting the pet
        // once per day — not forever on every timer tick.
        if (LastPenaltyDate.HasValue &&
            LastPenaltyDate.Value.Date < DateTime.Today)
        {
            foreach (var t in Tasks.Where(t => t.IsOverdue))
                t.OverduePenaltyApplied = false;
        }

        var overdueTasks = Tasks
            .Where(t => t.IsOverdue && !t.OverduePenaltyApplied)
            .ToList();

        if (overdueTasks.Count == 0) return PetChangeResult.None;

        int healthBefore = Pet.Health;
        int moodBefore = Pet.Mood;

        foreach (var t in overdueTasks)
        {
            Pet.ReactToTaskMissed();
            t.OverduePenaltyApplied = true;
        }

        LastPenaltyDate = DateTime.Now;

        return new PetChangeResult(
            true,
            MoodDelta: Pet.Mood - moodBefore,
            HealthDelta: Pet.Health - healthBefore,
            AffectedTasks: overdueTasks.Count);
    }

    // ── Shop ─────────────────────────────────────────────────────────
    public PetChangeResult BuyItem(ShopItem item)
    {
        if (Pet.Coins < item.Cost) return PetChangeResult.None;

        int healthBefore = Pet.Health;
        int moodBefore = Pet.Mood;
        int coinsBefore = Pet.Coins;

        Pet.Coins -= item.Cost;
        Pet.Health += item.HealthBoost;
        Pet.Mood += item.MoodBoost;

        return new PetChangeResult(
            true,
            MoodDelta: Pet.Mood - moodBefore,
            HealthDelta: Pet.Health - healthBefore,
            CoinDelta: Pet.Coins - coinsBefore,
            Item: item);
    }

    // ── Analytics ────────────────────────────────────────────────────
    public int CompletedToday =>
        Tasks.Count(t => t.IsCompleted && t.CompletedAt?.Date == DateTime.Today);

    public int PendingCount =>
        Tasks.Count(t => !t.IsCompleted);

    public double CompletionRate =>
        Tasks.Count == 0 ? 0 : (double)Tasks.Count(t => t.IsCompleted) / Tasks.Count * 100;

    // ── Persistence ──────────────────────────────────────────────────
    public void RestoreProgress(
        List<TaskItem> tasks,
        int totalCompleted,
        int currentStreak,
        int longestStreak,
        DateTime? lastCompletionDate,
        DateTime? lastPenaltyDate = null)   
    {
        Tasks = tasks;
        TotalCompleted = totalCompleted;
        CurrentStreak = currentStreak;
        LongestStreak = longestStreak;
        LastCompletionDate = lastCompletionDate;
        LastPenaltyDate = lastPenaltyDate;  
    }

    // ── Private helpers ──────────────────────────────────────────────
    private int GetTaskXpReward(TaskItem task)
    {
        if (Pet is CatPet)
            return task.Priority switch
            {
                TaskPriority.High => 30,
                TaskPriority.Medium => 20,
                _ => 10,
            };

        return task.Priority switch
        {
            TaskPriority.High => 25,
            TaskPriority.Medium => 15,
            _ => 8,
        };
    }

    private void UpdateStreak()
    {
        var today = DateTime.Today;

        if (LastCompletionDate == null ||
            LastCompletionDate.Value.Date < today.AddDays(-1))
            CurrentStreak = 1;
        else if (LastCompletionDate.Value.Date == today.AddDays(-1))
            CurrentStreak++;

        LastCompletionDate = DateTime.Now;
        if (CurrentStreak > LongestStreak) LongestStreak = CurrentStreak;
    }
}
