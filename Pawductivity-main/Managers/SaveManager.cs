using System.Text.Json;
using System.Text.Json.Serialization;
using Pawductivity.Models;

namespace Pawductivity.Persistence;

/// Handles all disk I/O for Pawductivity save data.
public static class SaveManager
{
    // ── Directory ────────────────────────────────────────────────────
    private static readonly string SaveDir =
        Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Pawductivity");

    // ── JSON options ─────────────────────────────────────────────────
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() },
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    // ── Helpers ──────────────────────────────────────────────────────
    /// Converts a username to a safe filename (lowercase, strips path chars).
    /// "Marie" → "marie.json"
    private static string FileFor(string username)
    {
        // Remove any characters that are illegal in file names
        var safe = string.Concat(username
            .ToLowerInvariant()
            .Select(c => Path.GetInvalidFileNameChars().Contains(c) ? '_' : c));

        return Path.Combine(SaveDir, safe + ".json");
    }

    // ── Public API ───────────────────────────────────────────────────
    /// Returns all usernames that have a save file on disk,
    /// sorted alphabetically. Returns an empty array if none exist yet.
    public static string[] ListProfiles()
    {
        if (!Directory.Exists(SaveDir))
            return [];

        return Directory.GetFiles(SaveDir, "*.json")
            .Select(path =>
            {
                // Try to read the UserName field from each file so the
                // display name matches what the user originally typed
                try
                {
                    var json = File.ReadAllText(path);
                    var data = JsonSerializer.Deserialize<SaveData>(json, JsonOpts);
                    return data?.UserName ?? Path.GetFileNameWithoutExtension(path);
                }
                catch
                {
                    return Path.GetFileNameWithoutExtension(path);
                }
            })
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .OrderBy(name => name, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    /// Returns true if a save file already exists for this username.
    public static bool ProfileExists(string username) =>
        File.Exists(FileFor(username));

    /// Writes the current game state to disk under the user's own file.
    /// Uses an atomic temp-then-rename write to prevent corruption on crash.
    public static void Save(Managers.GameManager gm)
    {
        try
        {
            Directory.CreateDirectory(SaveDir);

            var data = Snapshot(gm);
            string json = JsonSerializer.Serialize(data, JsonOpts);
            string path = FileFor(gm.UserName);
            string tmp = path + ".tmp";

            File.WriteAllText(tmp, json);
            File.Move(tmp, path, overwrite: true);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[SaveManager] Save failed: {ex.Message}");
        }
    }

    /// Loads the save file for the given username.
    /// Returns null if the file doesn't exist or is unreadable (treat as new game).
    public static SaveData? Load(string username)
    {
        string path = FileFor(username);
        if (!File.Exists(path)) return null;

        try
        {
            string json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<SaveData>(json, JsonOpts);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[SaveManager] Load failed: {ex.Message}");
            return null;
        }
    }

    /// Permanently deletes the save file for the given username.
    public static void DeleteProfile(string username)
    {
        string path = FileFor(username);
        if (File.Exists(path))
            File.Delete(path);
    }

    // ── Snapshot (live state → SaveData) ────────────────────────────
    private static SaveData Snapshot(Managers.GameManager gm) => new()
    {
        UserName = gm.UserName,
        TotalCompleted = gm.TotalCompleted,
        CurrentStreak = gm.CurrentStreak,
        LongestStreak = gm.LongestStreak,
        LastCompletionDate = gm.LastCompletionDate,

        Pet = new PetSaveData
        {
            PetType = gm.Pet is CatPet ? "Cat" : "Dog",
            Name = gm.Pet.Name,
            Health = gm.Pet.Health,
            Mood = gm.Pet.Mood,
            XP = gm.Pet.XP,
            Level = gm.Pet.Level,
            Coins = gm.Pet.Coins,
        },

        Tasks = gm.Tasks.Select(t => new TaskSaveData
        {
            Id = t.Id,
            Title = t.Title,
            Description = t.Description,
            Priority = t.Priority,
            DueDate = t.DueDate,
            IsCompleted = t.IsCompleted,
            CompletedAt = t.CompletedAt,
        }).ToList(),
    };

    // ── Restore (SaveData → live GameManager) ───────────────────────
    /// Reconstructs a fully-populated <see cref="Managers.GameManager"/>
    /// from a <see cref="SaveData"/> snapshot.
    public static Managers.GameManager Restore(SaveData data)
    {
        // Rebuild the correct Pet subclass
        Pet pet = data.Pet.PetType == "Dog"
            ? new DogPet(data.Pet.Name)
            : new CatPet(data.Pet.Name);

        pet.RestoreStats(
            data.Pet.Health,
            data.Pet.Mood,
            data.Pet.XP,
            data.Pet.Level,
            data.Pet.Coins);

        var tasks = data.Tasks.Select(t => new TaskItem
        {
            Id = t.Id,
            Title = t.Title,
            Description = t.Description,
            Priority = t.Priority,
            DueDate = t.DueDate,
            IsCompleted = t.IsCompleted,
            CompletedAt = t.CompletedAt,
        }).ToList();

        var gm = new Managers.GameManager(pet) { UserName = data.UserName };

        gm.RestoreProgress(
            tasks,
            data.TotalCompleted,
            data.CurrentStreak,
            data.LongestStreak,
            data.LastCompletionDate);

        return gm;
    }
}
