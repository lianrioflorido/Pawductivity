using Pawductivity.Models;

namespace Pawductivity.Persistence;

/// Flat, serialization-friendly snapshot of everything that must survive a restart.
/// All properties have safe defaults so loading a partial/old file never crashes.
public class SaveData
{
    // ── User ────────────────────────────────────────────────────────
    public string UserName { get; set; } = string.Empty;

    // ── Pet ─────────────────────────────────────────────────────────
    public PetSaveData Pet { get; set; } = new();

    // ── Tasks ────────────────────────────────────────────────────────
    public List<TaskSaveData> Tasks { get; set; } = [];

    // ── Game progress ────────────────────────────────────────────────
    public int TotalCompleted { get; set; }
    public int CurrentStreak { get; set; }
    public int LongestStreak { get; set; }
    public DateTime? LastCompletionDate { get; set; }
}

/// Serializable snapshot of a Pet.</summary>
public class PetSaveData
{
    /// "Cat" or "Dog" — used to reconstruct the concrete subclass.</summary>
    public string PetType { get; set; } = "Cat";

    public string Name { get; set; } = string.Empty;
    public int Health { get; set; } = 80;
    public int Mood { get; set; } = 70;
    public int XP { get; set; }
    public int Level { get; set; } = 1;
    public int Coins { get; set; }
}

/// Serializable snapshot of a TaskItem.</summary>
public class TaskSaveData
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public TaskPriority Priority { get; set; } = TaskPriority.Medium;
    public DateTime DueDate { get; set; } = DateTime.Today;
    public bool IsCompleted { get; set; }
    public DateTime? CompletedAt { get; set; }
}