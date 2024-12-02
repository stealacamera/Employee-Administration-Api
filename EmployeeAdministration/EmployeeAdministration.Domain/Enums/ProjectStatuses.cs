using Ardalis.SmartEnum;

namespace EmployeeAdministration.Domain.Enums;

public abstract class ProjectStatuses : SmartEnum<ProjectStatuses, sbyte>
{
    public static readonly ProjectStatuses InProgress = new InProgressStatus(),
                                           Paused = new PausedStatus(),
                                           Finished = new FinishedStatus();

    private static readonly Dictionary<sbyte, ProjectStatuses> _fromId = new()
    {
        { InProgress.Id, InProgress },
        { Paused.Id, Paused },
        { Finished.Id, Finished }
    };

    public static ProjectStatuses FromId(sbyte id)
        => _fromId.ContainsKey(id) ? _fromId[id] : throw new Exception("Invalid enum id");

    private ProjectStatuses(string name, sbyte value) : base(name, value) { }

    public abstract sbyte Id { get; }


    // Enum instances
    private sealed class InProgressStatus : ProjectStatuses
    {
        public InProgressStatus() : base("In progress", 0) { }
        public override sbyte Id => 1;
    }

    private sealed class PausedStatus : ProjectStatuses
    {
        public PausedStatus() : base("Paused", -1) { }
        public override sbyte Id => 2;
    }

    private sealed class FinishedStatus : ProjectStatuses
    {
        public FinishedStatus() : base("Finished", 1) { }
        public override sbyte Id => 3;
    }
}
