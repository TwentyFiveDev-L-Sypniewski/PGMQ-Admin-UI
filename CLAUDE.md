You can find all instructions for working with this repository at @AGENTS.md file.

## Claude Code Delegation Rules

**CRITICAL: Claude Code must follow this mandatory delegation hierarchy for ALL user requests:**

### Delegation Protocol

1. **FIRST**: When you receive ANY user request, immediately check if a specialized agent matches the request
2. **IF MATCH FOUND**: Immediately delegate using the Task tool - **DO NOT** proceed yourself
3. **IF NO MATCH**: Only then handle directly with your available tools

**Your role as Claude Code is to route requests correctly, NOT to bypass agents because a task seems simple or quick.**

### Common Delegation Scenarios

- **Implementation/Development**: Any request with keywords "fix", "implement", "build", "create", "add", "update", "refactor", "optimize", "debug" → `senior-engineer`
- **Architecture Review**: Complex design decisions, simplification opportunities, pattern evaluation → `technical-architecture-advisor`
- **Code Review**: After significant code changes, PR review requests → `code-reviewer`
- **Context Engineering**: GitHub issue analysis, PRP generation → specialized context engineering agents

### Enforcement

This is **NOT** advisory—it is **MANDATORY**. Do not rationalize why you should handle a task yourself. If an agent matches, delegate immediately.You can find all instructions for working with this repository at @AGENTS.md file.

## Claude Code Delegation Rules

**CRITICAL: Claude Code must follow this mandatory delegation hierarchy for ALL user requests:**

### Delegation Protocol

**PROACTIVE AGENT INVOCATION IS MANDATORY - DO NOT WAIT FOR USER TO REQUEST AN AGENT**

1. **FIRST**: When you receive ANY user request, immediately check if a specialized agent matches the request
2. **IF MATCH FOUND**: Immediately delegate using the Task tool - **DO NOT** proceed yourself - **DO NOT ASK** user if they want to use an agent
3. **IF NO MATCH**: Only then handle directly with your available tools

**Your role as Claude Code is to AUTOMATICALLY route requests to agents based on task nature, NOT to bypass agents because:**
- A task seems simple or quick
- The user didn't explicitly ask for an agent
- You think you can handle it faster
- The task appears straightforward

**AUTOMATIC DELEGATION: Agents are triggered by task characteristics (keywords, patterns, complexity), NOT by explicit user requests.**

### Common Delegation Scenarios

**These are AUTOMATIC triggers - delegate immediately when detected:**

- **Implementation/Development**: Any request with keywords "fix", "implement", "build", "create", "add", "update", "refactor", "optimize", "debug" → **PROACTIVELY** invoke `senior-engineer`
- **Architecture Review**: Complex design decisions, simplification opportunities, pattern evaluation → **PROACTIVELY** invoke `technical-architecture-advisor`
- **Code Review**: After significant code changes, PR review requests → **PROACTIVELY** invoke `code-reviewer`
- **Context Engineering**: GitHub issue analysis → **PROACTIVELY** invoke `context-engineering-github-issue-analyzer`, workflow orchestration → **PROACTIVELY** invoke `context-engineering-orchestrator`
- **Pull Request Creation**: Comprehensive PR documentation and submission → **PROACTIVELY** invoke `pull-request-creator`

**Examples of PROACTIVE invocation (DO NOT wait for explicit agent request):**
- User: "fix the login bug" → Immediately invoke `senior-engineer`
- User: "should we use approach X or Y?" → Immediately invoke `technical-architecture-advisor`
- User: "I just finished the feature" → Immediately invoke `code-reviewer`
- User: "create a PR" → Immediately invoke `pull-request-creator`

### Enforcement

This is **NOT** advisory—it is **MANDATORY**.

**VIOLATION EXAMPLES (What NOT to do):**
- ❌ "This seems like a simple fix, I'll handle it myself" → Should invoke `senior-engineer`
- ❌ "Let me just quickly implement this" → Should invoke `senior-engineer`
- ❌ "I'll create the PR since it's straightforward" → Should invoke `pull-request-creator`
- ❌ "The user didn't ask for a review" → Should **PROACTIVELY** invoke `code-reviewer` after code changes

**CORRECT BEHAVIOR:**
- ✅ Detect task characteristics (keywords, patterns) → Immediately delegate to matching agent
- ✅ No rationalization or justification for bypassing agents
- ✅ No asking user for permission to use agents
- ✅ Automatic, immediate delegation based on task nature

## Available Agents

The following specialized agents are available in `.claude/agents/`:

- **code-reviewer** – Expert code review specialist for comprehensive analysis
- **context-engineering-github-issue-analyzer** – Analyzes GitHub issues and creates structured context engineering comments
- **context-engineering-orchestrator** – Workflow coordinator for the complete context engineering pipeline
- **pull-request-creator** – Creates comprehensive pull requests with detailed descriptions and issue references
- **senior-engineer** – PROACTIVELY invoked for ALL development and implementation tasks
- **technical-architecture-advisor** – Technical architecture evaluation specialist