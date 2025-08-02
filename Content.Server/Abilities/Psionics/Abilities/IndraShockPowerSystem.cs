using Content.Shared.Abilities.Psionics;
using Content.Shared.StatusEffect;
using Content.Server.Stunnable;
using Content.Server.Beam;
using Content.Shared.Actions.Events;
using Content.Shared.Mobs.Systems;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Psionics.Glimmer;
using Content.Shared.Traits.Assorted.Components;
using Content.Shared.Damage;

namespace Content.Server.Abilities.Psionics
{
    public sealed class IndraShockPowerSystem : EntitySystem
    {
        [Dependency] private readonly SharedPsionicAbilitiesSystem _psionics = default!;
        [Dependency] private readonly StunSystem _stunSystem = default!;
        [Dependency] private readonly StatusEffectsSystem _statusEffectsSystem = default!;
        [Dependency] private readonly BeamSystem _beam = default!;
        [Dependency] private readonly DamageableSystem _damageable = default!;
        [Dependency] private readonly GlimmerSystem _glimmer = default!;

        public override void Initialize()
        {
            base.Initialize();
            
            SubscribeLocalEvent<IndraShockPowerActionEvent>(OnPowerUsed);
        }

        private void OnPowerUsed(EntityUid uid, IndraShockPowerActionEvent args, PsionicComponent component)
        {
            if (!_psionics.OnAttemptPowerUse(args.Performer, "Indra Shock"))
                return;

            args.ModifiedAmplification = _psionics.ModifiedAmplification(uid, component);
            args.ModifiedDampening = _psionics.ModifiedDampening(uid, component);

            if (component is null)
                return;

            if (!TryComp<DamageableComponent>(args.Target, out var damageableComponent))
                return;

            if (args.HealingAmount is not null)
                _damageable.TryChangeDamage(args.Target, args.HealingAmount * args.ModifiedAmplification, true, false, damageableComponent, uid);
            
            _beam.TryCreateBeam(args.Performer, args.Target, "LightningNoospheric");

            _stunSystem.TryParalyze(args.Target, TimeSpan.FromSeconds(2.5), false);
            _statusEffectsSystem.TryAddStatusEffect(args.Target, "Stutter", TimeSpan.FromSeconds(5), false, "StutteringAccent");

            _psionics.LogPowerUsed(args.Performer, "Indra Shock");
            args.Handled = true;
        }
    }    
}
