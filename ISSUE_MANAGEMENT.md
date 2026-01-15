# Issue Management Guide

This document explains how issues are organized and tracked for the Six Degrees project.

## Issue Structure

All issues follow a consistent structure:

### Components

1. **Title**: Clear, action-oriented description
2. **Labels**: Priority, milestone, type, area
3. **Description**: What needs to be done
4. **Tasks**: Checklist of specific subtasks
5. **Technical Details**: Code examples, specifications
6. **Acceptance Criteria**: Definition of "done"
7. **Dependencies**: What blocks this issue, what it blocks
8. **Testing**: How to verify the implementation

## Labels

### Priority
- `priority: high` - Critical path, blocking other work
- `priority: medium` - Important but not blocking
- `priority: low` - Nice to have, can be deferred

### Type
- `type: enhancement` - New feature or improvement
- `type: bug` - Something broken
- `type: documentation` - Documentation updates

### Area
- `area: backend` - C# server-side code
- `area: frontend` - JavaScript/HTML client code
- `area: infrastructure` - Build, CI/CD, tooling

### Status
- `status: blocked` - Waiting on another issue
- (No label) - Ready to work on

### Milestones
- `milestone-1` through `milestone-8` - Groups issues by milestone

## Issue Dependencies

Dependencies are explicitly documented in each issue:

```markdown
## Dependencies
**Blocked by:**
- #8 (Project Setup)

**Blocks:**
- #17 (Search Interface)
```

This creates a clear dependency graph:

```
#8 (Project Setup)
  ├─ blocks → #9 (Graph Data Structures)
  ├─ blocks → #10 (Pre-Commit Hooks)
  └─ blocks → #15 (Unit Testing)

#9 (Graph Data Structures)
  └─ blocks → #11 (Library Scanning)

#11 (Library Scanning)
  ├─ blocks → #12 (BFS Pathfinding)
  ├─ blocks → #13 (N-Degree Expansion)
  └─ blocks → #14 (REST API)

#12, #13 (Pathfinding)
  └─ blocks → #14 (REST API)

#14 (REST API)
  └─ blocks → #16 (Frontend Foundation)
```

## Task Checklists

Each issue contains a detailed checklist:

```markdown
## Tasks
- [x] Completed task
- [ ] Pending task
- [ ] Another pending task
```

Update these as you work by editing the issue on GitHub.

## Working with Issues

### Starting Work

1. **Check dependencies**: Ensure all "Blocked by" issues are closed
2. **Review tasks**: Read all tasks in the checklist
3. **Check acceptance criteria**: Understand what "done" means
4. **Ask questions**: Comment on the issue if anything is unclear

### During Work

1. **Update checklist**: Check off tasks as completed
2. **Comment progress**: Add comments with updates
3. **Link commits**: Reference issue in commits: `Implement graph service (#11)`
4. **Ask for help**: Comment if stuck or need clarification

### Completing Work

1. **Verify acceptance criteria**: Ensure all criteria are met
2. **Check all tasks**: All checklist items should be checked
3. **Test**: Follow testing steps in issue
4. **Document**: Update code comments and docs
5. **Create PR**: Reference issue in PR description: `Closes #11`
6. **Get review**: Request review before merging

## Issue Workflow

```
[ Open ] → [ In Progress ] → [ Review ] → [ Closed ]
   ↓            ↓               ↓
[Blocked]   [Needs Info]   [Changes Requested]
```

### States

- **Open**: Issue is ready to be worked on (dependencies met)
- **Blocked**: Has `status: blocked` label, waiting on dependency
- **In Progress**: Assigned and being worked on
- **Review**: PR created, awaiting review
- **Closed**: Complete, PR merged

## Finding Work

### What to work on next?

1. **Check Milestone 1** first (foundation issues)
2. **Look for unblocked issues** (no `status: blocked` label)
3. **Pick by priority**: `priority: high` first
4. **Check dependencies**: Ensure nothing blocks it

### Current Available Issues

**Ready to start:**
- #8 (Project Setup) - No dependencies, highest priority

**Blocked (don't start yet):**
- All others are blocked by #8

Once #8 is complete:
- #9 (Graph Data Structures)
- #10 (Pre-Commit Hooks)
- #15 (Unit Testing)

## Sub-Issues

Some issues are "parent" issues with sub-issues:

**Example: #11 (Library Scanning)**
```markdown
## Sub-Issues
- [ ] #11.1 - Service initialization
- [ ] #11.2 - Library scanning logic
- [ ] #11.3 - Graph building
- [ ] #11.4 - Serialization
- [ ] #11.5 - Cache validation
```

Create sub-issues as needed to break down complex work.

## Milestones

Issues are grouped into milestones on GitHub:

1. **Milestone 1**: Project Setup & Foundation
2. **Milestone 2**: Relationship Graph Service
3. **Milestone 3**: Pathfinding Algorithms
4. **Milestone 4**: REST API Endpoints
5. **Milestone 5**: Frontend Foundation
6. **Milestone 6**: Search Interface
7. **Milestone 7**: Force-Directed Graph
8. **Milestone 8**: Interactive Features

View all milestones: https://github.com/sydlexius/EmbySixDegrees/milestones

## Best Practices

### Creating New Issues

1. Use the existing issues as templates
2. Include all sections (Description, Tasks, etc.)
3. Add appropriate labels
4. Document dependencies
5. Link to related issues
6. Add acceptance criteria

### Updating Issues

1. Check off tasks as you complete them
2. Add comments with progress updates
3. Update labels if status changes
4. Link related commits/PRs
5. Close when fully complete

### Referencing Issues

**In commits:**
```bash
git commit -m "Implement graph service (#11)"
git commit -m "Fix pathfinding bug (fixes #12)"
```

**In PRs:**
```markdown
Closes #11
Fixes #12
Resolves #13
```

**In comments:**
```markdown
This relates to #8
Blocked by #9
See also #10
```

## GitHub CLI Commands

### Viewing Issues
```bash
# List all open issues
gh issue list

# List issues by label
gh issue list --label "priority: high"

# List issues for milestone
gh issue list --milestone "Milestone 1"

# View issue details
gh issue view 8
```

### Creating Issues
```bash
# Create new issue
gh issue create --title "Title" --body "Body" --label "priority: high"

# Create from template
gh issue create --template bug_report.md
```

### Updating Issues
```bash
# Add label
gh issue edit 8 --add-label "status: blocked"

# Remove label
gh issue edit 8 --remove-label "status: blocked"

# Close issue
gh issue close 8 --comment "Completed and tested"
```

### Commenting
```bash
# Add comment
gh issue comment 8 --body "Work in progress"
```

## Resources

- [GitHub Issues Documentation](https://docs.github.com/en/issues)
- [GitHub CLI Documentation](https://cli.github.com/manual/)
- [Project Milestones](https://github.com/sydlexius/EmbySixDegrees/milestones)
- [All Open Issues](https://github.com/sydlexius/EmbySixDegrees/issues)
