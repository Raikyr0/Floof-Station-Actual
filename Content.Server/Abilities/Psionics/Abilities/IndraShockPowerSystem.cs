using Content.Shared.Abilities.Psionics;
using Content.Shared.StatusEffect;
using Content.Server.Stunnable;
using Content.Server.Beam;
using Content.Shared.Actions.Events;

namespace Content.Server.Abilities.Psionics
{
    public sealed class IndraShockPowerSystem : EntitySystem
    {
        [Dependency] private readonly SharedPsionicAbilitiesSystem _psionics = default!;
        [Dependency] private readonly StunSystem _stunSystem = default!;
        [Dependency] private readonly StatusEffectsSystem _statusEffectsSystem = default!;
        [Dependency] private readonly BeamSystem _beam = default!;


        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<IndraShockPowerActionEvent>(OnPowerUsed);
        }

        private void OnPowerUsed(IndraShockPowerActionEvent args)
        {
            if (!_psionics.OnAttemptPowerUse(args.Performer, "Indra Shock"))
                return;

            args.ModifiedAmplification = _psionics.ModifiedAmplification(uid, component);
            args.ModifiedDampening = _psionics.ModifiedDampening(uid, component);

            if (!args.Immediate)
                AttemptDoAfter(uid, component, args);
            else ActivatePower(uid, component, args);
            
            _beam.TryCreateBeam(args.Performer, args.Target, "LightningNoospheric");

            _stunSystem.TryParalyze(args.Target, TimeSpan.FromSeconds(2.5), false);
            _statusEffectsSystem.TryAddStatusEffect(args.Target, "Stutter", TimeSpan.FromSeconds(5), false, "StutteringAccent");

            _psionics.LogPowerUsed(args.Performer, "Indra Shock");
            args.Handled = true;
        }
        
        private void AttemptDoAfter(EntityUid uid, PsionicComponent component, IndraShockPowerActionEvent args)
        {
            var ev = new IndraShockDoAfterEvent(_gameTiming.CurTime);
            if (args.HealingAmount is not null)
                ev.HealingAmount = args.HealingAmount;
            
            ev.ModifiedAmplification = args.ModifiedAmplification;
            ev.ModifiedDampening = args.ModifiedDampening;
        }

        private void OnDoAfter(EntityUid uid, PsionicComponent component, IndraShockPowerDoAfterEvent args)
        {
            // It's entirely possible for the caster to stop being Psionic(due to mindbreaking) mid cast
            if (component is null)
                return;
            component.DoAfter = null;

            // The target can also cease existing mid-cast
            // Or the DoAfter is cancelled(such as if the caster moves).
            if (args.Target is null
                || args.Cancelled)
                return;

            if (!TryComp<DamageableComponent>(args.Target.Value, out var damageableComponent))
                return;

            if (args.HealingAmount is not null)
                _damageable.TryChangeDamage(args.Target.Value, args.HealingAmount * args.ModifiedAmplification, true, false, damageableComponent, uid);
        }
        
        // This would be the same thing as OnDoAfter, except that here the target isn't nullable, so I have to reuse code with different arguments.
        private void ActivatePower(EntityUid uid, PsionicComponent component, IndraShockPowerActionEvent args)
        {
            if (component is null)
                return;

            if (!TryComp<DamageableComponent>(args.Target, out var damageableComponent))
                return;

            if (args.HealingAmount is not null)
                _damageable.TryChangeDamage(args.Target, args.HealingAmount * args.ModifiedAmplification, true, false, damageableComponent, uid);
        }
    }    
}
