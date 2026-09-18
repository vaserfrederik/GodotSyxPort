# A96 — source-priority settler AI dispatcher

The C# citizen loop previously chose new behaviour through a fixed sequence.
When work was available it claimed a job before home, service and subject
activities were considered. This was not the Java `AIModules` model and made
several already-ported plans appear to be placeholders.

## Ported from Java

- Added a non-rendering equivalent of `AIModules.Sorter2`.
- Candidate modules are attempted by source priority rather than C# method
  order: critical food 10, health 7, high food need 6, work 5, low food need 4,
  services 3, subject activity 2 and normal home activity 1.
- Equal priorities use a stable citizen/day rotation, corresponding to the
  randomized module offset used by `Sorter2` without tying simulation results
  to render-frame random calls.
- Work strikes now suppress only the work module. They no longer freeze food,
  services, home, mourning and idle interaction.
- Active disease or dangerous injury interrupts ordinary work at health
  priority 7. Reserved work and carried resources are released through the
  existing job/hauling systems before hospital or home recovery begins.
- Food interruption uses the same cancellation path, including carried-resource
  cleanup and construction-batch release.
- Work claiming was moved into the work module branch. A missing work plan now
  falls through to lower-priority service, subject, home or idle plans, as
  `AIModules.getNextPlan` does.
- Hospital recovery falls back to a home/ground recovery plan when a hospital
  plan cannot be activated.

## Source correspondence

- `settlement.entity.humanoid.ai.main.AIModules`
- `settlement.entity.humanoid.ai.work.AIModule_Work`
- `settlement.entity.humanoid.ai.consume.AIModule_Food`
- `settlement.entity.humanoid.ai.danger.AIModule_Health`
- `settlement.entity.humanoid.ai.home.AIModule_Home`
- `settlement.entity.humanoid.ai.service.AIModule_Service`
- `settlement.entity.humanoid.ai.subject.AIModule_Subject`

This checkpoint ports the dispatcher and interruption semantics. Individual
specialized plans (guards, prisoners, recruits, exposure, crime animations and
wild-animal hunting) remain separate semantic ports.

`python3 Tools/verify_port.py` returns `PORT_STATIC_VERIFICATION_OK`.
Per project workflow, no Godot/.NET compilation was run.
