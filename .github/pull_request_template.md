## What

<!-- What does this change and why? Link the issue if there is one. -->

## How to check

<!-- Steps for a reviewer: scene to open, keys to press, test to run. -->

- [ ] Tests pass locally (`godot --headless --fixed-fps 60 --path . res://tests/TestRunner.tscn`)
- [ ] New behaviour has a test, or the PR says why not
- [ ] No edits inside `addons/dof_*` (vendored: change godot-systems, then run `tools/sync-systems.sh`)
- [ ] New or changed labs are in `LabCatalog` and covered in `tests/LabTests.cs`
