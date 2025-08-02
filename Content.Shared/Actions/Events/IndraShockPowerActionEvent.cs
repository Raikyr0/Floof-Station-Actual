using Content.Shared.Damage;

namespace Content.Shared.Actions.Events;
public sealed partial class IndraShockPowerActionEvent : EntityTargetActionEvent 
{
    /// <summary>
    ///     Caster's Amplification that has been modified by the results of a MoodContest.
    /// </summary>
    public float ModifiedAmplification = default!;

    /// <summary>
    ///     Caster's Dampening that has been modified by the results of a MoodContest.
    /// </summary>
    public float ModifiedDampening = default!;

    [DataField]
    public DamageSpecifier? HealingAmount = default!;

    /// Controls whether or not a power fires immediately and with no DoAfter
    [DataField]
    public bool Immediate;
}
